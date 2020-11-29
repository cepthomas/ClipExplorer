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
using NBagOfTricks.UI;
using NBagOfTricks.Utils;


namespace ClipExplorer
{
    public partial class MainForm : Form
    {
        #region Fields
        /// <summary>Supported file types.</summary>
        readonly string _fileExts = ".wav;.mp3;.mid;";

        /// <summary>Audio device.</summary>
        WavePlayer _wavePlayer = null;

        /// <summary>Midi device.</summary>
        MidiPlayer _midiPlayer = null;

        /// <summary>Current play device.</summary>
        IPlayer _player = null;
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
        void MainForm_Load(object sender, EventArgs e)
        {
            // Get the settings.
            string appDir = MiscUtils.GetAppDataDir("ClipExplorer");
            DirectoryInfo di = new DirectoryInfo(appDir);
            di.Create();
            UserSettings.Load(appDir);

            // Create devices.
            _wavePlayer = new WavePlayer
            {
                Visible = false
            };
            _wavePlayer.PlaybackCompleted += Player_PlaybackCompleted;
            _wavePlayer.Log += Player_Log;
            _wavePlayer.Location = new Point(timeBar.Left, timeBar.Bottom + 5);
            splitContainer1.Panel2.Controls.Add(_wavePlayer);

            _midiPlayer = new MidiPlayer
            {
                Visible = false
            };
            _midiPlayer.PlaybackCompleted += Player_PlaybackCompleted;
            _midiPlayer.Log += Player_Log;
            _midiPlayer.Location = new Point(timeBar.Left, timeBar.Bottom + 5);
            splitContainer1.Panel2.Controls.Add(_midiPlayer);

            // Init UI from settings
            Location = new Point(Common.Settings.MainFormInfo.X, Common.Settings.MainFormInfo.Y);
            Size = new Size(Common.Settings.MainFormInfo.Width, Common.Settings.MainFormInfo.Height);
            WindowState = FormWindowState.Normal;
            KeyPreview = true; // for routing kbd strokes through MainForm_KeyDown
            sldVolume.Value = Common.Settings.Volume;

            timeBar.ProgressColor = Color.LightCyan;

            InitNavigator();

            Text = $"Clip Explorer {MiscUtils.GetVersionString()} - No file loaded";
            timer1.Enabled = true;

            ///// for testing only
            //OpenFile(@"C:\Dev\repos\ClipExplorer\_files\one-sec.wav");
            //OpenFile(@"C:\Dev\repos\ClipExplorer\_files\WICKGAME.MID");
            //var v = player._sourceEvents;
            //DumpMidi(v, "dump.txt");
        }

        /// <summary>
        /// Clean up on shutdown. Dispose() will get the rest.
        /// </summary>
        void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveSettings();

            _wavePlayer.Dispose();
            _midiPlayer.Dispose();
        }
        #endregion

        #region User settings
        /// <summary>
        /// Collect and save user settings.
        /// </summary>
        void SaveSettings()
        {
            Common.Settings.AllTags = ftree.AllTags.ToList();
            Common.Settings.Autoplay = !ftree.DoubleClickSelect;

            Common.Settings.TaggedPaths.Clear();
            ftree.TaggedPaths.ForEach(v => Common.Settings.TaggedPaths[v.path] = v.tags);

            Common.Settings.Volume = sldVolume.Value;
            Common.Settings.MainFormInfo = new Rectangle(Location.X, Location.Y, Width, Height);

            Common.Settings.Save();
        }

        /// <summary>
        /// Edit the common options in a property grid.
        /// </summary>
        void Settings_Click(object sender, EventArgs e)
        {
            using (Form f = new Form()
            {
                Text = "User Settings",
                Size = new Size(450, 450),
                StartPosition = FormStartPosition.Manual,
                Location = new Point(200, 200),
                FormBorderStyle = FormBorderStyle.FixedToolWindow,
                ShowIcon = false,
                ShowInTaskbar = false
            })
            {
                PropertyGridEx pg = new PropertyGridEx()
                {
                    Dock = DockStyle.Fill,
                    PropertySort = PropertySort.NoSort,
                    SelectedObject = Common.Settings
                };

                // Detect changes of interest.
                bool midiChange = false;
                bool audioChange = false;
                bool navChange = false;
                bool restart = false;

                pg.PropertyValueChanged += (sdr, args) =>
                {
                    restart |= args.ChangedItem.PropertyDescriptor.Name.EndsWith("Device");
                    midiChange |= args.ChangedItem.PropertyDescriptor.Category == "Midi";
                    audioChange |= args.ChangedItem.PropertyDescriptor.Category == "Audio";
                    navChange |= args.ChangedItem.PropertyDescriptor.Category == "Navigator";
                };

                f.Controls.Add(pg);
                f.ShowDialog();

                // Figure out what changed - each handled differently.
                if(restart)
                {
                    MessageBox.Show("Restart required for device changes to take effect");
                }

                if ((midiChange && _player is MidiPlayer) || (audioChange && _player is WavePlayer))
                {
                    _player.SettingsUpdated();
                }

                if(navChange)
                {
                    InitNavigator();
                }

                SaveSettings();
            }
        }
        #endregion

