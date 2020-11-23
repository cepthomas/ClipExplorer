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
// DeltaTicksPerQuarterNote (ppq): 384 = 
// 100 bpm = 38,400 ticks/min = 640 ticks/sec = 0.64 ticks/msec = 1.5625 msec/tick
// Length is 144,000 ticks = 3.75 min = 3:45 (yay)
// Smallest tick is 4

// Ableton Live exports MIDI files with a resolution of 96 ppq, which means a 16th note can be divided into 24 steps.
// All MIDI events are shifted to this grid accordingly when exported.
// DeltaTicksPerQuarterNote (ppq): 96
// 100 bpm = 9,600 ticks/min = 160 ticks/sec = 0.16 ticks/msec = 6.25 msec/tick

// If we use ppq of 8 for 32nd notes:
// 100 bpm = 800 ticks/min = 13.33 ticks/sec = 0.01333 ticks/msec = 75.0 msec/tick
//  99 bpm = 792 ticks/min = 13.20 ticks/sec = 0.0132 ticks/msec  = 75.757 msec/tick


// Nebulator:
// <summary>Subdivision setting. 4 means 1/16 notes, 8 means 1/32 notes.</summary>
// public const int SUBBEATS_PER_BEAT = 4;
// 100 bpm = 

// TODOC (+TimeBar) display bar.beat like 34.1:909  34.2:123  34.3:456  34.4:777  Time is like 00.00:000
// new slders.

// <summary>Subdivision setting aka resolution. 4 means 1/16 notes, 8 means 1/32 notes.</summary>


namespace ClipExplorer
{
    public partial class MidiPlayer : UserControl, IPlayer
    {
        #region Constants
        /// <summary>Midi caps.</summary>
        const int NUM_CHANNELS = 16;

        /// <summary>Only 4/4 time supported.</summary>
        const int BEATS_PER_BAR = 4;

        /// <summary>Subdivision setting aka resolution. 4 means 1/16 notes, 8 means 1/32 notes, etc.
        /// Higher is better accuracy relative to original timing.</summary>
        const int SUBBEATS_PER_BEAT = 8;

        /// <summary>Update time. Compromise between accuracy and resource usage.</summary>
        const int TIMER_PERIOD = 5;
        #endregion

        #region Fields
        /// <summary>Indicates whether or not the midi is playing.</summary>
        bool _running = false;

        /// <summary>Midi output device.</summary>
        MidiOut _midiOut = null;

        /// <summary>Fast timer.</summary>
        MmTimerEx _timer = new MmTimerEx();

        /// <summary>Current step time.</summary>
        //MidiTime _currentStep = new MidiTime();
        int _currentSubBeat = 0;

        /// <summary>Current tempo in bpm.</summary>
        int _tempo = 100;

        /// <summary>Max for whole source file.</summary>
        int _lastBeat = 0;

        /// <summary>Current volume between 0 and 1.</summary>
        double _volume = 0.5;

        /// <summary>Midi events from the input file.</summary>
        MidiEventCollection _sourceEvents = null;

        /// <summary>All the channels.</summary>
        readonly PlayChannel[] _playChannels = new PlayChannel[NUM_CHANNELS];
        #endregion

        #region Properties
        /// <inheritdoc />
        public double Volume { get { return _volume; } set { _volume = MathUtils.Constrain(value, 0, 1); } }

