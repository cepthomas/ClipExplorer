using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.Midi;
using NBagOfTricks.UI;
using NBagOfTricks.Utils;


// TODO Mute/solo individual drums.


// An example midi file: WICKGAME.MID is 3:45 long.
// DeltaTicksPerQuarterNote (ppq): 384.
// 100 bpm = 38,400 ticks/min = 640 ticks/sec = 0.64 ticks/msec = 1.5625 msec/tick.
// Length is 144,000 ticks = 3.75 min = 3:45.
// Smallest event is 4 ticks.

// Ableton Live exports MIDI files with a resolution of 96 ppq, which means a 16th note can be divided into 24 steps.
// DeltaTicksPerQuarterNote (ppq): 96.
// 100 bpm = 9,600 ticks/min = 160 ticks/sec = 0.16 ticks/msec = 6.25 msec/tick.

// If we use ppq of 8 (32nd notes):
// 100 bpm = 800 ticks/min = 13.33 ticks/sec = 0.01333 ticks/msec = 75.0 msec/tick
//  99 bpm = 792 ticks/min = 13.20 ticks/sec = 0.0132 ticks/msec  = 75.757 msec/tick


namespace ClipExplorer
{
    public partial class MidiPlayer : UserControl, IPlayer
    {
        #region Constants
        /// <summary>Midi caps.</summary>
        const int NUM_CHANNELS = 16;

        /// <summary>Only 4/4 time supported.</summary>
        const int BEATS_PER_BAR = 4;

        /// <summary>Our ppq aka resolution. 4 gives 16th note, 8 gives 32nd note, etc.</summary>
        const int TICKS_PER_BEAT = 32;
        #endregion

        #region Fields
        /// <summary>Current file.</summary>
        string _fn = "";

        /// <summary>Indicates whether or not the midi is playing.</summary>
        bool _running = false;

        /// <summary>Midi output device.</summary>
        MidiOut _midiOut = null;

        /// <summary>Period.</summary>
        double _msecPerTick = 0;

        /// <summary>Current volume between 0 and 1.</summary>
        double _volume = 0.5;

        /// <summary>Midi events from the input file.</summary>
        MidiEventCollection _sourceEvents = null;

        /// <summary>All the channels.</summary>
        readonly PlayChannel[] _playChannels = new PlayChannel[NUM_CHANNELS];

        /// <summary>Multimedia timer identifier.</summary>
        int _timerID = -1;

        /// <summary>Delegate for Windows mmtimer callback.</summary>
        delegate void TimeProc(int id, int msg, int user, int param1, int param2);

        /// <summary>Called by Windows when a mmtimer event occurs.</summary>
        TimeProc _timeProc;

        /// <summary>The midi instrument definitions.</summary>
        readonly Dictionary<int, string> _instrumentDefs = new Dictionary<int, string>();

        /// <summary>The midi drum definitions.</summary>
        readonly Dictionary<int, string> _drumDefs = new Dictionary<int, string>();

        /// <summary>The midi controller definitions.</summary>
        readonly Dictionary<int, string> _controllerDefs = new Dictionary<int, string>();
        #endregion

        #region Properties - interface implementation
        /// <inheritdoc />
        public double Volume { get { return _volume; } set { _volume = MathUtils.Constrain(value, 0, 1); } }
        #endregion

        #region Events
        /// <inheritdoc />
        public event EventHandler PlaybackCompleted;

        /// <inheritdoc />
        public event EventHandler<string> Log;
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
        /// Init UI.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MidiPlayer_Load(object sender, EventArgs e)
        {
            LoadMidiDefs();

            SettingsUpdated();

            // Figure out which midi output device.
            for (int devindex = 0; devindex < MidiOut.NumberOfDevices; devindex++)
            {
                if (Common.Settings.MidiOutDevice == MidiOut.DeviceInfo(devindex).ProductName)
                {
                    _midiOut = new MidiOut(devindex);
                    break;
                }
            }

            if(_midiOut == null)
            {
                MessageBox.Show($"Invalid midi device: {Common.Settings.MidiOutDevice}");
            }

            // Set up the channel/mute/solo grid.
            clickGrid.AddStateType((int)PlayChannel.PlayMode.Normal, Color.Black, Color.AliceBlue);
            clickGrid.AddStateType((int)PlayChannel.PlayMode.Solo, Color.Black, Color.LightGreen);
            clickGrid.AddStateType((int)PlayChannel.PlayMode.Mute, Color.Black, Color.Salmon);

            barBar.ProgressColor = Common.Settings.BarColor;
            barBar.CurrentTimeChanged += BarBar_CurrentTimeChanged;

            sldTempo.DrawColor = Common.Settings.SliderColor;

            _timeProc = new TimeProc(MmTimerCallback);
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            // Stop and destroy mmtimer.
            Stop();
            timeKillEvent(_timerID);

            // Resources.
            _midiOut?.Dispose();
            _midiOut = null;

            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }
        #endregion

