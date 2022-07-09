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
using NAudio.Midi;
using NAudio.Wave;
using NBagOfTricks;
using NBagOfTricks.Slog;
using NBagOfUis;
using MidiLib;
using AudioLib;


namespace ClipExplorer
{
    public partial class MainForm : Form
    {
        #region Types
        public enum ExplorerState { Stop, Play, Rewind, Complete }
        #endregion

        #region Fields
        /// <summary>My logger.</summary>
        readonly Logger _logger = LogManager.CreateLogger("MainForm");

        /// <summary>Current file.</summary>
        string _fn = "";

        /// <summary>Supported file types.</summary>
        readonly string _audioFileTypes = "*.wav;*.mp3;*.m4a;*.flac";
        readonly string _midiFileTypes = "*.mid";
        readonly string _styleFileTypes = "*.sty;*.pcs;*.sst;*.prs";

        /// <summary>Audio device.</summary>
        readonly AudioExplorer _audioExplorer;

        /// <summary>Midi device.</summary>
        readonly MidiExplorer _midiExplorer;

        /// <summary>Current play device.</summary>
        IExplorer _explorer;
        #endregion

        #region Lifecycle
        /// <summary>
        /// Constructor.
        /// </summary>
        public MainForm()
        {
            // Must do this first before initializing.
            string appDir = MiscUtils.GetAppDataDir("ClipExplorer", "Ephemera");
            Common.Settings = (UserSettings)Settings.Load(appDir, typeof(UserSettings));
            // Tell the libs about their settings.
            MidiSettings.LibSettings = Common.Settings.MidiSettings;
            AudioSettings.LibSettings = Common.Settings.AudioSettings;

            InitializeComponent();

            Icon = Properties.Resources.zebra;

            // Set up paths.
            Common.OutPath = Path.Combine(appDir, "out");
            DirectoryInfo di = new(Common.OutPath);
            di.Create();

            // Init logging.
            LogManager.MinLevelFile = Common.Settings.FileLogLevel;
            LogManager.MinLevelNotif = Common.Settings.NotifLogLevel;
            LogManager.LogEvent += LogManager_LogEvent;
            LogManager.Run();

            // Init main form from settings
            WindowState = FormWindowState.Normal;
            StartPosition = FormStartPosition.Manual;
            Location = new Point(Common.Settings.FormGeometry.X, Common.Settings.FormGeometry.Y);
            Size = new Size(Common.Settings.FormGeometry.Width, Common.Settings.FormGeometry.Height);
            KeyPreview = true; // for routing kbd strokes through OnKeyDown
            SetText();

            // The text output.
            txtViewer.Font = Font;
            txtViewer.WordWrap = true;
            txtViewer.Colors.Add("ERR", Color.LightPink);
            txtViewer.Colors.Add("WRN:", Color.Plum);

            // Other UI configs.
            toolStrip1.Renderer = new NBagOfUis.CheckBoxRenderer() { SelectedColor = Common.Settings.ControlColor };
            btnAutoplay.Checked = Common.Settings.Autoplay;
            btnLoop.Checked = Common.Settings.Loop;
            chkPlay.FlatAppearance.CheckedBackColor = Common.Settings.ControlColor;
            sldVolume.DrawColor = Common.Settings.ControlColor;
            sldVolume.Value = Common.Settings.Volume;

            // Hook up some simple UI handlers.
            btnRewind.Click += (_, __) => { UpdateState(ExplorerState.Rewind); };

            chkPlay.CheckedChanged += ChkPlay_CheckedChanged;

            // Debug stuff.
            //btnDebug.Visible = false;
            //btnDebug.Click += (_, __) => { chkPlay.Checked = true; };
            btnDebug.Click += (_, __) => { DumpDevices(); };

            // Create devices.
            Point loc = new(chkPlay.Left, chkPlay.Bottom + 5);
            _audioExplorer = new() { Location = loc, Volume = Common.Settings.Volume, BorderStyle = BorderStyle.FixedSingle };
            _audioExplorer.PlaybackCompleted += (_, __) => { this.InvokeIfRequired(_ => { UpdateState(ExplorerState.Complete); }); };
            Controls.Add(_audioExplorer); // TODO combine child and parent toolstrips? also midi.

            _midiExplorer = new() { Location = loc, Volume = Common.Settings.Volume, BorderStyle = BorderStyle.FixedSingle };
            _midiExplorer.PlaybackCompleted += (_, __) => { this.InvokeIfRequired(_ => { UpdateState(ExplorerState.Complete); }); };
            Controls.Add(_midiExplorer);

            _explorer = _midiExplorer;
        }

        /// <summary>
        /// Form is legal now. Init things that want to log.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            _logger.Info($"OK to log now!!");

            if(!_audioExplorer.Valid)
            {
                _logger.Error($"Something wrong with your audio output device:{Common.Settings.AudioSettings.WavOutDevice}");
            }

