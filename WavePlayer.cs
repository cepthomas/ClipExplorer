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
        /// <summary>Wave output play device.</summary>
        WaveOut _waveOut = null;

        /// <summary>Input device for playing wav file.</summary>
        AudioFileReader _audioFileReader = null;

        public float Volume { get { return _waveOut.Volume; } set { _waveOut.Volume = value; } }


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

            // My stuff.

            
            base.Dispose(disposing);
        }



        /// <summary>
        /// Opens an audio wave file. Includes compressed.
        /// </summary>
        /// <param name="fn">The file to open.</param>
        /// <returns>Status.</returns>
        public bool OpenFile(string fn)
        {
            bool ok = true;

            using (new WaitCursor())
            {
                try
                {

                    // Clean up first.
                    Close();

                    // Create output device.
                    for (int id = 0; id < WaveOut.DeviceCount; id++)
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

//                        timeBar.CurrentTime = new TimeSpan();
//                        timeBar.Length = _audioFileReader.TotalTime;

                        // Create reader.
                        ISampleProvider sampleProvider;

                        var sampleChannel = new SampleChannel(_audioFileReader, true);
                        sampleChannel.PreVolumeMeter += SampleChannel_PreVolumeMeter;

                        var postVolumeMeter = new MeteringSampleProvider(sampleChannel);
                        postVolumeMeter.StreamVolume += PostVolumeMeter_StreamVolume;

                        sampleProvider = postVolumeMeter;
                        _waveOut.Init(sampleProvider);
                        _waveOut.Volume = Volume;

                        ShowClip(fn);
                    }
                    else
                    {
    //                    ErrorMessage($"Failed to create output device: {Common.Settings.WavOutDevice}");
                        ok = false;
                    }
                }
                catch (Exception ex)
                {
//                    ErrorMessage($"Couldn't open the file: {fn} because: {ex.Message}");
                    ok = false;
                }
            }

            if (!ok)
            {
                Close();
            }

            return ok;
        }

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

                if (chkLoop.Checked)
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

        /// <summary>
        /// 
        /// </summary>
        void ResetMeters()
        {
            levelL.AddValue(0);
            levelR.AddValue(0);
        }

        public void Start()
        {
            if (_waveOut != null && _audioFileReader != null)
            {
                _waveOut.Play();
            }
        }

        public void Stop()
        {
            if (_waveOut != null && _audioFileReader != null)
            {
                _waveOut.Pause(); // or Stop?
                ResetMeters();
            }
        }

        public void Rewind()
        {
            if (_waveOut != null && _audioFileReader != null)
            {
                _waveOut.Stop();
                _audioFileReader.Position = 0;
            }
        }

        public void Close()
        {
            _waveOut?.Stop();
            _waveOut?.Dispose();
            _waveOut = null;

            _audioFileReader?.Dispose();
            _audioFileReader = null;
        }
    }
}
