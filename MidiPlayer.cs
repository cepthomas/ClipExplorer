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


// Nebulator:
// <summary>Subdivision setting. 4 means 1/16 notes, 8 means 1/32 notes.</summary>
// public const int SUBBEATS_PER_BEAT = 4;
// 100 bpm = 
//void SetSpeedTimerPeriod()
//{
//    double secPerBeat = 60 / potSpeed.Value; 0.6
//    double msecPerTick = 1000 * secPerBeat / Time.SUBBEATS_PER_BEAT; 150
//    _timer.SetTimer("NEB", (int)msecPerTick);
//}

// TODOC (+TimeBar) display bar.beat like 34.1:909  34.2:123  34.3:456  34.4:777  Time is like 00.00:000


namespace ClipExplorer
{
    public partial class MidiPlayer : UserControl, IPlayer
    {
        #region Fields
        /// <summary>Midi caps.</summary>
        const int NUM_CHANNELS = 16;

        /// <summary>Indicates whether or not the midi is playing.</summary>
        bool _running = false;

        /// <summary>Midi output device.</summary>
        MidiOut _midiOut = null;

        /// <summary>Fast timer.</summary>
        MmTimerEx _timer = new MmTimerEx();

        /// <summary>Current step time.</summary>
        MidiTime _currentStep = new MidiTime();

        /// <summary>Current tempo in bpm.</summary>
        int _tempo = 100;

        /// <summary>Max for whole source file.</summary>
        int _lastBeat = 0;

        /// <summary>Current volume between 0 and 1.</summary>
        double _volume = 0.5;

        /// <summary>From the input file.</summary>
        MidiEventCollection _sourceEvents = null;

        /// <summary>All the channels.</summary>
        readonly PlayChannel[] _playChannels = new PlayChannel[NUM_CHANNELS];
        #endregion

        #region Properties
        /// <inheritdoc />
        public double Volume { get { return _volume; } set { _volume = MathUtils.Constrain(value, 0, 1); } }

        /// <inheritdoc />
        public double CurrentTime { get; set; }
        //public double CurrentTime //TODOC all these with _stepTime
        //{
        //    get { return _midiOut == null ? 0 : TicksToTime(_currentTick); }
        //    set { if (_midiOut != null) { _currentTick = TimeToTicks(value); } }
        //}

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
                                MidiTime mtime = new MidiTime(); // TODOC from source event

                                _playChannels[te.Channel].Steps.AddStep(mtime, step);

                                if (te is TempoEvent) // dig out tempo
                                {
                                    tempo = (int)(te as TempoEvent).Tempo;
                                }
                            }
                            
                            _maxTick = Math.Max(_maxTick, te.AbsoluteTime);
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
            _currentStep.Reset();
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
            double msecPerTick = 1000 * secPerBeat / MidiTime.SUBBEATS_PER_BEAT;
            _timer.SetTimer("NEB", (int)msecPerTick);
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
                        var steps = ch.Steps.GetSteps(_currentStep);

                        foreach (var step in steps)
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

                // Bump time.
                _currentStep.Advance();

                // Check for end of play. Client will take care of looping.
                if (_currentStep.Beat > _lastBeat)
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
        double TimeToTicks(double msec)
        {
            double ticks = (double)_tempo * _sourceEvents.DeltaTicksPerQuarterNote * msec / 60 / 1000;
            return ticks;
        }

        /// <summary>
        /// Convert ticks to time in msec.
        /// </summary>
        /// <returns></returns>
        double TicksToTime(double ticks)
        {
            double msec = ticks / (double)_tempo / _sourceEvents.DeltaTicksPerQuarterNote * 60 * 1000;
            return msec;
        }
        #endregion
    }

    /// <summary>Channel events and other properties.</summary>
    public class PlayChannel
    {
        /// <summary>For muting/soloing.</summary>
        public bool Enabled { get; set; } = true;

        /// <summary>Channel midi events.</summary>
        public MidiStepCollection Steps { get; set; } = new MidiStepCollection();
    }
}
