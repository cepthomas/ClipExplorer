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
using NAudio.Wave;
using NAudio.CoreAudioApi;
using NAudio.Wave.SampleProviders;
using NAudio.Midi;
//using NAudio.Gui;
using NBagOfTricks;
using NBagOfTricks.UI;
using NBagOfTricks.Utils;



//load from file, save to file
//ftree.Cleanup();
//// Inspect.
//var at = ftree.AllTags;
//var tp = ftree.TaggedPaths;


namespace ClipExplorer
{
    public partial class MainForm : Form
    {
        #region Enums
        /// <summary>Internal status.</summary>
        enum PlayCommand { Start, Stop, Rewind, StopRewind, UpdateUiTime }
        #endregion

        #region Fields
        /// <summary>
        /// Supported file types.
        /// </summary>
        string _fileExts = ".wav;.mp3;";

        /// <summary>
        /// Current file name.
        /// </summary>
        string _fn = "???";

        /// <summary>
        /// Output play device.
        /// </summary>
        WaveOut _waveOut = null;

        /// <summary>
        /// Input device for file.
        /// </summary>
        AudioFileReader _audioFileReader = null;
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

            // Init UI from settings
            Location = new Point(UserSettings.TheSettings.MainFormInfo.X, UserSettings.TheSettings.MainFormInfo.Y);
            Size = new Size(UserSettings.TheSettings.MainFormInfo.Width, UserSettings.TheSettings.MainFormInfo.Height);
            WindowState = FormWindowState.Normal;
            KeyPreview = true; // for routing kbd strokes through MainForm_KeyDown
            sldVolume.Value = UserSettings.TheSettings.Volume;
            ResetMeters();

            PopulateRecentMenu();

            InitNavigator();

            Text = $"Clip Explorer {MiscUtils.GetVersionString()} - No file loaded";
            timer1.Enabled = true;
        }

        /// <summary>
        /// Clean up on shutdown. Dispose() will get the rest.
        /// </summary>
        void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            CloseDevices();