        #region Info
        /// <summary>
        /// All about me.
        /// </summary>
        /// <param name="s"></param>
        void ErrorMessage(string s)
        {
            MessageBox.Show(s);
        }

        /// <summary>
        /// All about me.
        /// </summary>
        void About_Click(object sender, EventArgs e)
        {
            // Make some markdown.
            List<string> mdText = new List<string>();

            // Main help file.
            mdText.Add(File.ReadAllText(@"Resources\README.md"));

            // Put it together.
            List<string> htmlText = new List<string>
            {
                // Boilerplate
                $"<!DOCTYPE html><html><head><meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">",
                $"<style>body {{ background-color: white; font-family: \"Arial\", Helvetica, sans-serif; }}",
                $"</style></head><body>"
            };

            // Meat.
            string mdHtml = string.Join(Environment.NewLine, mdText);
            htmlText.Add(mdHtml);

            // Bottom.
            string ss = "<!-- Markdeep: --><style class=\"fallback\">body{visibility:hidden;white-space:pre;font-family:monospace}</style><script src=\"markdeep.min.js\" charset=\"utf-8\"></script><script src=\"https://casual-effects.com/markdeep/latest/markdeep.min.js\" charset=\"utf-8\"></script><script>window.alreadyProcessedMarkdeep||(document.body.style.visibility=\"visible\")</script>";
            htmlText.Add(ss);
            htmlText.Add($"</body></html>");

            string fn = Path.Combine(Path.GetTempPath(), "ClipExplorer.html");
            File.WriteAllText(fn, string.Join(Environment.NewLine, htmlText));
            Process.Start(fn);
        }

        /// <summary>
        /// Log helper.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Player_Log(object sender, string e)
        {
            rtbInfo.AppendText($"Player:{e}{Environment.NewLine}");
        }
        #endregion

        #region File handling
        /// <summary>
        /// Organize the file drop down.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void File_DropDownOpening(object sender, EventArgs e)
        {
            // Take a reference to add back in later.
            var op = openToolStripMenuItem;

            fileDropDownButton.DropDownItems.Clear();
            fileDropDownButton.DropDownItems.Add(op);

            Common.Settings.RecentFiles.ForEach(f =>
            {
                ToolStripMenuItem menuItem = new ToolStripMenuItem(f, null, new EventHandler(Recent_Click));
                fileDropDownButton.DropDownItems.Add(menuItem);
            });
        }

        /// <summary>
        /// The user has asked to open a recent file.
        /// </summary>
        void Recent_Click(object sender, EventArgs e)
        {
            //ToolStripMenuItem item = sender as ToolStripMenuItem;
            string fn = sender.ToString();
            OpenFile(fn);
        }

        /// <summary>
        /// Allows the user to select an audio clip or midi from file system.
        /// </summary>
        void Open_Click(object sender, EventArgs e)
        {
            string sext = "Clip Files | ";
            foreach(string ext in _fileExts.SplitByToken(";"))
            {
                sext += ($"*{ext}; ");
            }
            
            OpenFileDialog openDlg = new OpenFileDialog()
            {
                Filter = sext,
                Title = "Select a file"
            };

            if (openDlg.ShowDialog() == DialogResult.OK)
            {
                OpenFile(openDlg.FileName);
            }

            openDlg.Dispose();
        }

