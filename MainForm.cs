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
using NAudio.Wave;
using NAudio.CoreAudioApi;
using NAudio.Wave.SampleProviders;
using NBagOfTricks;
using NBagOfTricks.UI;
using NBagOfTricks.Utils;
using NAudio.Midi;
using System.Diagnostics;


namespace ClipExplorer
{
    public partial class MainForm : Form
    {
        #region Enums
        /// <summary>Internal status.</summary>
        enum PlayCommand { Start, Stop, Rewind, StopRewind, UpdateUiTime }
        #endregion

        #region Fields
        string _fileExts = ".wav;.mp3;"; //.aiff;.aac";

        IWavePlayer _waveOut;

        string _fn;

        AudioFileReader _audioFileReader;
        #endregion

        #region Lifecycle
        /// <summary>
        /// Constructor.
        /// </summary>
        public MainForm()
        {
            // Need to load settings before creating controls in MainForm_Load().
            string appDir = MiscUtils.GetAppDataDir("ClipExplorer");
            DirectoryInfo di = new DirectoryInfo(appDir);
            di.Create();
            UserSettings.Load(appDir);
            InitializeComponent();
        }

        /// <summary>
        /// Initialize form controls.
        /// </summary>
        void MainForm_Load(object sender, EventArgs e)
        {
            // Init UI from settings
            Location = new Point(UserSettings.TheSettings.MainFormInfo.X, UserSettings.TheSettings.MainFormInfo.Y);
            Size = new Size(UserSettings.TheSettings.MainFormInfo.Width, UserSettings.TheSettings.MainFormInfo.Height);
            WindowState = FormWindowState.Normal;

            KeyPreview = true; // for routing kbd strokes through MainForm_KeyDown

            PopulateRecentMenu();

            CreateWaveOut();

            Text = $"Clip Explorer {MiscUtils.GetVersionString()} - No file loaded";

            List<string> paths = new List<string>() { UserSettings.TheSettings.RootDir, @"C:\Dev\repos\ClipExplorer\files" };
            List<string> exts = _fileExts.SplitByToken(";");
            navigator.Init(paths, exts);

            timer1.Enabled = true;
        }

        /// <summary>
        /// Clean up on shutdown.
        /// </summary>
        void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _waveOut?.Stop();
            timer1?.Stop();

            // Save user settings.
            SaveSettings();
        }

