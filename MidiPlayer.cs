using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.Midi;
using NBagOfTricks.UI;
using NBagOfTricks.Utils;


// An example midi file:
// WICKGAME.MID is 3:45
// DeltaTicksPerQuarterNote (ppq): 384
// 100 bpm = 38,400 ticks/min = 640 ticks/sec = 0.64 ticks/msec
// Length is 144,000 ticks = 3.75 min = 3:45 (yay)
// Smallest tick is 4
// 
// Ableton Live exports MIDI files with a resolution of 96 ppq, which means a 16th note can be divided into 24 steps.
// All MIDI events are shifted to this grid accordingly when exported.

namespace ClipExplorer
{
    public partial class MidiPlayer : UserControl, IPlayer
    {
        #region Fields
        /// <summary>Midi caps.</summary>
        const int MAX_MIDI = 127;

        /// <summary>Midi caps.</summary>
        const int MAX_CHANNELS = 16;

        /// <summary>Midi caps.</summary>
        const int MAX_PITCH = 16383;

        /// <summary>Indicates whether or not the timer is running.</summary>
        bool _running = false;

        /// <summary>Midi output device.</summary>
        MidiOut _midiOut = null;

        /// <summary>Current tempo in bpm.</summary>
        int _tempo = 100;

        /// <summary>From the input file.</summary>
        MidiEventCollection _sourceEvents = null;

        /// <summary>All the channels.</summary>
        readonly PlayChannel[] _playChannels = new PlayChannel[MAX_CHANNELS];

        /// <summary>How many steps to execute per mmtimer tick.</summary>
        double _ticksPerTimerPeriod = 0;

        /// <summary>Msec for mm timer tick.</summary>
        const int MMTIMER_PERIOD = 3;

        /// <summary>Multimedia timer identifier.</summary>
        int _timerID = -1;

        /// <summary>Delegate for Windows mmtimer callback.</summary>
        delegate void TimeProc(int id, int msg, int user, int param1, int param2);

        /// <summary>Called by Windows when a mmtimer event occurs.</summary>
        TimeProc _timeProc;
        #endregion

        #region Interop Multimedia Timer Functions
#pragma warning disable IDE1006 // Naming Styles

        [DllImport("winmm.dll")]
        private static extern int timeGetDevCaps(ref TimerCaps caps, int sizeOfTimerCaps);

        /// <summary>Start her up.</summary>
        [DllImport("winmm.dll")]
        private static extern int timeSetEvent(int delay, int resolution, TimeProc proc, IntPtr user, int mode);

        [DllImport("winmm.dll")]
        private static extern int timeKillEvent(int id);

        /// <summary>Represents information about the multimedia timer capabilities.</summary>
        [StructLayout(LayoutKind.Sequential)]
        struct TimerCaps
        {
            /// <summary>Minimum supported period in milliseconds.</summary>
            public int periodMin;

            /// <summary>Maximum supported period in milliseconds.</summary>
            public int periodMax;
        }

#pragma warning restore IDE1006 // Naming Styles
        #endregion

        #region Properties //TODOC check range etc
        /// <inheritdoc />
        public double Volume { get; set; } = 0.5;

        /// <inheritdoc />
        public double PlayPosition { get; set; }

        /// <inheritdoc />
        public double Length { get; private set; }
        #endregion

        #region Events
        /// <inheritdoc />
        public event EventHandler PlaybackCompleted;
        #endregion

        #region Lifecycle
        /// <summary>
        /// Normal constructor.
        /// </summary>
        public MidiPlayer()
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

            // Stop and destroy mmtimer.
            Stop();
            timeKillEvent(_timerID);

            // Resources.
            Close();

            base.Dispose(disposing);
        }
        #endregion

