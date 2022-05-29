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
using NAudio.Wave.SampleProviders;
using NBagOfTricks;
using NBagOfUis;
using AudioLib;


namespace ClipExplorer
{
    public partial class AudioExplorer : UserControl, IExplorer
    {
        #region Fields
        /// <summary>Wave output play device.</summary>
        readonly AudioPlayer _player;

        /// <summary>Input device for audio file.</summary>
        AudioFileReader? _audioFileReader;

        /// <summary>Stream read chunk.</summary>
        const int READ_BUFF_SIZE = 1000000;
        #endregion

        #region Events
        /// <inheritdoc />
        public event EventHandler? PlaybackCompleted;

        /// <inheritdoc />
        public event EventHandler<LogEventArgs>? Log;
        #endregion

        #region Properties
        /// <inheritdoc />
        public bool Valid { get { return _player.Valid; } }

        /// <inheritdoc />
        public double Volume { get { return _player.Volume; } set { _player.Volume = value; } }

        /// <inheritdoc />
        public bool Playing { get { return _player.Playing; } }
        #endregion

        #region Lifecycle
        /// <summary>
        /// Constructor.
        /// </summary>
        public AudioExplorer()
        {
            InitializeComponent();

            // Init settings.
            SettingsChanged();

            // Init UI.
            toolStrip1.Renderer = new NBagOfUis.CheckBoxRenderer() { SelectedColor = Common.Settings.ControlColor };
            waveViewerL.DrawColor = Color.Black;
            waveViewerR.DrawColor = Color.Black;
            timeBar.ProgressColor = Common.Settings.ControlColor;
            timeBar.CurrentTimeChanged += (_, __) => { if(_audioFileReader is not null) _audioFileReader.CurrentTime = timeBar.Current; };

            // Create output device.
            _player = new(Common.Settings.WavOutDevice, int.Parse(Common.Settings.Latency));
            _player.PlaybackStopped += Player_PlaybackStopped;

            Visible = false;
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
            }

            // My stuff here.
            _player.Run(false);
            _player.Dispose();

            _audioFileReader?.Dispose();

            base.Dispose(disposing);
        }
        #endregion

        #region File functions
        /// <inheritdoc />
        public bool OpenFile(string fn)
        {
            bool ok = true;

            using (new WaitCursor())
            {
                // Clean up first.
                _audioFileReader?.Dispose();
                waveViewerL.Reset();
                waveViewerR.Reset();

                // Create input device.
                _audioFileReader = new AudioFileReader(fn);

                timeBar.Length = _audioFileReader.TotalTime;
                timeBar.Start = TimeSpan.Zero; 
                timeBar.End = TimeSpan.Zero;
                timeBar.Current = TimeSpan.Zero;

                // Create reader.
                var sampleChannel = new SampleChannel(_audioFileReader, false);
                sampleChannel.PreVolumeMeter += SampleChannel_PreVolumeMeter;
                var postVolumeMeter = new MeteringSampleProvider(sampleChannel);
                postVolumeMeter.StreamVolume += PostVolumeMeter_StreamVolume;

                _player.Init(postVolumeMeter);

                ShowClip();
            }

            if (!ok)
            {
                _audioFileReader?.Dispose();
                _audioFileReader = null;
            }

            return ok;
        }
        #endregion

        #region Play functions
        /// <inheritdoc />
        public void Play()
        {
            _player.Run(true);
        }

        /// <inheritdoc />
        public void Stop()
        {
            _player.Run(false);
        }

        /// <inheritdoc />
        public void Rewind()
        {
            if(_audioFileReader is not null)
            {
                _audioFileReader.Position = 0;
            }
            _player.Rewind();
            timeBar.Current = TimeSpan.Zero;
        }
        #endregion

        #region Misc functions
        /// <inheritdoc />
        public bool SettingsChanged()
        {
            timeBar.SnapMsec = Common.Settings.SnapMsec;
            return true;
        }
        #endregion

        #region Private functions
        /// <summary>
        /// Show a clip waveform.
        /// </summary>
        void ShowClip()
        {
            if (_audioFileReader is not null)
            {
                _audioFileReader.Position = 0; // rewind
                var sampleChannel = new SampleChannel(_audioFileReader, false);

                // Read all data.
                long len = _audioFileReader.Length / (_audioFileReader.WaveFormat.BitsPerSample / 8);
                var data = new float[len];
                int offset = 0;
                int num = -1;

                while (num != 0)
                {
                    // This throws for flac and m4a files for unknown reason but works ok.
                    try
                    {
                        num = _audioFileReader.Read(data, offset, READ_BUFF_SIZE);
                        offset += num;
                    }
                    catch (Exception)
                    {
                    }
                }

                if (sampleChannel.WaveFormat.Channels == 2) // stereo
                {
                    long stlen = len / 2;
                    var dataL = new float[stlen];
                    var dataR = new float[stlen];

                    for (long i = 0; i < stlen; i++)
                    {
                        dataL[i] = data[i * 2];
                        dataR[i] = data[i * 2 + 1];
                    }

                    waveViewerL.Init(dataL, 1.0f);
                    waveViewerR.Init(dataR, 1.0f);
                }
                else // mono
                {
                    waveViewerL.Init(data, 1.0f);
                    waveViewerR.Init(null, 0);
                }

                timeBar.Length = _audioFileReader.TotalTime;
                timeBar.Start = TimeSpan.Zero;
                timeBar.End = TimeSpan.Zero;
                timeBar.Current = TimeSpan.Zero;

                _audioFileReader.Position = 0; // rewind
            }
        }

        /// <summary>
        /// Logger.
        /// </summary>
        /// <param name="cat"></param>
        /// <param name="msg"></param>
        void LogMessage(string cat, string msg)
        {
            Log?.Invoke(this, new LogEventArgs(cat, msg));
        }
        #endregion

        #region UI event handlers
        /// <summary>
        /// Usually end of file but could be error.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Player_PlaybackStopped(object? sender, StoppedEventArgs e)
        {
            if (e.Exception is not null)
            {
                LogMessage("ERR", e.Exception.Message);
            }

            PlaybackCompleted?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Hook for processing.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SampleChannel_PreVolumeMeter(object? sender, StreamVolumeEventArgs e)
        {
        }

        /// <summary>
        /// Hook for processing.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PostVolumeMeter_StreamVolume(object? sender, StreamVolumeEventArgs e)
        {
            if (_audioFileReader is not null)
            {
                timeBar.Current = _audioFileReader.CurrentTime;
            }
        }

        /// <summary>
        /// Export current file to human readable.
        /// </summary>
        void Export_Click(object? sender, EventArgs e)
        {
            var stext = ((ToolStripMenuItem)sender!).Text;

            if(_audioFileReader is not null)
            {
                try
                {
                    switch (stext)
                    {
                        case "Text":
                            {
                                string name = Path.GetFileNameWithoutExtension(_audioFileReader.FileName);
                                // Clean the file name.
                                name = name.Replace('.', '-').Replace(' ', '_');
                                var newfn = Path.Join(Common.ExportPath, $"{name}.txt");
                                _player.Export(newfn, _audioFileReader);
                                LogMessage("INF", $"Exported to {newfn}");
                            }
                            break;

                        default:
                            LogMessage("ERR", $"Ooops: {stext}");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    LogMessage("ERR", $"{ex.Message}");
                }
            }
        }
        #endregion
    }
}