            if (!_midiExplorer.Valid)
            {
                _logger.Error($"Something wrong with your midi output device:{Common.Settings.MidiSettings.OutputDevice}");
            }

            // Initialize tree from user settings.
            InitNavigator();

            _logger.Info("Hello. C=clear, W=wrap");


            // Look for filename passed in.
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                OpenFile(args[1]);
            }
        }

        /// <summary>
        /// Clean up on shutdown. Dispose() will get the rest.
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            LogManager.Stop();
            UpdateState(ExplorerState.Stop);
            SaveSettings();
            base.OnFormClosing(e);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            // Resources.
            _audioExplorer?.Dispose();
            _midiExplorer?.Dispose();

            if (disposing)
            {
                components?.Dispose();
            }

            base.Dispose(disposing);
        }
        #endregion


        /// <summary>
        /// Show log events.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void LogManager_LogEvent(object? sender, LogEventArgs e)
        {
            // Usually come from a different thread.
            if(IsHandleCreated)
            {
                this.InvokeIfRequired(_ => { txtViewer.AppendLine($"{e.Message}"); });
            }
        }

        /// <summary>
        /// Debug stuff.
        /// </summary>
        void DumpDevices()
        {
            for (int id = -1; id < WaveOut.DeviceCount; id++)
            {
                var cap = WaveOut.GetCapabilities(id);
                _logger.Info($"WaveOut {id} {cap.ProductName}");
            }
            for (int id = -1; id < WaveIn.DeviceCount; id++)
            {
                var cap = WaveIn.GetCapabilities(id);
                _logger.Info($"WaveIn {id} {cap.ProductName}");
            }
        }


        #region State management
        /// <summary>
        /// General state management.
        /// </summary>
        void UpdateState(ExplorerState state) 
        {
            if (_explorer.Valid)
            {
                // Unhook.
                chkPlay.CheckedChanged -= ChkPlay_CheckedChanged;
                //_logger.Debug($"state:{state}  chkPlay{chkPlay.Checked}  btnLoop{btnLoop.Checked}  Playing:{_explorer.Playing}");

                try
                {
                    switch (state)
                    {
                        case ExplorerState.Complete:
                            _explorer.Rewind();
                            if (btnLoop.Checked)
                            {
                                chkPlay.Checked = true;
                                _explorer.Play();
                            }
                            else
                            {
                                chkPlay.Checked = false;
                                _explorer.Stop();
                            }
                            break;

                        case ExplorerState.Play:
                            chkPlay.Checked = true;
                            _explorer.Play();
                            break;

                        case ExplorerState.Stop:
                            chkPlay.Checked = false;
                            _explorer.Stop();
                            break;

                        case ExplorerState.Rewind:
                            _explorer.Rewind();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    // Rehook.
                    chkPlay.CheckedChanged += ChkPlay_CheckedChanged;
                }
            }
        }

        /// <summary>
        /// Play button handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChkPlay_CheckedChanged(object? sender, EventArgs e)
        {
            UpdateState(chkPlay.Checked ? ExplorerState.Play : ExplorerState.Stop);
        }
        #endregion

        #region File handling
        /// <summary>
        /// Common file opener.
        /// </summary>
        /// <param name="fn">The file to open.</param>
        /// <returns>Status.</returns>
        public bool OpenFile(string fn)
        {
            bool ok = true;

            UpdateState(ExplorerState.Stop);

            _logger.Info($"Opening file: {fn}");

            using (new WaitCursor())
            {
                try
                {
                    var ext = Path.GetExtension(fn).ToLower();
                    if (_audioFileTypes.Contains(ext))
                    {
                        if(_audioExplorer.Valid)
                        {
                            _audioExplorer.Visible = true;
                            _midiExplorer.Visible = false;
                            _explorer = _audioExplorer;
                        }
                        else
                        {
                            _logger.Error("Your audio device is invalid.");
                            ok = false;
                        }
                    }
                    else if (_midiFileTypes.Contains(ext) || _styleFileTypes.Contains(ext))
                    {
                        if (_midiExplorer.Valid)
                        {
                            _audioExplorer.Visible = false;
                            _midiExplorer.Visible = true;
                            _explorer = _midiExplorer;
                        }
                        else
                        {
                            _logger.Error("Your midi device is invalid.");
                            ok = false;
                        }
                    }
                    else
                    {
                        _logger.Error($"Invalid file type: {fn}");
                        ok = false;
                    }

                    if (ok)
                    {
                        ok = _explorer.OpenFile(fn);
                        _fn = fn;
                        Common.Settings.RecentFiles.UpdateMru(fn);
                        SetText();
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error($"Couldn't open the file: {fn} because: {ex.Message}");
                    _fn = "";
                    SetText();
                    ok = false;
                }
            }

            chkPlay.Enabled = ok;

            if (ok)
            {
                if (Common.Settings.Autoplay)
                {
                    UpdateState(ExplorerState.Rewind);
                    UpdateState(ExplorerState.Play);
                }
            }

            return ok;
        }

        /// <summary>
        /// Initialize tree from user settings.
        /// </summary>
        void InitNavigator()
        {
            var s = _audioFileTypes + _midiFileTypes + _styleFileTypes;
            ftree.FilterExts = s.SplitByTokens("|;*");
            ftree.RootDirs = Common.Settings.RootDirs;
            ftree.SingleClickSelect = true;

            try
            {
                ftree.Init();
            }
            catch (DirectoryNotFoundException)
            {
                _logger.Warn("No tree directories");
            }
        }

        /// <summary>
        /// Tree has selected a file to play.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="fn"></param>
        void Navigator_FileSelectedEvent(object? sender, string fn)
        {
            OpenFile(fn);
        }

        /// <summary>
        /// Organize the file menu item drop down.
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
            if (sender is not null)
            {
                string fn = sender.ToString()!;
                OpenFile(fn);
            }
        }

        /// <summary>
        /// Allows the user to select an audio clip or midi from file system.
        /// </summary>
        void Open_Click(object? sender, EventArgs e)
        {
            var fileTypes = $"All files|*.*|Audio Files|{_audioFileTypes}|Midi Files|{_midiFileTypes}|Style Files|{_styleFileTypes}";
            using OpenFileDialog openDlg = new()
            {
                Filter = fileTypes,
                Title = "Select a file"
            };

            if (openDlg.ShowDialog() == DialogResult.OK && openDlg.FileName != _fn)
            {
                OpenFile(openDlg.FileName);
                _fn = openDlg.FileName;
            }
        }
        #endregion

        #region Misc handlers
        /// <summary>
        /// Do some global key handling. Space bar is used for stop/start playing.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Space:
                    // Toggle.
                    UpdateState(chkPlay.Checked ? ExplorerState.Stop : ExplorerState.Play);
                    e.Handled = true;
                    break;

                case Keys.C:
                    if (e.Modifiers == 0)
                    {
                        txtViewer.Clear();
                        e.Handled = true;
                    }
                    break;

                case Keys.W:
                    if (e.Modifiers == 0)
                    {
                        txtViewer.WordWrap = !txtViewer.WordWrap;
                        e.Handled = true;
                    }
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Volume_ValueChanged(object? sender, EventArgs e)
        {
            float vol = (float)sldVolume.Value;
            Common.Settings.Volume = vol;
            _explorer.Volume = vol;
        }
        #endregion

        #region User settings
        /// <summary>
        /// Collect and save user settings.
        /// </summary>
        void SaveSettings()
        {
            Common.Settings.FormGeometry = new Rectangle(Location.X, Location.Y, Width, Height);
            Common.Settings.Volume = sldVolume.Value;
            Common.Settings.Autoplay = btnAutoplay.Checked;
            Common.Settings.Loop = btnLoop.Checked;
            Common.Settings.Save();
        }

        /// <summary>
        /// Edit the common options in a property grid.
        /// </summary>
        void Settings_Click(object? sender, EventArgs e)
        {
            var changes = Common.Settings.Edit("User Settings", 500);

            // Detect changes of interest.
            bool midiChange = false;
            bool audioChange = false;
            bool navChange = false;
            bool restart = false;

            foreach (var (name, cat) in changes)
            {
                switch(name)
                {
                    case "WavOutDevice":
                    case "Latency":
                    case "InputDevice":
                    case "OutputDevice":
                    case "ControlColor":
                        restart = true;
                        break;

                    case "SnapMsec":
                        audioChange = true;
                        break;

                    case "TempoResolution":
                        midiChange = true;
                        break;

                    case "RootDirs":
                        navChange = true;
                        break;
                }
            }

            if (restart)
            {
                MessageBox.Show("Restart required for device changes to take effect");
            }

            if (midiChange)
            {
                _midiExplorer?.SettingsChanged();
            }

            if (audioChange)
            {
                _audioExplorer?.SettingsChanged();
            }

            if (navChange)
            {
                InitNavigator();
            }

            // Benign changes.
            btnLoop.Checked = Common.Settings.Loop;

            SaveSettings();
        }
        #endregion

        #region Misc
        /// <summary>
        /// All about me.
        /// </summary>
        void About_Click(object? sender, EventArgs e)
        {
            MiscUtils.ShowReadme("ClipExplorer");
        }

        /// <summary>
        /// Utility for header.
        /// </summary>
        void SetText()
        {
            var s = _fn == "" ? "No file loaded" : _fn;
            Text = $"Clip Explorer {MiscUtils.GetVersionString()} - {s}";
        }
        #endregion
    }
}