            // Save user settings.
            ftree.Cleanup();
            SaveSettings();
        }
        #endregion

        #region User settings
        /// <summary>
        /// Collect and save user settings.
        /// </summary>
        void SaveSettings()
        {
            UserSettings.TheSettings.AllTags = ftree.AllTags.ToList();
            UserSettings.TheSettings.Autoplay = !ftree.DoubleClickSelect;

            UserSettings.TheSettings.TaggedPaths.Clear();
            ftree.TaggedPaths.ForEach(v => UserSettings.TheSettings.TaggedPaths[v.path] = v.tags);

            UserSettings.TheSettings.Volume = sldVolume.Value;
            UserSettings.TheSettings.MainFormInfo = new Rectangle(Location.X, Location.Y, Width, Height);

            UserSettings.TheSettings.Save();
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
                    SelectedObject = UserSettings.TheSettings
                };

                // Detect changes of interest.
                bool restart = false;
                bool reinit = false;
                pg.PropertyValueChanged += (sdr, args) =>
                {
                    restart = args.ChangedItem.PropertyDescriptor.Category == "Audio";
                    reinit = args.ChangedItem.PropertyDescriptor.Category == "Navigator";
                };

                f.Controls.Add(pg);
                f.ShowDialog();

                // Figure out what changed - each handled differently.
                if (restart)
                {
                    MessageBox.Show("UI changes require a restart to take effect.");
                }
                else if(reinit)
                {
                    InitNavigator();
                }

                SaveSettings();
            }
        }
        #endregion

        #region Info
        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        void ErrorMessage(string s)
        {
            MessageBox.Show(s);
        }

        /// <summary>
        /// The meaning of life.
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
                // CSS
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
        #endregion

        #region File handling
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
        /// Allows the user to select a n audio clip from file system.
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
        }

        /// <summary>
        /// Common file opener.
        /// </summary>
        /// <param name="fn">The file to open.</param>
        /// <returns>Status.</returns>
        public bool OpenFile(string fn)
        {
            bool ret = true;

            using (new WaitCursor())
            {
                try
                {
                    if (File.Exists(fn))
                    {
                        Text = $"ClipExplorer {MiscUtils.GetVersionString()} - {fn}";
                        AddToRecentDefs(fn);

                        _fn = fn;

                        // Clean up first.
                        CloseDevices();

                        // Create output device.
                        for (int id = 0; id < WaveOut.DeviceCount; id++)
                        {
                            var cap = WaveOut.GetCapabilities(id);
                            if (UserSettings.TheSettings.OutputDevice == cap.ProductName)
                            {
                                _waveOut = new WaveOut
                                {
                                    DeviceNumber = id,
                                    DesiredLatency = int.Parse(UserSettings.TheSettings.Latency)
                                };
                                _waveOut.PlaybackStopped += WaveOut_PlaybackStopped;
                                break;
                            }
                        }

                        // Create input device.
                        if (_waveOut != null)
                        {
                            _audioFileReader = new AudioFileReader(_fn);

                            // Create reader.
                            ISampleProvider sampleProvider;

                            var sampleChannel = new SampleChannel(_audioFileReader, true);
                            sampleChannel.PreVolumeMeter += SampleChannel_PreVolumeMeter;

                            var postVolumeMeter = new MeteringSampleProvider(sampleChannel);
                            postVolumeMeter.StreamVolume += PostVolumeMeter_StreamVolume;

                            sampleProvider = postVolumeMeter;
                            _waveOut.Init(sampleProvider);
                            _waveOut.Volume = (float)sldVolume.Value;
                        }
                        else
                        {
                            ErrorMessage($"Failed to create output device: {UserSettings.TheSettings.OutputDevice}");
                            ret = false;
                        }

                        if (_waveOut == null || _audioFileReader == null)
                        {
                            CloseDevices();
                        }
                    }
                    else
                    {
                        ErrorMessage($"Invalid file: {fn}");
                        ret = false;
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage($"Couldn't open the file: {fn} because: {ex.Message}");
                    ret = false;
                }
            }

            return ret;
        }

        /// <summary>
        /// Create the menu with the recently used files.
        /// </summary>
        void PopulateRecentMenu()
        {
            ToolStripItemCollection menuItems = recentToolStripMenuItem.DropDownItems;
            menuItems.Clear();

            UserSettings.TheSettings.RecentFiles.ForEach(f =>
            {
                ToolStripMenuItem menuItem = new ToolStripMenuItem(f, null, new EventHandler(Recent_Click));
                menuItems.Add(menuItem);
            });
        }

        /// <summary>
        /// Update the mru with the user selection.
        /// </summary>
        /// <param name="fn">The selected file.</param>
        void AddToRecentDefs(string fn)
        {
            if (File.Exists(fn))
            {
                UserSettings.TheSettings.RecentFiles.UpdateMru(fn);
                PopulateRecentMenu();
            }
        }
        #endregion

        #region Transport
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Play_CheckedChanged(object sender, EventArgs e)
        {
            if(_waveOut != null && _audioFileReader != null)
            {
                if (chkPlay.Checked)
                {
                    // Start.
                    lblTime.Text = string.Format("{0:00}:{1:00}", (int)_audioFileReader.TotalTime.TotalMinutes, _audioFileReader.TotalTime.Seconds);
                    _waveOut?.Play();
                }
                else
                {
                    // Stop/pause.
                    //_waveOut?.Stop();
                    _waveOut?.Pause();
                    ResetMeters();
                }
            }
        }

        /// <summary>
        /// Do some global key handling. Space bar is used for stop/start playing.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MainForm_KeyDown(object sender, KeyEventArgs e) //TODOC consolidate with above + time display.
        {
            if (e.KeyCode == Keys.Space)
            {
                // Toggle.

                if (chkPlay.Checked)
                {
                    _waveOut.Pause();
                    chkPlay.Checked = false;
                    ResetMeters();
                }
                else
                {
                    _waveOut.Play();
                    chkPlay.Checked = true;
                }
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
            if (_waveOut != null && _audioFileReader != null)
            {
                _waveOut.Stop();
                _audioFileReader.Position = 0;
            }
            chkPlay.Checked = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TrackBarPosition_Scroll(object sender, EventArgs e)
        {
            if (_waveOut != null && _audioFileReader != null)
            {
                _audioFileReader.CurrentTime = TimeSpan.FromSeconds(_audioFileReader.TotalTime.TotalSeconds * trackBarPosition.Value / 100.0);
            }
        }
        #endregion

        #region Volume
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Volume_ValueChanged(object sender, EventArgs e)
        {
            if(_waveOut != null)
            {
                _waveOut.Volume = (float)sldVolume.Value;
            }
        }
        #endregion

        #region Meters
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SampleChannel_PreVolumeMeter(object sender, StreamVolumeEventArgs e)
        {
            waveformPainter1.AddMax(e.MaxSampleValues[0]);
            waveformPainter2.AddMax(e.MaxSampleValues[1]);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PostVolumeMeter_StreamVolume(object sender, StreamVolumeEventArgs e)
        {
            volL.AddValue(e.MaxSampleValues[0]);
            volR.AddValue(e.MaxSampleValues[1]);
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void WaveOut_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            if (e.Exception != null)
            {
                MessageBox.Show(e.Exception.Message, "Playback Device Error");
            }

            if (_audioFileReader != null)
            {
                _audioFileReader.Position = 0;

                if(chkLoop.Checked)
                {
                    _waveOut.Play();
                }
                else
                {
                    chkPlay.Checked = false;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void CloseDevices()
        {
            _waveOut?.Stop();
            _waveOut?.Dispose();
            _waveOut = null;

            _audioFileReader?.Dispose();
            _audioFileReader = null;
        }

        /// <summary>
        /// Initialize ftree from user settings.
        /// </summary>
        void InitNavigator()
        {
            ftree.FilterExts = _fileExts.SplitByToken(";");
            ftree.RootPaths = UserSettings.TheSettings.RootDirs.DeepClone();
            ftree.AllTags = UserSettings.TheSettings.AllTags.DeepClone();
            ftree.DoubleClickSelect = !UserSettings.TheSettings.Autoplay;

            ftree.TaggedPaths.Clear();
            UserSettings.TheSettings.TaggedPaths.ForEach(kv => ftree.TaggedPaths.Add((kv.Key, kv.Value)));

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

        /// <summary>
        /// Update realtime clock.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Timer1_Tick(object sender, EventArgs e)
        {
            if (_waveOut != null && _audioFileReader != null)
            {
                TimeSpan currentTime = (_waveOut.PlaybackState == PlaybackState.Stopped) ? TimeSpan.Zero : _audioFileReader.CurrentTime;
                trackBarPosition.Value = Math.Min(trackBarPosition.Maximum, (int)(100 * currentTime.TotalSeconds / _audioFileReader.TotalTime.TotalSeconds));
                lblTime.Text = string.Format("{0:00}:{1:00}", (int)currentTime.TotalMinutes, currentTime.Seconds);
            }
            else
            {
                trackBarPosition.Value = 0;
                lblTime.Text = "boing";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void ResetMeters()
        {
            volL.AddValue(0);
            volR.AddValue(0);
        }

        private void TimeControl_ValueChanged(object sender, EventArgs e)
        {

        }
    }
}