        /// <summary>
        /// Resource clean up.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                timer1?.Dispose();
                _audioFileReader?.Dispose();
                _waveOut?.Dispose();
                components?.Dispose();
            }

            base.Dispose(disposing);
        }
        #endregion

        #region User settings
        /// <summary>
        /// Save user settings that aren't automatic.
        /// </summary>
        void SaveSettings()
        {
            UserSettings.TheSettings.MainFormInfo.FromForm(this);
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
                bool ctrls = false;
                pg.PropertyValueChanged += (sdr, args) =>
                {
                    string p = args.ChangedItem.PropertyDescriptor.Name;
                    //ctrls |= (p.Contains("Font") | p.Contains("Color"));
                };

                f.Controls.Add(pg);
                f.ShowDialog();

                // Figure out what changed - each handled differently.
                if (ctrls)
                {
                    MessageBox.Show("UI changes require a restart to take effect.");
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
            MessageBox.Show(s); //TODOC
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
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            string fn = sender.ToString();
            OpenFile(fn);
        }

        /// <summary>
        /// Allows the user to select a file from file system.
        /// </summary>
        void Open_Click(object sender, EventArgs e)
        {
            //openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            //You can add several filter patterns to a filter by separating the file types with semicolons, for example:
            //Image Files(*.BMP; *.JPG; *.GIF)| *.BMP; *.JPG; *.GIF | All files(*.*) | *.*
            //".wav;.mp3;"

            string sext = "";
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
            string ret = "";

            using (new WaitCursor())
            {
                try
                {
                    if (File.Exists(fn))
                    {
                        Text = $"ClipExplorer {MiscUtils.GetVersionString()} - {fn}";
                        AddToRecentDefs(fn);

                        _fn = fn;

                        // Create reader.
                        ISampleProvider sampleProvider;

                        _audioFileReader?.Dispose();
                        _audioFileReader = new AudioFileReader(_fn);

                        var sampleChannel = new SampleChannel(_audioFileReader, true);
                        sampleChannel.PreVolumeMeter += OnPreVolumeMeter;
                        var postVolumeMeter = new MeteringSampleProvider(sampleChannel);
                        postVolumeMeter.StreamVolume += OnPostVolumeMeter;

                        sampleProvider = postVolumeMeter;
                        _waveOut.Init(sampleProvider);
                    }
                    else
                    {
                        ret = $"Invalid file: {fn}";
                    }
                }
                catch (Exception ex)
                {
                    ret = $"Couldn't open the file: {fn} because: {ex.Message}";
                }
            }

            if(ret != "")
            {
                ErrorMessage(ret);
            }

            return ret == "";
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
            if(chkPlay.Checked)
            {
                // Start.


                labelTotalTime.Text = string.Format("{0:00}:{1:00}", (int)_audioFileReader.TotalTime.TotalMinutes, _audioFileReader.TotalTime.Seconds);



                _waveOut?.Play();
            }
            else
            {
                // Stop/pause.
                //_waveOut?.Stop();
                _waveOut?.Pause();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Rewind_Click(object sender, EventArgs e)
        {
            _waveOut.Stop();
            _audioFileReader.Position = 0;
            chkPlay.Checked = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TrackBarPosition_Scroll(object sender, EventArgs e)
        {
            if (_waveOut != null)
            {
                _audioFileReader.CurrentTime = TimeSpan.FromSeconds(_audioFileReader.TotalTime.TotalSeconds * trackBarPosition.Value / 100.0);
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Volume_ValueChanged(object sender, EventArgs e)
        {
            _waveOut.Volume = (float)sldVolume.Value;
        }
        #endregion

        #region Meters
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnPreVolumeMeter(object sender, StreamVolumeEventArgs e)
        {
            // we know it is stereo TODOC???
            waveformPainter1.AddMax(e.MaxSampleValues[0]);
            waveformPainter2.AddMax(e.MaxSampleValues[1]);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnPostVolumeMeter(object sender, StreamVolumeEventArgs e)
        {
            // we know it is stereo TODOC???
            volumeMeter1.Amplitude = e.MaxSampleValues[0];
            volumeMeter2.Amplitude = e.MaxSampleValues[1];
        }
        #endregion

        #region Wave device management
        /// <summary>
        /// 
        /// </summary>
        void CreateWaveOut()
        {
            CloseWaveOut();

            // Find output device.
            MMDevice mmd = null;

            var endPoints = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
            foreach (var endPoint in endPoints)
            {
                if(UserSettings.TheSettings.OutputDevice == endPoint.FriendlyName)
                {
                    mmd = endPoint;
                    break;
                }
            }

            if (mmd != null)
            {
                var wasapi = new WasapiOut(
                    mmd,
                    UserSettings.TheSettings.WasapiExclusive ? AudioClientShareMode.Exclusive : AudioClientShareMode.Shared,
                    UserSettings.TheSettings.WasapiEventCallback,
                    int.Parse(UserSettings.TheSettings.Latency));

                _waveOut = wasapi;
                _waveOut.PlaybackStopped += OnPlaybackStopped;
            }
            else
            {
                ErrorMessage($"Bad device: {UserSettings.TheSettings.OutputDevice}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnPlaybackStopped(object sender, StoppedEventArgs e)
        {
            if (e.Exception != null)
            {
                MessageBox.Show(e.Exception.Message, "Playback Device Error");
            }
            if (_audioFileReader != null)
            {
                _audioFileReader.Position = 0;

                //TODOC Check for loop and restart. Else unset button.
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void CloseWaveOut()
        {
            if (_waveOut != null)
            {
                _waveOut.Stop();
                //TODOC -= events?
                _waveOut.Dispose();
                _waveOut = null;
            }

            if (_audioFileReader != null)
            {
                _audioFileReader.Dispose();
                _audioFileReader = null;
            }
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Navigator_FileSelectedEvent(object sender, string e)
        {
            rtbInfo.AppendText($"Sel file {e}{Environment.NewLine}");
            //TODOC play file.

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer1_Tick(object sender, EventArgs e)
        {
            if (_waveOut != null && _audioFileReader != null)
            {
                TimeSpan currentTime = (_waveOut.PlaybackState == PlaybackState.Stopped) ? TimeSpan.Zero : _audioFileReader.CurrentTime;
                trackBarPosition.Value = Math.Min(trackBarPosition.Maximum, (int)(100 * currentTime.TotalSeconds / _audioFileReader.TotalTime.TotalSeconds));
                labelCurrentTime.Text = string.Format("{0:00}:{1:00}", (int)currentTime.TotalMinutes, currentTime.Seconds);
            }
            else
            {
                trackBarPosition.Value = 0;
            }
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
                // Handle start/stop toggle.
                //ProcessPlay(chkPlay.Checked ? PlayCommand.Stop : PlayCommand.Start, true);
                e.Handled = true;
            }
        }

        private void navigator_FileSelectedEvent(object sender, string e)
        {

        }
    }
}
