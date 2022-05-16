using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using NBagOfTricks;
using NBagOfUis;
using MidiLib;



namespace ClipExplorer
{
    public partial class MainForm : Form
    {
        #region Fields
        /// <summary>Supported file types..</summary>
        readonly string[] _fileTypes = new[] { ".mid", ".wav", ".mp3", ".m4a", ".flac" };

        /// <summary>Audio device.</summary>
        WavePlayer? _wavePlayer = null;

        /// <summary>Midi device.</summary>
        MidiPlayer? _midiPlayer = null;

        /// <summary>Current file.</summary>
        string _fn = "";

        /// <summary>Current play device.</summary>
        IPlayer? _player = null;
        #endregion

        #region Lifecycle
        /// <summary>
        /// Constructor.
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initialize form controls.
        /// </summary>
        void MainForm_Load(object? sender, EventArgs e)
        {
            // Get settings and set up paths.
            string appDir = MiscUtils.GetAppDataDir("ClipExplorer", "Ephemera");
            Common.Settings = (UserSettings)Settings.Load(appDir, typeof(UserSettings));
            Common.Settings.ExportPath = Path.Combine(appDir, "export");
            DirectoryInfo di = new(Common.Settings.ExportPath);
            di.Create();

            toolStrip1.Renderer = new NBagOfUis.CheckBoxRenderer() { SelectedColor = Common.Settings.ControlColor };

            // Toolbar configs.
            btnAutoplay.Checked = Common.Settings.Autoplay;
            btnLoop.Checked = Common.Settings.Loop;

            // The text output.
            txtViewer.Font = Font;
            txtViewer.WordWrap = true;
            txtViewer.BackColor = Color.Cornsilk;
            txtViewer.Colors.Add("ERR", Color.LightPink);
            txtViewer.Colors.Add("WRN:", Color.Plum);

            // Create devices.
            _wavePlayer = new WavePlayer() { Visible = false, Volume = Common.Settings.Volume };
            _wavePlayer.PlaybackCompleted += Player_PlaybackCompleted;
            _wavePlayer.Log += (sdr, args) => { LogMessage(sdr, args.Category, args.Message); };
            _wavePlayer.Location = new(chkPlay.Left, chkPlay.Bottom + 5);
            splitContainer1.Panel2.Controls.Add(_wavePlayer);

            _midiPlayer = new MidiPlayer() { Visible = false, Volume = Common.Settings.Volume };
            _midiPlayer.PlaybackCompleted += Player_PlaybackCompleted;
            _midiPlayer.Log += (sdr, args) => { LogMessage(sdr, args.Category, args.Message); };
            _midiPlayer.Location = new(chkPlay.Left, chkPlay.Bottom + 5);
            splitContainer1.Panel2.Controls.Add(_midiPlayer);

            // Init UI from settings
            Location = new Point(Common.Settings.FormGeometry.X, Common.Settings.FormGeometry.Y);
            Size = new Size(Common.Settings.FormGeometry.Width, Common.Settings.FormGeometry.Height);
            WindowState = FormWindowState.Normal;
            KeyPreview = true; // for routing kbd strokes through MainForm_KeyDown
            sldVolume.Value = Common.Settings.Volume;
            sldVolume.DrawColor = Common.Settings.ControlColor;

            InitNavigator();

            // Hook up UI handlers.
            chkPlay.CheckedChanged += (_, __) => { _ = chkPlay.Checked ? Play() : Stop(); };
            btnRewind.Click += (_, __) => { Rewind(); };

            Text = $"Clip Explorer {MiscUtils.GetVersionString()} - No file loaded";
        }

        /// <summary>
        /// Clean up on shutdown. Dispose() will get the rest.
        /// </summary>
        void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            SaveSettings();

            _wavePlayer?.Dispose();
            _midiPlayer?.Dispose();
        }
        #endregion

