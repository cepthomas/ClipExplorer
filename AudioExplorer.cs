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
using NBagOfTricks.Slog;
using NBagOfUis;
using AudioLib;


namespace ClipExplorer
{
    public partial class AudioExplorer : UserControl, IExplorer
    {
        #region Fields
        /// <summary>My logger.</summary>
        readonly Logger _logger = LogManager.CreateLogger("AudioExplorer");

        /// <summary>Wave output play device.</summary>
        readonly AudioPlayer _player;

        /// <summary>Input device for audio player.</summary>
        readonly SwappableSampleProvider _waveOutSwapper;

        /// <summary>Input device for audio file.</summary>
        AudioFileReader? _audioFileReader;
        #endregion

        #region Events
        /// <inheritdoc />
        public event EventHandler? PlaybackCompleted;
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
            UpdateSettings();

            // Init UI.
            toolStrip1.Renderer = new NBagOfUis.CheckBoxRenderer() { SelectedColor = Common.Settings.ControlColor };
            waveViewerL.DrawColor = Common.Settings.ControlColor;
            waveViewerR.DrawColor = Common.Settings.ControlColor;
            timeBar.ProgressColor = Common.Settings.ControlColor;
            timeBar.CurrentTimeChanged += (_, __) => { if(_audioFileReader is not null) _audioFileReader.CurrentTime = timeBar.Current; };

            // Create output device.
            _waveOutSwapper = new();
            _player = new(Common.Settings.AudioSettings.WavOutDevice, int.Parse(Common.Settings.AudioSettings.Latency), _waveOutSwapper);
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

            // Clean up first.
            _audioFileReader?.Dispose();
            waveViewerL.Rewind();
            waveViewerR.Rewind();

            // Create input device.
            _audioFileReader = new AudioFileReader(fn);

            // If it doesn't match, create a resampled temp file.
            if (_audioFileReader.WaveFormat.SampleRate != AudioLibDefs.SAMPLE_RATE)
            {
                _logger.Warn("Invalid sample rate for {fn}");
                ok = false;
            }

            if(ok)
            {
                var sampleChannel = new SampleChannel(_audioFileReader, false);
                sampleChannel.PreVolumeMeter += SampleChannel_PreVolumeMeter;
                var postVolumeMeter = new MeteringSampleProvider(sampleChannel);
                postVolumeMeter.StreamVolume += PostVolumeMeter_StreamVolume;

                // For playing.
                _waveOutSwapper.SetInput(postVolumeMeter);

                // For seeing.
                _audioFileReader.Position = 0; // rewind
                ShowWave(_audioFileReader, _audioFileReader.Length);
            }

            return ok;
        }

        /// <summary>
        /// Paint the wave viewer from the provider.
        /// </summary>
        /// <param name="prov"></param>
        /// <param name="length"></param>
        void ShowWave(ISampleProvider prov, long length)
        {
            _waveOutSwapper.SetInput(prov);

            int bytesPerSample = prov.WaveFormat.BitsPerSample / 8;
            int sclen = (int)(length / bytesPerSample);

            int ht = waveViewerR.Bottom - waveViewerL.Top;
            int wd = waveViewerL.Width;

            // If it's stereo split into two monos, one viewer per.
            if (prov.WaveFormat.Channels == 2) // stereo
            {
                prov.Rewind();
                waveViewerL.Size = new(wd, ht / 2);
                waveViewerL.Init(new StereoToMonoSampleProvider(prov) { LeftVolume = 1.0f, RightVolume = 0.0f });

                prov.Rewind();
                waveViewerR.Visible = true;
                waveViewerR.Size = new(wd, ht / 2);
                waveViewerR.Init(new StereoToMonoSampleProvider(prov) { LeftVolume = 0.0f, RightVolume = 1.0f });
            }
            else // mono
            {
                waveViewerR.Visible = false;
                waveViewerL.Size = new(wd, ht);
                waveViewerL.Init(prov);
            }

            prov.Rewind();
            Text = NAudioEx.GetInfoString(prov);
            //int days, int hours, int minutes, int seconds, int milliseconds
            int msec = 1000 * sclen / prov.WaveFormat.SampleRate;
            timeBar.Length = new(0, 0, 0, 0, msec);
            timeBar.Marker1 = new TimeSpan(0, 0, 0, 0, msec / 3);
            timeBar.Marker2 = new TimeSpan(0, 0, 0, 0, msec / 2);
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
            //_player.Rewind();
            timeBar.Current = TimeSpan.Zero;
        }
        #endregion

        #region Misc functions
        /// <inheritdoc />
        public bool UpdateSettings()
        {
           // timeBar.SnapMsec = Common.Settings.AudioSettings.SnapMsec;
            return true;
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
                _logger.Exception(e.Exception, "Other NAudio error");
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
                                var newfn = Path.Join(Common.OutPath, $"{name}.txt");
                                _audioFileReader.Export(newfn);
                                _logger.Info($"Exported to {newfn}");
                            }
                            break;

                        default:
                            _logger.Error($"Ooops: {stext}");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error($"{ex.Message}");
                }
            }
        }
        #endregion
    }
}
