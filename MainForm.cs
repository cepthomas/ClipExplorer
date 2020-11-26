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

        /// <summary>Play device.</summary>
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

            // Init UI from settings
            Location = new Point(Common.Settings.MainFormInfo.X, Common.Settings.MainFormInfo.Y);
            Size = new Size(Common.Settings.MainFormInfo.Width, Common.Settings.MainFormInfo.Height);
            WindowState = FormWindowState.Normal;
            KeyPreview = true; // for routing kbd strokes through MainForm_KeyDown
            sldVolume.Value = Common.Settings.Volume;

            timeBar.ProgressColor = Color.LightCyan;

            PopulateRecentMenu();

            InitNavigator();

            Text = $"Clip Explorer {MiscUtils.GetVersionString()} - No file loaded";
            timer1.Enabled = true;

            ///// for testing only
            OpenFile(@"C:\Dev\repos\ClipExplorer\_files\one-sec.wav");
            //OpenFile(@"C:\Dev\repos\ClipExplorer\_files\WICKGAME.MID");
            //var v = player._sourceEvents;
            //DumpMidi(v, "dump.txt");
        }

        /// <summary>
        /// Clean up on shutdown. Dispose() will get the rest.
        /// </summary>
        void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _player?.Close();
            _player?.Dispose();

            // Save user settings.
            ftree.FlushChanges();
            SaveSettings();
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
                bool restart = false;
                bool reinit = false;
                pg.PropertyValueChanged += (sdr, args) =>
                {
                    restart = args.ChangedItem.PropertyDescriptor.Category == "Audio";
                    reinit = args.ChangedItem.PropertyDescriptor.Category == "Navigator";
                };

                f.Controls.Add(pg);
                f.ShowDialog();
                f.Dispose();

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
            chkPlay.Checked = false;

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
                                if(!(_player is WavePlayer))
                                {
                                    if(_player != null)
                                    {
                                        _player.Close();
                                        _player.PlaybackCompleted -= Player_PlaybackCompleted;
                                        _player.Log -= Player_Log;
                                        (_player as UserControl).Visible = false;
                                    }
                                    _player = new WavePlayer();
                                }
                                break;

                            case ".mid":
                                if (!(_player is MidiPlayer))
                                {
                                    if (_player != null)
                                    {
                                        _player.Close();
                                        _player.PlaybackCompleted -= Player_PlaybackCompleted;
                                        _player.Log -= Player_Log;
                                        (_player as UserControl).Visible = false;
                                    }
                                    _player = new MidiPlayer();
                                }
                                break;

                            default:
                                ErrorMessage($"Invalid file type: {fn}");
                                ok = false;
                                break;
                        }

                        if(ok)
                        {
                            _player.PlaybackCompleted += Player_PlaybackCompleted;
                            _player.Log += Player_Log;
                            UserControl ctrl = _player as UserControl;
                            ctrl.Location = new Point(timeBar.Left, timeBar.Bottom + 5);
                            splitContainer1.Panel2.Controls.Add(ctrl);
                            ok = _player.OpenFile(fn);
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
                AddToRecentDefs(fn);
                timeBar.Length = MiscUtils.SecondsToTimeSpan(_player.Length);
            }
            else
            {
                Text = $"ClipExplorer {MiscUtils.GetVersionString()} - No file loaded";

                if (_player != null)
                {
                    _player.Close();
                    _player.PlaybackCompleted -= Player_PlaybackCompleted;
                    _player.Log -= Player_Log;
                    _player = null;
                }
            }

            return ok;
        }

        /// <summary>
        /// Log helper.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Player_Log(object sender, string e)
        {
            rtbInfo.AppendText($"Player:{e}{Environment.NewLine}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Player_PlaybackCompleted(object sender, EventArgs e)
        {
            if (chkLoop.Checked)
            {
                _player.CurrentTime = 0;
                _player.Start();
            }
            else
            {
                chkPlay.Checked = false;
            }
        }

        /// <summary>
        /// Create the menu with the recently used files.
        /// </summary>
        void PopulateRecentMenu()
        {
            ToolStripItemCollection menuItems = recentToolStripMenuItem.DropDownItems;
            menuItems.Clear();

            Common.Settings.RecentFiles.ForEach(f =>
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
                Common.Settings.RecentFiles.UpdateMru(fn);
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
            if (chkPlay.Checked)
            {
                _player?.Start();
            }
            else
            {
                _player?.Stop();
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
                // Toggle.
                if (chkPlay.Checked)
                {
                    _player?.Stop();
                    chkPlay.Checked = false;
                }
                else
                {
                    _player?.Start();
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
            _player?.Rewind();
            chkPlay.Checked = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimeBar_CurrentTimeChanged(object sender, EventArgs e)
        {
            if(_player != null)
            {
                _player.CurrentTime = MiscUtils.TimeSpanToSeconds(timeBar.CurrentTime);
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
            if(_player != null)
            {
                _player.Volume = (float)sldVolume.Value;
            }
        }
        #endregion

        #region Utilities
        /// <summary>
        /// Make a human readable version of midi data.
        /// </summary>
        /// <param name="events"></param>
        /// <param name="fn"></param>
        void DumpMidi(MidiEventCollection events, string fn)
        {
            List<string> st = new List<string>();
            st.Add($"MidiFileType:{events.MidiFileType}");
            st.Add($"DeltaTicksPerQuarterNote:{events.DeltaTicksPerQuarterNote}");
            st.Add($"StartAbsoluteTime:{events.StartAbsoluteTime}");
            st.Add($"Tracks:{events.Tracks}");

            for (int trk = 0; trk < events.Tracks; trk++)
            {
                st.Add($"  Track:{trk}");

                var trackEvents = events.GetTrackEvents(trk);
                for (int te = 0; te < trackEvents.Count; te++)
                {
                    st.Add($"    {trackEvents[te]}");
                }
            }

            File.WriteAllLines(fn, st.ToArray());
        }

        /// <summary>
        /// Make a csv file of data for external processing.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="fn"></param>
        void DumpWave(float[] data, string fn)
        {
            List<string> samples = new List<string>();
            for (int i = 0; i < data.Length; i++)
            {
                samples.Add($"{i + 1}, {data[i]}");
            }
            File.WriteAllLines(fn, samples);
        }
        #endregion

        #region Private functions
        /// <summary>
        /// Initialize ftree from user settings.
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
        #endregion
    }
}