        #region User settings
        /// <summary>
        /// Collect and save user settings.
        /// </summary>
        void SaveSettings()
        {
            Common.Settings.Autoplay = btnAutoplay.Checked;
            Common.Settings.Loop = btnLoop.Checked;
            Common.Settings.Volume = sldVolume.Value;
            Common.Settings.FormGeometry = new Rectangle(Location.X, Location.Y, Size.Width, Size.Height);

            Common.Settings.Save();
        }

        /// <summary>
        /// Edit the common options in a property grid.
        /// </summary>
        void Settings_Click(object? sender, EventArgs e)
        {
            var changes = Common.Settings.Edit("User Settings");

            // Detect changes of interest.
            bool midiChange = false;
            bool audioChange = false;
            bool navChange = false;
            bool restart = false;

            foreach (var (name, cat) in changes)
            {
                restart |= name.EndsWith("Device");
                restart |= cat == "Cosmetics";
                midiChange |= cat == "Midi";
                audioChange |= cat == "Audio";
                navChange |= cat == "Navigator";
            }

            if (restart)
            {
                MessageBox.Show("Restart required for device changes to take effect");
            }

            if ((midiChange && _player is MidiPlayer) || (audioChange && _player is WavePlayer))
            {
                _player.SettingsChanged();
            }

            if (navChange)
            {
                InitNavigator();
            }

            SaveSettings();
        }
        #endregion

        #region Info
        /// <summary>
        /// All about me.
        /// </summary>
        void About_Click(object? sender, EventArgs e)
        {
            MiscUtils.ShowReadme("ClipExplorer");
        }

        /// <summary>
        /// Something you should know.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ea"></param>
        void LogMessage(object? sender, string cat, string msg)
        {
            int catSize = 3;
            cat = cat.Length >= catSize ? cat.Left(catSize) : cat.PadRight(catSize);

            // May come from a different thread.
            this.InvokeIfRequired(_ =>
            {
                //string s = $"{DateTime.Now:mm\\:ss\\.fff} {cat} ({((Control)sender!).Name}) {msg}";
                string s = $"> {cat} ({((Control)sender!).Name}) {msg}";
                txtViewer.AppendLine(s);
            });
        }
        #endregion

        #region File handling
        /// <summary>
        /// Organize the file drop down.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void File_DropDownOpening(object? sender, EventArgs e)
        {
            fileDropDownButton.DropDownItems.Clear();

            // Always:
            fileDropDownButton.DropDownItems.Add(new ToolStripMenuItem("Open...", null, Open_Click));
            fileDropDownButton.DropDownItems.Add(new ToolStripSeparator());

            Common.Settings.RecentFiles.ForEach(f =>
            {
                ToolStripMenuItem menuItem = new(f, null, new EventHandler(Recent_Click));
                fileDropDownButton.DropDownItems.Add(menuItem);
            });
        }

        /// <summary>
        /// The user has asked to open a recent file.
        /// </summary>
        void Recent_Click(object? sender, EventArgs e)
        {
            //ToolStripMenuItem item = sender as ToolStripMenuItem;
            string fn = sender!.ToString()!;
            if (fn != _fn)
            {
                OpenFile(fn);
                _fn = fn;
            }
        }

        /// <summary>
        /// Allows the user to select an audio clip or midi from file system.
        /// </summary>
        void Open_Click(object? sender, EventArgs e)
        {
            string sext = "Clip Files | ";
            foreach (string ext in _fileTypes)
            {
                sext += $"*{ext}; ";
            }

            using OpenFileDialog openDlg = new()
            {
                Filter = sext,
                Title = "Select a file"
            };

            if (openDlg.ShowDialog() == DialogResult.OK && openDlg.FileName != _fn)
            {
                OpenFile(openDlg.FileName);
                _fn = openDlg.FileName;
            }
        }

