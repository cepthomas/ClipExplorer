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
using NBagOfTricks.UI;


namespace ClipExplorer
{
    public partial class WavePlayer : UserControl, IPlayer
    {
        #region Fields
        /// <summary>Wave output play device.</summary>
        WaveOut _waveOut = null;

        /// <summary>Input device for playing wav file.</summary>
        AudioFileReader _audioFileReader = null;

        /// <summary>Stream read chunk.</summary>
        const int READ_BUFF_SIZE = 1000000;

        /// <summary>The volume.</summary>
        double _volume = 0.8;
        #endregion

        #region Events
        /// <inheritdoc />
        public event EventHandler PlaybackCompleted;

        /// <inheritdoc />
        public event EventHandler<LogEventArgs> Log;
        #endregion

        #region Properties
        /// <inheritdoc />
        public double Volume
        {
            get { return _volume; }
            set { _volume = MathUtils.Constrain(value, 0, 1); if(_waveOut != null) _waveOut.Volume = (float)_volume; }
        }
        #endregion

        #region Lifecycle
        /// <summary>
        /// Constructor.
        /// </summary>
        public WavePlayer()
        {
            InitializeComponent();
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

            // My stuff here.
            CloseAudio();
            
            base.Dispose(disposing);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WavePlayer_Load(object sender, EventArgs e)
        {
            ResetMeters();

            waveViewerL.DrawColor = Color.Black;
            waveViewerR.DrawColor = Color.Black;
            levelL.DrawColor = Common.Settings.MeterColor;
            levelR.DrawColor = Common.Settings.MeterColor;
            timeBar.ProgressColor = Common.Settings.BarColor;
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
                CloseAudio();
                waveViewerL.Reset();
                waveViewerR.Reset();

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

                    timeBar.Length = _audioFileReader.TotalTime;
                    timeBar.Start = TimeSpan.Zero;
                    timeBar.End = TimeSpan.Zero;
                    timeBar.Current = TimeSpan.Zero;

                    // Create reader.
                    var sampleChannel = new SampleChannel(_audioFileReader, false);
                    sampleChannel.PreVolumeMeter += SampleChannel_PreVolumeMeter;

                    var postVolumeMeter = new MeteringSampleProvider(sampleChannel);
                    postVolumeMeter.StreamVolume += PostVolumeMeter_StreamVolume;

                    _waveOut.Init(postVolumeMeter);
                    _waveOut.Volume = (float)Common.Settings.Volume;

                    ShowClip();
                }
                else
                {
                    ok = false;
                }
            }

            if (!ok)
            {
                CloseAudio();
            }

            return ok;
        }

        /// <inheritdoc />
        public void Export()
        {
            LogMessage("INFO", "Sorry, not supported");
        }
        #endregion

        #region Play functions
        /// <inheritdoc />
        public void Play()
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
                _audioFileReader.Position = 0;
                timeBar.Current = TimeSpan.Zero;
            }
        }

        #region Misc functions
        /// <inheritdoc />
        public bool SettingsChanged()
        {
            // Nothing to do.
            return true;
        }

        /// <inheritdoc />
        public List<string> Dump()
        {
            List<string> ret = new List<string>();

            if (_audioFileReader != null)
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
                    try // see OpenFile().
                    {
                        num = _audioFileReader.Read(data, offset, READ_BUFF_SIZE);
                        offset += num;
                    }
                    catch (Exception)
                    {
                    }
                }

                // Make a csv file of data for external processing.
                if (sampleChannel.WaveFormat.Channels == 2) // stereo
                {
                    ret.Add($"Index,Left,Right");
                    long stlen = len / 2;

                    for (long i = 0; i < stlen; i++)
                    {
                        ret.Add($"{i + 1}, {data[i * 2]}, {data[i * 2 + 1]}");
                    }
                }
                else // mono
                {
                    ret.Add($"Index,Val");
                    for (int i = 0; i < data.Length; i++)
                    {
                        ret.Add($"{i + 1}, {data[i]}");
                    }
                }
            }
            else
            {
                LogMessage("ERROR", "Audio file not open");
                ret.Clear();
            }

            return ret;
        }
        #endregion
        #endregion

        #region Private functions
        /// <summary>
        /// 
        /// </summary>
        void CloseAudio()
        {
            _waveOut?.Stop();
            _waveOut?.Dispose();
            _waveOut = null;

            _audioFileReader?.Dispose();
            _audioFileReader = null;
        }

        /// <summary>
        /// Show a clip waveform.
        /// </summary>
        void ShowClip()
        {
            if (_audioFileReader != null)
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
                    
                    for(long i = 0; i < stlen; i++)
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

        /// <summary>
        /// 
        /// </summary>
        void ResetMeters()
        {
            levelL.AddValue(0);
            levelR.AddValue(0);
        }
        #endregion

        #region UI event handlers
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TimeBar_CurrentTimeChanged(object sender, EventArgs e)
        {
            //CurrentTime = timeBar.Current;
        }

        /// <summary>
        /// Usually end of file but could be error.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void WaveOut_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            if (e.Exception != null)
            {
                LogMessage("ERROR", e.Exception.Message);
            }

            PlaybackCompleted?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SampleChannel_PreVolumeMeter(object sender, StreamVolumeEventArgs e)
        {
            //waveformPainterL.AddMax(e.MaxSampleValues[0]);
            //waveformPainterR.AddMax(e.MaxSampleValues[1]);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PostVolumeMeter_StreamVolume(object sender, StreamVolumeEventArgs e)
        {
            levelL.AddValue(e.MaxSampleValues[0]);
            levelR.AddValue(e.MaxSampleValues.Count() > 1 ? e.MaxSampleValues[1] : 0); // stereo?
            timeBar.Current = _audioFileReader.CurrentTime;
        }
        #endregion
    }
}
