using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.Midi;
using NBagOfTricks.Utils;


namespace ClipExplorer
{
    /// <summary>Channel events and other aspects.</summary>
    public class PlayChannel
    {
        /// <summary>For muting/soloing.</summary>
        public bool Enabled { get; set; } = true;

        /// <summary>Channel midi events, sorted by AbsoluteTime.</summary>
        public List<MidiEvent> Events { get; set; } = new List<MidiEvent>();

        /// <summary>Where we are now in Events aka next event to send.</summary>
        public int CurrentIndex { get; set; } = 0;
    }

    public class MidiPlayer : IDisposable
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

        /// <summary>Msec for mm timer tick.</summary>
        const int MMTIMER_PERIOD = 3;

        /// <summary>Where we are now.</summary>
        double _currentMidiTime = 0;

        /// <summary>How many steps to execute per mmtimer tick.</summary>
        double _ticksPerTimerPeriod = 0;

        /// <summary>Multimedia timer identifier.</summary>
        int _timerID = -1;

        /// <summary>Midi output device.</summary>
        MidiOut _midiOut = null;

        /// <summary>BPM.</summary>
        int _bpm = 100;

        /// <summary>From the input file.</summary>
        MidiEventCollection _sourceEvents = null;

        /// <summary>All the channels.</summary>
        PlayChannel[] _playChannels = new PlayChannel[MAX_CHANNELS];

        /// <summary>Delegate for Windows mmtimer callback.</summary>
        delegate void TimeProc(int id, int msg, int user, int param1, int param2);

        /// <summary>Called by Windows when a mmtimer event occurs.</summary>
        TimeProc _timeProc;
        #endregion

        #region Interop Multimedia Timer Functions
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
        #endregion

        #region Properties
        /// <summary>Master volume.</summary>
        public float Volume { get; set; } = 0.5f;

        /// <summary>Current beat. Make it a double?</summary>
        public int CurrentBeat { get; set; }
        #endregion

        #region Lifecycle
        /// <summary>
        /// Normal constructor.
        /// </summary>
        public MidiPlayer()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            Stop();
            // Stop and destroy timer.
            timeKillEvent(_timerID);

            _midiOut?.Dispose();
            _midiOut = null;
        }
        #endregion

        #region Public Functions
        /// <summary>
        /// Load the midi file and convert to internal use.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="devName"></param>
        public bool LoadFile(string fileName, string devName)
        {
            bool ok = false;

            Stop();

            // Figure out which device.
            for (int devindex = 0; devindex < MidiOut.NumberOfDevices; devindex++)
            {
                if (devName == MidiOut.DeviceInfo(devindex).ProductName)
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
                var mfile = new MidiFile(fileName, true);
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

            return ok;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="enable"></param>
        public void EnableChannel(int channel, bool enable)
        {
            if (channel < MAX_CHANNELS)
            {
                _playChannels[channel].Enabled = enable;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bpm"></param>
        public void SetTempo(int bpm)
        {
            _bpm = bpm;

            // Figure out number of ticks per mmtimer period.
            _ticksPerTimerPeriod = (double)bpm * _sourceEvents.DeltaTicksPerQuarterNote * MMTIMER_PERIOD / 60 / 1000;
        }

        /// <summary>
        /// Starts periodic timer.
        /// </summary>
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

        /// <summary>
        /// Stops periodic timer.
        /// </summary>
        public void Stop()
        {
            timeKillEvent(_timerID);
            _running = false;
        }
        #endregion

        #region Private Functions
        /// <summary>Multimedia timer callback. Just calls the</summary>
        void MmTimerCallback(int id, int msg, int user, int param1, int param2)
        {
            if (_running)
            {
                // Output next time/steps. TODOC
                double newTime = _currentMidiTime + _ticksPerTimerPeriod;

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

                _currentMidiTime = newTime;


            }
        }
        #endregion
    }
}