        /// <summary>
        /// Common file opener.
        /// </summary>
        /// <param name="fn">The file to open.</param>
        /// <returns>Status.</returns>
        public bool OpenFile(string fn)
        {
            bool ok = true;

            chkPlay.Checked = false; // ==> stop

            LogMessage(this, "INF", $"Opening file: {fn}");

            using (new WaitCursor())
            {
                try
                {
                    if (File.Exists(fn))
                    {
                        switch (Path.GetExtension(fn).ToLower())
                        {
                            case ".wav":
                            case ".mp3":
                            case ".m4a":
                            case ".flac":
                                _wavePlayer!.Visible = true;
                                _midiPlayer!.Visible = false;
                                _player = _wavePlayer;
                                break;

                            case ".mid":
                                _wavePlayer!.Visible = false;
                                _midiPlayer!.Visible = true;
                                _player = _midiPlayer;
                                break;

                            default:
                                LogMessage(this, "ERR", $"Invalid file type: {fn}");
                                ok = false;
                                break;
                        }

                        if (ok)
                        {
                            ok = _player!.OpenFile(fn);
                            if (Common.Settings.Autoplay)
                            {
                                chkPlay.Checked = true; // ==> run
                            }
                        }
                    }
                    else
                    {
                        LogMessage(this, "ERR", $"Invalid file: {fn}");
                        ok = false;
                    }
                }
                catch (Exception ex)
                {
                    LogMessage(this, "ERR", $"Couldn't open the file: {fn} because: {ex.Message}");
                    ok = false;
                }
            }

            if (ok)
            {
                Text = $"ClipExplorer {MiscUtils.GetVersionString()} - {fn}";
                Common.Settings.RecentFiles.UpdateMru(fn);
            }
            else
            {
                Text = $"ClipExplorer {MiscUtils.GetVersionString()} - No file loaded";
            }

            return ok;
        }
        #endregion

        #region Transport control
        /// <summary>
        /// Internal handler.
        /// </summary>
        /// <returns></returns>
        bool Stop()
        {
            _player?.Stop();
            return true;
        }

        /// <summary>
        /// Internal handler.
        /// </summary>
        /// <returns></returns>
        bool Play()
        {
            _player?.Rewind();
            _player?.Play();
            return true;
        }

        /// <summary>
        /// Go back Jack.
        /// </summary>
        public void Rewind()
        {
            // Might come from another thread.
            this.InvokeIfRequired(_ =>
            {
                _player!.Rewind();
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Player_PlaybackCompleted(object? sender, EventArgs e)
        {
            // Usually comes from a different thread.
            this.InvokeIfRequired(_ =>
            {
                if (btnLoop.Checked)
                {
                    Play();
                }
                else
                {
                    chkPlay.Checked = false;
                    Rewind();
                }
            });
        }

        /// <summary>
        /// Do some global key handling. Space bar is used for stop/start playing.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MainForm_KeyDown(object? sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Space:
                    // Toggle.
                    chkPlay.Checked = !chkPlay.Checked;
                    e.Handled = true;
                    break;

                case Keys.C:
                    txtViewer.Clear();
                    e.Handled = true;
                    break;

                case Keys.W:
                    txtViewer.WordWrap = !txtViewer.WordWrap;
                    e.Handled = true;
                    break;
            }
        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //void Rewind_Click(object? sender, EventArgs e)
        //{
        //    Stop();
        //    _player!.Rewind();
        //}
        #endregion

        #region Navigator functions
        /// <summary>
        /// Initialize tree from user settings.
        /// </summary>
        void InitNavigator()
        {
            ftree.FilterExts = _fileTypes.ToList();
            ftree.RootDirs = Common.Settings.RootDirs;
            ftree.SingleClickSelect = true;

            ftree.Init();
        }

        /// <summary>
        /// Tree has seleccted a file to play.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="fn"></param>
        void Navigator_FileSelectedEvent(object? sender, string fn)
        {
            OpenFile(fn);
            _fn = fn;
        }
        #endregion

        #region Misc handlers
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Volume_ValueChanged(object? sender, EventArgs e)
        {
            float vol = (float)sldVolume.Value;
            Common.Settings.Volume = vol;
            if (_player is null)
            {
                _midiPlayer!.Volume = vol;
                _wavePlayer!.Volume = vol;
            }
            else
            {
                _player.Volume = vol;
            }
        }
        #endregion
    }
}