        /// <inheritdoc />
        public double CurrentTime //TODOC all these with _stepTime
        {
           get { return _midiOut == null ? 0 : SubBeatsToTime(_currentSubBeat); }
           set { if (_midiOut != null) { _currentSubBeat = TimeToSubBeats(value); } }
        }

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
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MidiPlayer_Load(object sender, EventArgs e)
        {
            // Fast mm timer.
            _timer = new MmTimerEx();
            SetTempo(100);
            _timer.TimerElapsedEvent += TimerElapsedEvent;
            _timer.SetTimer("MP", TIMER_PERIOD);
            _timer.Start();
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            Stop();

            _timer?.Stop();
            _timer?.Dispose();
            _timer = null;

            // Resources.
            Close();

            if (disposing && (components != null))
            {
                components.Dispose();
            }

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
                    // Default in case not specified in file.
                    int tempo = 100;
                    _lastBeat = 0;

                    // Get events.
                    var mfile = new MidiFile(fn, true);
                    _sourceEvents = mfile.Events;

                    // Init internal structure.
                    for (int i = 0; i < _playChannels.Count(); i++)
                    {
                        _playChannels[i] = new PlayChannel();
                    }

                    // Bin events by channel. Scale times to 96 
                    for (int track = 0; track < _sourceEvents.Tracks; track++)
                    {
                        _sourceEvents.GetTrackEvents(track).ForEach(te =>
                        {
                            if (te.Channel < NUM_CHANNELS)
                            {
                                MidiStep step = new MidiStep() { RawEvent = te, VelocityToPlay = 101 };
                                MidiTime mtime = new MidiTime(); // TODOC from source event w/scaling

                                _playChannels[te.Channel].AddStep(mtime, step);

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
            _running = true;
        }

        /// <inheritdoc />
        public void Stop()
        {
            _running = false;

            //// Send midi stop all notes just in case.
            //_outputs.ForEach(o => o.Value?.Kill());
        }

        /// <inheritdoc />
        public void Rewind()
        {
            Stop();
            //_currentStep.Reset();
            _currentSubBeat = 0;
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
        /// From UI for solo/mute.
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="enable"></param>
        void EnableChannel(int channel, bool enable)
        {
            if (channel < NUM_CHANNELS)
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
//            _ticksPerTimerPeriod = TimeToTicks(MMTIMER_PERIOD);

            double secPerBeat = 60 / sldTempo.Value;
            double msecPerTick = 1000 * secPerBeat / SUBBEATS_PER_BEAT;
        }

        /// <summary>
        /// Multimedia timer tick handler. Synchronously outputs the next midi events.
        /// </summary>
        void TimerElapsedEvent(object sender, MmTimerEx.TimerEventArgs e)
        {
            // Kick over to main UI thread. TODOC needed?
            BeginInvoke((MethodInvoker)delegate ()
            {
                NextStep(e);
            });
        }

        /// <summary>
        /// Output next time/step.
        /// </summary>
        /// <param name="e">Information about updates required.</param>
        void NextStep(MmTimerEx.TimerEventArgs e)
        {
            if (_running)
            {
                // Process each channel.
                foreach (var ch in _playChannels)
                {
                    // Look for events to send.

                    if (ch.Enabled)
                    {
                        // Process any sequence steps.
                        //var steps = ch.Steps.GetSteps(_currentStep);

                        if(ch.Steps.ContainsKey(_currentSubBeat))
                        {
                            foreach (var step in ch.Steps[_currentSubBeat])
                            {
                                var mevt = step.RawEvent;

                                // Maybe adjust volume.
                                if (mevt is NoteEvent)
                                {
                                    double vel = (mevt as NoteEvent).Velocity;
                                    (mevt as NoteEvent).Velocity = (int)(vel * Volume);
                                    _midiOut.Send(mevt.GetAsShortMessage());
                                    // Need to restore.
                                    (mevt as NoteEvent).Velocity = (int)vel;
                                }
                                else // not pertinent
                                {
                                    _midiOut.Send(mevt.GetAsShortMessage());
                                }
                            }
                        }
                    }
                }

                // Bump time.
                //_currentStep.Advance();
                _currentSubBeat++;

                // Check for end of play. Client will take care of looping.
                //if (_currentStep.Beat > _lastBeat)

                if (_currentSubBeat > _lastBeat)
                {
                    Stop();
                    PlaybackCompleted?.Invoke(this, new EventArgs());
                }

                //// Process any lingering noteoffs etc.
                //_outputs.ForEach(o => o.Value?.Housekeep());
                //_inputs.ForEach(i => i.Value?.Housekeep());
            }
        }

        /// <summary>
        /// Convert time to ticks in msec.
        /// </summary>
        /// <param name="msec"></param>
        /// <returns></returns>
        int TimeToSubBeats(double msec)
        {
            int subBeats = _tempo * _sourceEvents.DeltaTicksPerQuarterNote * msec / 60 / 1000;
            return subBeats;
        }

        /// <summary>
        /// Convert subbeats to time in msec.
        /// </summary>
        /// <returns></returns>
        double SubBeatsToTime(int subBeats)
        {
            double msec = subBeats / (double)_tempo / _sourceEvents.DeltaTicksPerQuarterNote * 60 * 1000;
            return msec;
        }
        #endregion

        int ScaleSubBeats(long midiTime)
        {
            // An example midi file:
            // WICKGAME.MID is 3:45
            // DeltaTicksPerQuarterNote (ppq): 384 = 
            // 100 bpm = 38,400 ticks/min = 640 ticks/sec = 0.64 ticks/msec = 1.5625 msec/tick
            // Length is 144,000 ticks = 3.75 min = 3:45 (yay)
            // Smallest tick is 4



            ///// <summary>Only 4/4 time supported.</summary>
            //const int BEATS_PER_BAR = 4;
            ///// <summary>Subdivision setting aka resolution. 4 means 1/16 notes, 8 means 1/32 notes.</summary>
            //const int SUBBEATS_PER_BEAT = 4;
            ///// <summary>Update time. Compromise between accuracy and resource usage.</summary>
            //const int TIMER_PERIOD = 5;

        }

        // /// <summary>Total subBeats for the unit of time.</summary>
        // public int TotalBeats { get { return Bar * BEATS_PER_BAR + Beat; } }

        // /// <summary>Total subBeats for the unit of time.</summary>
        // public int TotalSubBeats { get { return (Bar * BEATS_PER_BAR + Beat) * SUBBEATS_PER_BEAT + SubBeat; } }


        // public MidiTime(int bar, int beat, int subBeat)
        // {
        //     if (beat < 0)
        //     {
        //         //throw new Exception("Negative value is invalid");
        //         beat = 0;
        //     }

        //     if (subBeat >= 0)
        //     {
        //         Beat = beat + subBeat / SUBBEATS_PER_BEAT;
        //         SubBeat = subBeat % SUBBEATS_PER_BEAT;
        //     }
        //     else
        //     {
        //         subBeat = Math.Abs(subBeat);
        //         Beat = beat - (subBeat / SUBBEATS_PER_BEAT) - 1;
        //         SubBeat = SUBBEATS_PER_BEAT - (subBeat % SUBBEATS_PER_BEAT);
        //     }
        // }

        // public MidiTime(int subBeats)
        // {
        //     if (subBeats < 0)
        //     {
        //         throw new Exception("Negative value is invalid");
        //     }

        //     Bar = subBeats / BEATS_PER_BAR;
        //     Beat = subBeats % (BEATS_PER_BAR * SUBBEATS_PER_BEAT);
        //     SubBeat = subBeats % SUBBEATS_PER_BEAT;
        // }

    }

    /// <summary>Channel events and other properties.</summary>
    public class PlayChannel
    {
        /// <summary>For muting/soloing.</summary>
        public bool Enabled { get; set; } = true;

        /// <summary>Channel midi events.</summary>
  //      public MidiStepCollection Steps { get; set; } = new MidiStepCollection();

        ///<summary>The main collection of Steps. The key is the subbeat/tick to send the list.</summary>
        public Dictionary<int, List<MidiStep>> Steps { get; set; } = new Dictionary<int, List<MidiStep>>();

        ///<summary>The duration of the whole thing.</summary>
        public int MaxBeat { get; private set; } = 0;


        /// <summary>
        /// Add a step at the given time.
        /// </summary>
        /// <param name="time"></param>
        /// <param name="step"></param>
        public void AddStep(MidiTime time, MidiStep step)
        {
            int subbeat = time.TotalSubBeats;
            if (!Steps.ContainsKey(subbeat))
            {
                Steps.Add(subbeat, new List<MidiStep>());
            }
            Steps[subbeat].Add(step);

            MaxBeat = Math.Max(MaxBeat, time.Beat);
        }

    }

    /// <summary>
    /// Container for raw midi event and internal stuff.
    /// </summary>
    public class MidiStep
    {
        public MidiEvent RawEvent { get; set; } = null;

        /// <summary>The possibly modified Volume.</summary>
        public double VelocityToPlay { get; set; } = 0.5;

        /// <summary>For viewing pleasure.</summary>
        // public override string ToString()
        // {
        //     return $"channel:{ChannelNumber}";
        // }

        // public void Adjust(double masterVolume, double channelVolume)
        // {
        //     // Maybe alter note velocity.
        //     if (Device is NOutput)
        //     {
        //         NOutput nout = Device as NOutput;
        //         double vel = Velocity * channelVolume * masterVolume;
        //         VelocityToPlay = MathUtils.Constrain(vel, 0, 1.0);
        //     }
        // }

        // /// <summary>For viewing pleasure.</summary>
        // public override string ToString()
        // {
        //     return $"StepNoteOn: {base.ToString()} note:{NoteNumber:F2} vel:{VelocityToPlay:F2} dur:{Duration}";
        // }

    }

}