        #region Public Functions - interface implementation
        /// <inheritdoc />
        public bool OpenFile(string fn)
        {
            bool ok = true;
            _fn = fn;

            using (new WaitCursor())
            {
                // Clean up first.
                clickGrid.Clear();
                Rewind();

                if (ok)
                {
                    // Default in case not specified in file.
                    int tempo = 100;
                    int lastTick = 0;

                    // Get events.
                    var mfile = new MidiFile(fn, true);
                    _sourceEvents = mfile.Events;

                    // Init internal structure.
                    for (int i = 0; i < _playChannels.Count(); i++)
                    {
                        _playChannels[i] = new PlayChannel();
                    }

                    // Bin events by channel. Scale ticks to internal ppq.
                    for (int track = 0; track < _sourceEvents.Tracks; track++)
                    {
                        foreach(var te in _sourceEvents.GetTrackEvents(track))
                        {
                            if (te.Channel - 1 < NUM_CHANNELS) // midi is one-based
                            {
                                // Adjust channel for non-compliant drums.
                                if(Common.Settings.MapDrumChannel && te.Channel == Common.Settings.DrumChannel)
                                {
                                    te.Channel = 10;
                                }

                                // Scale tick to internal.
                                long tick = te.AbsoluteTime * TICKS_PER_BEAT / _sourceEvents.DeltaTicksPerQuarterNote;

                                _playChannels[te.Channel - 1].AddEvent((int)tick, te);

                                // Dig out special events.
                                switch (te)
                                {
                                    case TempoEvent tem:
                                        tempo = (int)tem.Tempo;
                                        break;
                                    case PatchChangeEvent pte:
                                        _playChannels[te.Channel - 1].Patch = pte.Patch;
                                        break;
                                    //case TextEvent tevt:
                                    //    if (tevt.MetaEventType == MetaEventType.SequenceTrackName)
                                    //    {
                                    //        _playChannels[tevt.Channel - 1].Name = tevt.Text;
                                    //    }
                                    //    break;
                                }
                            }
                        };
                    }

                    // Final fixups.
                    for (int i = 0; i < _playChannels.Count(); i++)
                    {
                        var pc = _playChannels[i];

                        pc.Name = $"Ch:({i + 1}) ";

                        if(i == 9)
                        {
                            pc.Name += pc.Name = $"Drums";
                        }
                        else if (pc.Patch == -1)
                        {
                            pc.Name += pc.Name = $"NoPatch";
                        }
                        else if(_instrumentDefs.ContainsKey(pc.Patch))
                        {
                            pc.Name += $"{_instrumentDefs[pc.Patch]}";
                        }
                        else
                        {
                            pc.Name += $"Patch:{pc.Patch}";
                        }

                        lastTick = Math.Max(lastTick, pc.MaxTick);
                        if(pc.Valid)
                        {
                            clickGrid.AddIndicator(pc.Name, i);
                        }
                    }

                    clickGrid.Show(2, clickGrid.Width / 2, 20);

                    // Figure out times.
                    sldTempo.Value = tempo;

                    barBar.Length = new BarSpan(lastTick);
                    barBar.Start = BarSpan.Zero;
                    barBar.End = barBar.Length - BarSpan.OneTick;
                    barBar.Current = BarSpan.Zero;
                }
            }

            return ok;
        }