        /// <summary>
        /// Common file opener.
        /// </summary>
        /// <param name="fn">The file to open.</param>
        /// <returns>Status.</returns>
        public bool OpenFile(string fn)
        {
            bool ok = true;
            Stop();

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
                                _wavePlayer.Visible = true;
                                _midiPlayer.Visible = false;
                                _player = _wavePlayer;
                                break;

                            case ".mid":
                                _wavePlayer.Visible = false;
                                _midiPlayer.Visible = true;
                                _player = _midiPlayer;
                                break;

                            default:
                                ErrorMessage($"Invalid file type: {fn}");
                                ok = false;
                                break;
                        }

                        if(ok)
                        {
                            ok = _player.OpenFile(fn);
                            if(Common.Settings.Autoplay)
                            {
                                Start();
                            }
                        }
                    }
                    else
                    {
                        ErrorMessage($"Invalid file: {fn}");
                        ok = false;
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage($"Couldn't open the file: {fn} because: {ex.Message}");
                    ok = false;
                }
            }

            if (ok)
            {
                Text = $"ClipExplorer {MiscUtils.GetVersionString()} - {fn}";
                Common.Settings.RecentFiles.UpdateMru(fn);
                timeBar.Length = MiscUtils.SecondsToTimeSpan(_player.Length);
            }
            else
            {
                Text = $"ClipExplorer {MiscUtils.GetVersionString()} - No file loaded";
            }

            return ok;
        }
        #endregion

        #region Transport
        /// <summary>
        /// Internal handler.
        /// </summary>
        /// <returns></returns>
        bool Stop()
        {
            _player?.Stop();
            SetPlayCheck(false);
            return true;
        }

        /// <summary>
        /// Internal handler.
        /// </summary>
        /// <returns></returns>
        bool Start()
        {
            _player?.Start();
            SetPlayCheck(true);
            return true;
        }

        /// <summary>
        /// Need to temporarily suppress CheckedChanged event.
        /// </summary>
        /// <param name="on"></param>
        void SetPlayCheck(bool on)
        {
            chkPlay.CheckedChanged -= Play_CheckedChanged;
            chkPlay.Checked = on;
            chkPlay.CheckedChanged += Play_CheckedChanged;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Play_CheckedChanged(object sender, EventArgs e)
        {
            bool _ = chkPlay.Checked ? Start() : Stop();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Player_PlaybackCompleted(object sender, EventArgs e)
        {
            // Comes from a different thread.
            this.InvokeIfRequired(o =>
            {
                bool _ = chkLoop.Checked ? Start() : Stop();
            });
        }

        /// <summary>
        /// Do some global key handling. Space bar is used for stop/start playing.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                // Toggle.
                bool _ = chkPlay.Checked ? Stop() : Start();
                e.Handled = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Rewind_Click(object sender, EventArgs e)
        {
            Stop();
            _player.Rewind();
        }
        #endregion

        #region Navigator functions
        /// <summary>
        /// Initialize tree from user settings.
        /// </summary>
        void InitNavigator()
        {
            ftree.FilterExts = _fileExts.ToLower().SplitByToken(";");
            ftree.RootPaths = Common.Settings.RootDirs.DeepClone();
            ftree.AllTags = Common.Settings.AllTags.DeepClone();
            ftree.DoubleClickSelect = !Common.Settings.Autoplay;

            ftree.TaggedPaths.Clear();
            Common.Settings.TaggedPaths.ForEach(kv => ftree.TaggedPaths.Add((kv.Key, kv.Value)));

            ftree.Init();
        }

        /// <summary>
        /// Tree has seleccted a file to play.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="fn"></param>
        void Navigator_FileSelectedEvent(object sender, string fn)
        {
            rtbInfo.AppendText($"Sel file {fn}{Environment.NewLine}");
            OpenFile(fn);
        }
        #endregion

        #region Misc handlers
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Volume_ValueChanged(object sender, EventArgs e)
        {
            if(_player != null)
            {
                _player.Volume = (float)sldVolume.Value;
            }
        }

        /// <summary>
        /// Update realtime clock.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Timer1_Tick(object sender, EventArgs e)
        {
            if(_player != null)
            {
                timeBar.CurrentTime = MiscUtils.SecondsToTimeSpan(_player.CurrentTime);
            }
            else
            {
                timeBar.CurrentTime = new TimeSpan();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TimeBar_CurrentTimeChanged(object sender, EventArgs e)
        {
            if(_player != null)
            {
                _player.CurrentTime = MiscUtils.TimeSpanToSeconds(timeBar.CurrentTime);
            }
        }
        #endregion
    }
}
