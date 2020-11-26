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
    public partial class WavePlayer : UserControl, IPlayer
    {
        #region Fields
        /// <summary>Wave output play device.</summary>
        WaveOut _waveOut = null;

        /// <summary>Input device for playing wav file.</summary>
        AudioFileReader _audioFileReader = null;
        #endregion

        #region Properties - interface implementation
        /// <inheritdoc />
        public double Volume { get { return _waveOut.Volume; } set { _waveOut.Volume = (float)MathUtils.Constrain(value, 0, 1); } }

        /// <inheritdoc />
        public double CurrentTime
        { 
            get { return _audioFileReader == null ? 0 : MiscUtils.TimeSpanToSeconds(_audioFileReader.CurrentTime); }
            set { if (_audioFileReader != null) { _audioFileReader.CurrentTime = MiscUtils.SecondsToTimeSpan(value); } }
        }

        /// <inheritdoc />
        public double Length { get; private set; }
        #endregion

        #region Events
        /// <inheritdoc />
        public event EventHandler PlaybackCompleted;

        /// <inheritdoc />
        public event EventHandler<string> Log;
        #endregion

        #region Lifecycle
        /// <summary>
        /// Constructor.
        /// </summary>
        public WavePlayer()
        {
            InitializeComponent();
            ResetMeters();
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            // My stuff here....
            Close();
            
            base.Dispose(disposing);
        }
        #endregion

        #region Public Functions - interface implementation
        /// <inheritdoc />
        public bool OpenFile(string fn)
        {
            bool ok = true;

            using (new WaitCursor())
            {
                // Clean up first.
                Close();

                // Create output device.
                for (int id = -1; id < WaveOut.DeviceCount; id++)
                {
                    var cap = WaveOut.GetCapabilities(id);
                    if (Common.Settings.WavOutDevice == cap.ProductName)
                    {
                        _waveOut = new WaveOut
                        {
                            DeviceNumber = id,
                            DesiredLatency = int.Parse(Common.Settings.Latency)
                        };
                        _waveOut.PlaybackStopped += WaveOut_PlaybackStopped;
                        break;
                    }
                }

                // Create input device.
                if (_waveOut != null)
                {
                    _audioFileReader = new AudioFileReader(fn);

                    CurrentTime = 0;
                    Length = MiscUtils.TimeSpanToSeconds(_audioFileReader.TotalTime);

                    // Create reader.
                    ISampleProvider sampleProvider;

                    var sampleChannel = new SampleChannel(_audioFileReader, true);
                    sampleChannel.PreVolumeMeter += SampleChannel_PreVolumeMeter;

                    var postVolumeMeter = new MeteringSampleProvider(sampleChannel);
                    postVolumeMeter.StreamVolume += PostVolumeMeter_StreamVolume;

                    sampleProvider = postVolumeMeter;
                    _waveOut.Init(sampleProvider);
                    _waveOut.Volume = (float)Volume;

                    ShowClip(fn);
                }
                else
                {
                    ok = false;
                }
            }

            if (!ok)
            {
                Close();
            }

            return ok;
        }

        /// <inheritdoc />
        public void Start()
        {
            if (_waveOut != null && _audioFileReader != null)
            {
                _waveOut.Play();
            }
        }

        /// <inheritdoc />
        public void Stop()
        {
            if (_waveOut != null && _audioFileReader != null)
            {
                _waveOut.Pause(); // or Stop?
                ResetMeters();
            }
        }

        /// <inheritdoc />
        public void Rewind()
        {
            if (_waveOut != null && _audioFileReader != null)
            {
                _waveOut.Stop();
                _audioFileReader.Position = 0;
            }
        }

        /// <inheritdoc />
        public void Close()
        {
            _waveOut?.Stop();
            _waveOut?.Dispose();
            _waveOut = null;

            _audioFileReader?.Dispose();
            _audioFileReader = null;
        }
        #endregion

        #region Private functions
        /// <summary>
        /// Show a clip waveform.
        /// </summary>
        void ShowClip(string fn)
        {
            using (AudioFileReader afrdr = new AudioFileReader(fn))
            {
                var sampleChannel = new SampleChannel(afrdr, true);

                long len = afrdr.Length / sizeof(float);
                var data = new float[len];

                int offset = 0;
                int num = -1;
                while (num != 0)
                {
                    num = afrdr.Read(data, offset, 20000);
                    offset += num;
                }

                waveViewer.Init(data, 1.0f);
            }
        }

        /// <summary>
        /// Logger.
        /// </summary>
        /// <param name="s"></param>
        void LogMessage(string s)
        {
            Log?.Invoke(this, $"WavePlayer:{s}");
        }

        /// <summary>
        /// 
        /// </summary>
        void ResetMeters()
        {
            levelL.AddValue(0);
            levelR.AddValue(0);
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Usually end of file but could be error.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void WaveOut_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            if (e.Exception != null)
            {
                throw e.Exception;
            }

            if (_audioFileReader != null)
            {
                _audioFileReader.Position = 0;
                PlaybackCompleted?.Invoke(this, new EventArgs());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SampleChannel_PreVolumeMeter(object sender, StreamVolumeEventArgs e)
        {
            waveformPainterL.AddMax(e.MaxSampleValues[0]);
            waveformPainterR.AddMax(e.MaxSampleValues[1]);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PostVolumeMeter_StreamVolume(object sender, StreamVolumeEventArgs e)
        {
            levelL.AddValue(e.MaxSampleValues[0]);
            levelR.AddValue(e.MaxSampleValues[1]);
        }
        #endregion
    }
}