        /// <inheritdoc />
        public void Start()
        {
            // Start or restart?
            if(!_running)
            {
                timeKillEvent(_timerID);

                // Calculate the actual period to tell the user.
                double secPerBeat = 60 / sldTempo.Value;
                _msecPerTick = 1000 * secPerBeat / TICKS_PER_BEAT;

                int period = _msecPerTick > 1.0 ? (int)Math.Round(_msecPerTick) : 1;
                float msecPerBeat = period * TICKS_PER_BEAT;
                float actualBpm = 60.0f * 1000.0f / msecPerBeat;
                LogMessage($"Period:{period} Goal_BPM:{sldTempo.Value:f2} Actual_BPM:{actualBpm:f2}");

                // Create and start periodic timer. Resolution is 1. Mode is TIME_PERIODIC.
                _timerID = timeSetEvent(period, 1, _timeProc, IntPtr.Zero, 1);

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
            else
            {
                Rewind();
            }
        }

        /// <inheritdoc />
        public void Stop()
        {
            if(_running)
            {
                _running = false;
                timeKillEvent(_timerID);
                _timerID = -1;

                // Send midi stop all notes just in case.
                for (int i = 0; i < _playChannels.Count(); i++)
                {
                    if(_playChannels[i].Valid)
                    {
                        Kill(i);
                    }
                }
            }
        }

        /// <inheritdoc />
        public void Rewind()
        {
            barBar.Current = BarSpan.Zero;
        }

        /// <inheritdoc />
        public bool SettingsUpdated()
        {
            bool ok = true;

            chkMapDrums.Checked = Common.Settings.MapDrumChannel;
            chkMapDrums.Text = $"Drums\r\nchan {Common.Settings.DrumChannel}";

            barBar.BeatsPerBar = BEATS_PER_BAR;
            barBar.TicksPerBeat = TICKS_PER_BEAT;
            barBar.Snap = Common.Settings.Snap;

            return ok;
        }

        /// <inheritdoc />
        public bool Dump(string fn)
        {
            bool ok = true;

            if(_sourceEvents != null)
            {
                List<string> st = new List<string>
                {
                    $"MidiFileType:{_sourceEvents.MidiFileType}",
                    $"DeltaTicksPerQuarterNote:{_sourceEvents.DeltaTicksPerQuarterNote}",
                    $"StartAbsoluteTime:{_sourceEvents.StartAbsoluteTime}",
                    $"Tracks:{_sourceEvents.Tracks}"
                };

                for (int trk = 0; trk < _sourceEvents.Tracks; trk++)
                {
                    st.Add($"");
                    st.Add($">>> Track:{trk}");

                    var trackEvents = _sourceEvents.GetTrackEvents(trk);
                    for (int te = 0; te < trackEvents.Count; te++)
                    {
                        st.Add($"{trackEvents[te]}");
                    }
                }

                File.WriteAllLines(fn, st.ToArray());
            }
            else
            {
                Log?.Invoke(this, "ERR: Midi file not open");
                ok = false;
            }

            return ok;
        }

        /// <inheritdoc />
        public bool SaveSelection(string fn)
        {
            bool ok = true;
            //TODO Make a new clip file from selection.  >> MidiFile.Export(target, events);
            return ok;
        }
        #endregion

        #region Private Functions
        /// <summary>
        /// Multimedia timer callback. Synchronously outputs the next midi events.
        /// </summary>
        void MmTimerCallback(int id, int msg, int user, int param1, int param2)
        {
            if (_running)
            {
                // Any soloes?
                bool solo = _playChannels.Where(c => c.Mode == PlayChannel.PlayMode.Solo).Count() > 0;

                // Process each channel.
                foreach (var ch in _playChannels)
                {
                    if(ch.Valid)
                    {
                        // Look for events to send.
                        if (ch.Mode == PlayChannel.PlayMode.Solo || (!solo && ch.Mode == PlayChannel.PlayMode.Normal))
                        {
                            // Process any sequence steps.
                            if (ch.Events.ContainsKey(barBar.Current.TotalTicks))
                            {
                                foreach (var mevt in ch.Events[barBar.Current.TotalTicks])
                                {
                                    // Maybe adjust volume.
                                    if (mevt is NoteEvent)
                                    {
                                        double vel = (mevt as NoteEvent).Velocity;
                                        (mevt as NoteEvent).Velocity = (int)(vel * Volume);
                                        _midiOut?.Send(mevt.GetAsShortMessage());
                                        // Need to restore.
                                        (mevt as NoteEvent).Velocity = (int)vel;
                                    }
                                    else // not pertinent
                                    {
                                        _midiOut?.Send(mevt.GetAsShortMessage());
                                    }
                                }
                            }
                        }
                    }
                }

                // Bump time. Check for end of play. Client will take care of transport control.
                if(barBar.IncrementCurrent(1))
                {
                    _running = false;
                    PlaybackCompleted?.Invoke(this, new EventArgs());
                }
            }
        }

        /// <summary>
        /// Logger.
        /// </summary>
        /// <param name="s"></param>
        void LogMessage(string s)
        {
            Log?.Invoke(this, s);
        }

        /// <summary>
        /// Send all notes off.
        /// </summary>
        /// <param name="channel"></param>
        void Kill(int channel)
        {
            //LogMessage($"Kill:{channel}");
            ControlChangeEvent nevt = new ControlChangeEvent(0, channel + 1, MidiController.AllNotesOff, 0);
            _midiOut?.Send(nevt.GetAsShortMessage());
        }

        /// <summary>
        /// Load midi definitions from md file.
        /// </summary>
        void LoadMidiDefs()
        {
            _instrumentDefs.Clear();
            _drumDefs.Clear();
            _controllerDefs.Clear();

            // Read the file.
            object section = null;

            string fpath = Path.Combine(MiscUtils.GetExeDir(), @"Resources\gm_defs.md");
            foreach (string sl in File.ReadAllLines(fpath))
            {
                List<string> parts = sl.SplitByToken("|");

                if (parts.Count > 1 && !parts[0].StartsWith("#"))
                {
                    switch (parts[0])
                    {
                        case "Instrument":
                            section = _instrumentDefs;
                            break;

                        case "Drum":
                            section = _drumDefs;
                            break;

                        case "Controller":
                            section = _controllerDefs;
                            break;

                        case string s when !s.StartsWith("---"):
                            (section as Dictionary<int, string>)[int.Parse(parts[1])] = parts[0];
                            break;
                    }
                }
            }
        }
        #endregion

        #region Event handlers
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClickGrid_IndicatorEvent(object sender, IndicatorEventArgs e)
        {
            int channel = e.Id;
            PlayChannel pch = _playChannels[channel];
            //LogMessage($"Click:{channel}");

            switch (pch.Mode)
            {
                case PlayChannel.PlayMode.Normal:
                    pch.Mode = PlayChannel.PlayMode.Solo;
                    // Mute any other non-solo channels.
                    for (int i = 0; i < NUM_CHANNELS; i++)
                    {
                        if (i != channel && _playChannels[i].Valid && _playChannels[i].Mode != PlayChannel.PlayMode.Solo)
                        {
                            Kill(i);
                        }
                    }
                    break;

                case PlayChannel.PlayMode.Solo:
                    pch.Mode = PlayChannel.PlayMode.Mute;
                    // Mute this channel.
                    Kill(channel);
                    break;

                case PlayChannel.PlayMode.Mute:
                    pch.Mode = PlayChannel.PlayMode.Normal;
                    break;
            }

            clickGrid.SetIndicator(channel, (int)pch.Mode);
        }

        /// <summary>
        /// User changed tempo.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Tempo_ValueChanged(object sender, EventArgs e)
        {
            if (_running)
            {
                Stop();
                Start();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BarBar_CurrentTimeChanged(object sender, EventArgs e)
        {
            //CurrentTime = TicksToTime(barBar.Current.TotalTicks);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MapDrums_CheckedChanged(object sender, EventArgs e)
        {
            Common.Settings.MapDrumChannel = chkMapDrums.Checked;
            if(_fn != "")
            {
                // Reload.
                OpenFile(_fn);
            }
        }
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
            public int periodMin;
            public int periodMax;
        }
        #pragma warning restore IDE1006 // Naming Styles
        #endregion
    }

    /// <summary>Channel events and other properties.</summary>
    public class PlayChannel
    {
        #region Properties
        /// <summary>For UI.</summary>
        public string Name { get; set; } = "";

        /// <summary>Channel used.</summary>
        public bool Valid { get { return Events.Count > 0; } }

        /// <summary>For muting/soloing.</summary>
        public PlayMode Mode { get; set; } = PlayMode.Normal;
        public enum PlayMode { Normal = 0, Solo = 1, Mute = 2 }

        /// <summary>For UI.</summary>
        public int Patch { get; set; } = -1;

        ///<summary>The main collection of Steps. The key is the subbeat/tick to send the list.</summary>
        public Dictionary<int, List<MidiEvent>> Events { get; set; } = new Dictionary<int, List<MidiEvent>>();

        ///<summary>The duration of the whole channel.</summary>
        public int MaxTick { get; private set; } = 0;
        #endregion

        /// <summary>Add an event at the given tick.</summary>
        /// <param name="tick"></param>
        /// <param name="evt"></param>
        public void AddEvent(int tick, MidiEvent evt)
        {
            if (!Events.ContainsKey(tick))
            {
                Events.Add(tick, new List<MidiEvent>());
            }
            Events[tick].Add(evt);
            MaxTick = Math.Max(MaxTick, tick);
        }

        /// <summary>For viewing pleasure.</summary>
        public override string ToString()
        {
            return $"PlayChannel: Name:{Name} Mode:{Mode} Events:{Events.Count} MaxTick:{MaxTick}";
        }
    }
}