        #region Public Functions
        /// <inheritdoc />
        public bool OpenFile(string fn)
        {
            bool ok = true;

            using (new WaitCursor())
            {
                // Clean up first.
                Close();

                // Figure out which output device.
                for (int devindex = 0; devindex < MidiOut.NumberOfDevices; devindex++)
                {
                    if (Common.Settings.MidiOutDevice == MidiOut.DeviceInfo(devindex).ProductName)
                    {
                        _midiOut = new MidiOut(devindex);
                        ok = true;
                        break;
                    }
                }

                if (ok)
                {
                    // Initialize timer with default values.
                    _timeProc = new TimeProc(MmTimerCallback);

                    // Default in case not specified in file.
                    int tempo = 100;

                    // Get events.
                    var mfile = new MidiFile(fn, true);
                    _sourceEvents = mfile.Events;

                    // Init internal structure.
                    for (int i = 0; i < _playChannels.Count(); i++)
                    {
                        _playChannels[i] = new PlayChannel();
                    }

                    // Bin events by channel.
                    for (int track = 0; track < _sourceEvents.Tracks; track++)
                    {
                        _sourceEvents.GetTrackEvents(track).ForEach(te =>
                        {
                            if (te.Channel < MAX_CHANNELS)
                            {
                                _playChannels[te.Channel].Events.Add(te);

                                if (te is TempoEvent) // dig out tempo
                                {
                                    tempo = (int)(te as TempoEvent).Tempo;
                                }
                            }
                        });
                    }

                    SetTempo(tempo);
                }
            }

            return ok;
        }

        /// <inheritdoc />
        public void Start()
        {
            // Create and start periodic timer. Resolution is 1. Mode is TIME_PERIODIC.
            _timerID = timeSetEvent(MMTIMER_PERIOD, 1, _timeProc, IntPtr.Zero, 1);

            // If the timer was created successfully.
            if (_timerID != 0)
            {
                _running = true;
            }
            else
            {
                _running = false;
                throw new Exception("Unable to start periodic multimedia Timer.");
            }
        }

        /// <inheritdoc />
        public void Stop()
        {
            timeKillEvent(_timerID);
            _running = false;
        }

        /// <inheritdoc />
        public void Rewind()
        {
            Stop();
            PlayPosition = 0;
        }

        /// <inheritdoc />
        public void Close()
        {
            Stop();

            _midiOut?.Dispose();
            _midiOut = null;
        }
        #endregion

        #region Private Functions
        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="enable"></param>
        void EnableChannel(int channel, bool enable)
        {
            if (channel < MAX_CHANNELS)
            {
                _playChannels[channel].Enabled = enable;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tempo">BPM</param>
        void SetTempo(int tempo)
        {
            _tempo = tempo;

            // Figure out number of ticks per mmtimer period.
            _ticksPerTimerPeriod = (double)tempo * _sourceEvents.DeltaTicksPerQuarterNote * MMTIMER_PERIOD / 60 / 1000;
        }

        /// <summary>
        /// Multimedia timer callback. Synchronously outputs the next midi events.
        /// </summary>
        void MmTimerCallback(int id, int msg, int user, int param1, int param2)
        {
            if (_running)
            {
                // Output next time/steps.
                double newTime = PlayPosition + _ticksPerTimerPeriod;

                //MidiEvent
                //public MidiCommandCode CommandCode { get; }
                //public int DeltaTime { get; }
                //public virtual int Channel { get; set; }
                //public long AbsoluteTime { get; set; }

                // Output any midi events between last and now. TODOC
                // Check for solo/mute/enabled.
                // Adjust volume.

                //// Process any lingering noteoffs etc.
                //_outputs.ForEach(o => o.Value?.Housekeep());
                //_inputs.ForEach(i => i.Value?.Housekeep());

                PlayPosition = newTime;
            }
        }
        #endregion
    }

    /// <summary>Channel events and other properties.</summary>
    public class PlayChannel
    {
        /// <summary>For muting/soloing.</summary>
        public bool Enabled { get; set; } = true;

        /// <summary>Channel midi events, sorted by AbsoluteTime.</summary>
        public List<MidiEvent> Events { get; set; } = new List<MidiEvent>();

        /// <summary>Where we are now in Events aka next event to send.</summary>
        public int CurrentIndex { get; set; } = 0;
    }
}
