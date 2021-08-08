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
using NBagOfTricks;
using NBagOfTricks.UI;


// FUTURE solo/mute individual drums.


namespace ClipExplorer
{
    /// <summary>
    /// A "good enough" midi player.
    /// There are some limitations: Windows multimedia timer has 1 msec resolution at best. This causes a trade-off between
    /// ppq resolution and accuracy. The timer is also inherently wobbly.
    /// </summary>
    public partial class MidiPlayer : UserControl, IPlayer
    {
        #region Constants
        /// <summary>Midi caps.</summary>
        const int NUM_CHANNELS = 16;

        /// <summary>Only 4/4 time supported.</summary>
        const int BEATS_PER_BAR = 4;

        /// <summary>Our ppq aka resolution.</summary>
        const int PPQ = 32;

        /// <summary>The normal drum channel.</summary>
        const int DEFAULT_DRUM_CHANNEL = 10;//TODO not always
        #endregion

        #region Fields
        /// <summary>Midi output device.</summary>
        MidiOut _midiOut = null;

        /// <summary>The fast timer.</summary>
        MmTimerEx _mmTimer = new MmTimerEx();

        /// <summary>Current file.</summary>
        string _fn = "";

        /// <summary>Indicates whether or not the midi is playing.</summary>
        bool _running = false;

        /// <summary>Period.</summary>
        double _msecPerTick = 0;

        /// <summary>Midi events from the input file.</summary>
        MidiEventCollection _sourceEvents = null;

        /// <summary>All the channels.</summary>
        readonly PlayChannel[] _playChannels = new PlayChannel[NUM_CHANNELS];

        /// <summary>Requested tempo from file.</summary>
        int _tempo = 100;

        /// <summary>The midi instrument definitions.</summary>
        readonly Dictionary<int, string> _instrumentDefs = new Dictionary<int, string>();

        /// <summary>The midi drum definitions.</summary>
        readonly Dictionary<int, string> _drumDefs = new Dictionary<int, string>();

        /// <summary>The midi controller definitions.</summary>
        readonly Dictionary<int, string> _controllerDefs = new Dictionary<int, string>();
        #endregion

        #region Events
        /// <inheritdoc />
        public event EventHandler PlaybackCompleted;

        /// <inheritdoc />
        public event EventHandler<string> Log;
        #endregion

        #region Properties - interface implementation
        /// <inheritdoc />
        public double Volume { get; set; }
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
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            // Stop and destroy mmtimer.
            Stop();

            // Resources.
            _midiOut?.Dispose();
            _midiOut = null;

            _mmTimer?.Stop();
            _mmTimer?.Dispose();
            _mmTimer = null;

            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }
        #endregion


        int DrumChannel = DEFAULT_DRUM_CHANNEL;



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
                    int lastTick = 0;
                    _tempo = 100;
                    //HashSet<int> allDrums = new HashSet<int>();

                    // Get events.
                    var mfile = new MidiFile(fn, true);
                    _sourceEvents = mfile.Events;

                    // Init internal structure.
                    for (int i = 0; i < _playChannels.Count(); i++)
                    {
                        _playChannels[i] = new PlayChannel() { ChannelNumber = i + 1 };
                    }

                    // Scale ticks to internal ppq.
                    MidiTime mt = new MidiTime()
                    {
                        InternalPpq = PPQ,
                        MidiPpq = _sourceEvents.DeltaTicksPerQuarterNote,
                        Tempo = _tempo
                    };

                    // Bin events by channel. Scale ticks to internal ppq.
                    for (int track = 0; track < _sourceEvents.Tracks; track++)
                    {
                        foreach(var te in _sourceEvents.GetTrackEvents(track))
                        {
                            if (te.Channel - 1 < NUM_CHANNELS) // midi is one-based
                            {
                                // Do some miscellaneous fixups.

                                // Scale tick to internal.
                                long tick = mt.MidiToInternal(te.AbsoluteTime);

                                //// Adjust channel for non-standard drums.
                                //if (Common.Settings.MapDrumChannel && te.Channel == Common.Settings.DrumChannel)
                                //{
                                //    te.Channel = DRUM_CHANNEL;
                                //}

                                // Other ops.
                                switch(te)
                                {
                                    case NoteOnEvent non:
                                        // Collect drum list.
                                        //if(non.Channel == DRUM_CHANNEL)
                                        //{
                                        //    allDrums.Add(non.NoteNumber);
                                        //}

                                        //if (non.Velocity > 0 && non.NoteLength == 0)
                                        //{
                                        //    non.NoteLength = 1;
                                        //}
                                        break;

                                    case TempoEvent evt:
                                        _tempo = (int)evt.Tempo;
                                        break;

                                    case PatchChangeEvent evt:
                                        _playChannels[te.Channel - 1].Patch = evt.Patch;
                                        break;
                                }

                                // Add to our collection.
                                _playChannels[te.Channel - 1].AddEvent((int)tick, te);
                            }
                        };
                    }

                    // Final fixups.
                    for (int i = 0; i < _playChannels.Count(); i++)
                    {
                        var pc = _playChannels[i];

                        // Make a name for UI.
                        pc.Name = $"Ch:({i + 1}) ";
                        if(i + 1 == DrumChannel) //TODO temp - not correct.
                        {
                            pc.Name += $"Drums";
                        }
                        else if (pc.Patch == -1)
                        {
                            pc.Name += $"NoPatch";
                        }
                        else if(_instrumentDefs.ContainsKey(pc.Patch))
                        {
                            pc.Name += $"{_instrumentDefs[pc.Patch]}";
                        }
                        else
                        {
                            pc.Name += $"Patch:{pc.Patch}";
                        }

                        // Extents.
                        lastTick = Math.Max(lastTick, pc.MaxTick);

                        // Maybe add to UI.
                        if(pc.Valid && pc.HasNotes)
                        {
                            clickGrid.AddIndicator(pc.Name, i);
                        }
                    }

                    clickGrid.Show(2, clickGrid.Width / 2, 20);

                    // Figure out times.
                    sldTempo.Value = _tempo;

                    barBar.Length = new BarSpan(lastTick);
                    barBar.Start = BarSpan.Zero;
                    barBar.End = barBar.Length - BarSpan.OneTick;
                    barBar.Current = BarSpan.Zero;

                    //// Debug stuff. EXP
                    //List<string> sdbg = new List<string>() { "Drums:" };
                    //foreach(int dr in allDrums)
                    //{
                    //    string snm = _drumDefs.ContainsKey(dr) ? _drumDefs[dr] : "???";
                    //    sdbg.Add($"{dr}={snm}");
                    //}
                    //LogMessage(string.Join(" ", sdbg));
                }
            }

            return ok;
        }

        /// <inheritdoc />
        public void Play()
        {
            // Start or restart?
            if(!_running)
            {
                // Calculate the actual period.
                MidiTime mt = new MidiTime()
                {
                    InternalPpq = PPQ,
                    MidiPpq = _sourceEvents.DeltaTicksPerQuarterNote,
                    Tempo = _tempo
                };
                
                _msecPerTick = 60.0 / _tempo;
                int period = mt.RoundedInternalPeriod();

                // Create periodic timer.
                _mmTimer.SetTimer(period, MmTimerCallback);
                _mmTimer.Start();

                _running = true;
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

                _mmTimer.Stop();

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

            //chkMapDrums.Checked = Common.Settings.MapDrumChannel;
            //chkMapDrums.Text = $"Drums\r\nchan {Common.Settings.DrumChannel}";

            barBar.BeatsPerBar = BEATS_PER_BAR;
            barBar.TicksPerBeat = PPQ;
            barBar.Snap = Common.Settings.Snap;

            return ok;
        }

        /// <inheritdoc />
        public List<string> Dump()
        {
            List<string> ret = new List<string>();

            if(_sourceEvents != null)
            {
                List<string> meta = new List<string>
                {
                    $"Meta,Value",
                    $"MidiFileType,{_sourceEvents.MidiFileType}",
                    $"DeltaTicksPerQuarterNote,{_sourceEvents.DeltaTicksPerQuarterNote}",
                    $"StartAbsoluteTime,{_sourceEvents.StartAbsoluteTime}",
                    $"Tracks,{_sourceEvents.Tracks}"
                };

                List<string> notes = new List<string>()
                {
                    "",
                    "Time,Track,Channel,Note,Velocity,Duration",
                };

                List<string> other = new List<string>()
                {
                    "",
                    "Time,Track,Channel,Event,Val1,Val2",
                };

                for (int trk = 0; trk < _sourceEvents.Tracks; trk++)
                {
                    var trackEvents = _sourceEvents.GetTrackEvents(trk);
                    foreach(var te in trackEvents)
                    {
                        string ntype = te.GetType().ToString().Replace("NAudio.Midi.", "");
                        switch (te)
                        {
                            case NoteOnEvent evt:
                                int len = evt.OffEvent == null ? 0 : evt.NoteLength;
                                string nnum = $"{evt.NoteNumber}";
                                if(te.Channel == DrumChannel && _drumDefs.ContainsKey(evt.NoteNumber))
                                {
                                    nnum += $"_{_drumDefs[evt.NoteNumber]}";
                                }
                                notes.Add($"{te.AbsoluteTime},{trk},{te.Channel},{nnum},{evt.Velocity},{len}");
                                break;

                            case TempoEvent evt:
                                _tempo = (int)evt.Tempo;
                                meta.Add($"Tempo,{evt.Tempo}");
                                other.Add($"{evt.AbsoluteTime},{trk},{evt.Channel},{ntype},{evt.Tempo},{evt.MicrosecondsPerQuarterNote}");
                                break;

                            case TimeSignatureEvent evt:
                                other.Add($"{evt.AbsoluteTime},{trk},{evt.Channel},{ntype},{evt.TimeSignature},NA");
                                break;

                            case KeySignatureEvent evt:
                                other.Add($"{evt.AbsoluteTime},{trk},{evt.Channel},{ntype},{evt.SharpsFlats},{evt.MajorMinor}");
                                break;

                            case PatchChangeEvent evt:
                                other.Add($"{evt.AbsoluteTime},{trk},{evt.Channel},{ntype},{evt.Patch},NA");
                                break;

                            case ControlChangeEvent evt:
                                //other.Add($"{evt.AbsoluteTime},{trk},{evt.Channel},{ntype},{evt.Controller},{evt.ControllerValue}");
                                break;

                            case PitchWheelChangeEvent evt:
                                //other.Add($"{evt.AbsoluteTime},{trk},{evt.Channel},{ntype},{evt.Pitch},NA");
                                break;

                            case TextEvent evt:
                                other.Add($"{evt.AbsoluteTime},{trk},{evt.Channel},{ntype},{evt.Text},{evt.Data.Length}");
                                break;

                            case NoteEvent evt:
                                // skip
                                break;

                            //case ChannelAfterTouchEvent:
                            //case SysexEvent:
                            //case MetaEvent:
                            //case RawMetaEvent:
                            //case SequencerSpecificEvent:
                            //case SmpteOffsetEvent:
                            //case TrackSequenceNumberEvent:
                            default:
                                //other.Add($"{te.AbsoluteTime},{trk},{te.Channel},{ntype},{te}");
                                break;
                        }
                    }
                }

                ret.AddRange(meta);
                ret.AddRange(notes);
                ret.AddRange(other);
            }
            else
            {
                Log?.Invoke(this, "ERR: Midi file not open");
                ret.Clear();
            }

            return ret;
        }

        /// <inheritdoc />
        public bool SaveSelection(string fn)
        {
            bool ok = true;
            // FUTURE Make a new clip file from selection.  >> MidiFile.Export(target, events);
            return ok;
        }
        #endregion

        #region Midi send
        /// <summary>
        /// Multimedia timer callback. Synchronously outputs the next midi events.
        /// </summary>
        void MmTimerCallback(double totalElapsed, double periodElapsed)
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
                                    switch (mevt)
                                    {
                                        case NoteOnEvent evt:
                                            if (ch.ChannelNumber == DrumChannel && evt.Velocity == 0)
                                            {
                                                // Skip drum noteoffs as windows GM doesn't like them.
                                            }
                                            else
                                            {
                                                //public override MidiEvent Clone() => new NoteOnEvent(AbsoluteTime, Channel, NoteNumber, Velocity, NoteLength);
                                                //NoteOnEvent ne  = evt.Clone() as NoteOnEvent;

                                                // Adjust volume and maybe drum channel. Also NAudio NoteLength bug.
                                                NoteOnEvent ne = new NoteOnEvent(
                                                    evt.AbsoluteTime,
                                                    ch.ChannelNumber == DrumChannel ? DEFAULT_DRUM_CHANNEL : evt.Channel,
                                                    evt.NoteNumber,
                                                    (int)(evt.Velocity * Volume),
                                                    evt.OffEvent == null ? 0 : evt.NoteLength);

                                                MidiSend(ne);
                                            }
                                            break;

                                        case NoteEvent evt:
                                            if(ch.ChannelNumber == DrumChannel)
                                            {
                                                // Skip drum noteoffs as windows GM doesn't like them.
                                            }
                                            else
                                            {
                                                MidiSend(evt);
                                            }
                                            break;

                                        // No change.
                                        default:
                                            MidiSend(mevt);
                                            break;
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
        /// 
        /// </summary>
        /// <param name="evt"></param>
        void MidiSend(MidiEvent evt)
        {
            _midiOut?.Send(evt.GetAsShortMessage());

            if (Common.Settings.LogEvents)
            {
                LogMessage(evt.ToString());
            }
        }
        #endregion

        #region Private Functions
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
            MidiSend(nevt);
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
                _tempo = (int)sldTempo.Value;
                Play();
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

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //void MapDrums_CheckedChanged(object sender, EventArgs e)
        //{
        //    Common.Settings.MapDrumChannel = chkMapDrums.Checked;
        //    if (_fn != "")
        //    {
        //        // Reload.
        //        OpenFile(_fn);
        //    }
        //}
        #endregion


        /// <summary>
        /// User wants to change the drum channel.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DrumChannel_TextChanged(object sender, EventArgs e)
        {
            // Check for valid number.
            bool valid = int.TryParse(txtDrumChannel.Text, out int dch);
            if (valid && dch >= 1 && dch <= 16)
            {
                DrumChannel = dch;
            }
            else
            {
                DrumChannel = DEFAULT_DRUM_CHANNEL;
                txtDrumChannel.Text = DrumChannel.ToString();
            }
        }
    }

    /// <summary>Channel events and other properties.</summary>
    public class PlayChannel
    {
        #region Properties
        /// <summary>For UI.</summary>
        public int ChannelNumber { get; set; } = -1;

        /// <summary>For UI.</summary>
        public string Name { get; set; } = "";

        /// <summary>Channel used.</summary>
        public bool Valid { get { return Events.Count > 0; } }

        /// <summary>Music or control/meta.</summary>
        public bool HasNotes { get; private set; } = false;

        /// <summary>For muting/soloing.</summary>
        public PlayMode Mode { get; set; } = PlayMode.Normal;
        public enum PlayMode { Normal = 0, Solo = 1, Mute = 2 }

        /// <summary>Optional patch.</summary>
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
            HasNotes |= evt is NoteEvent;
        }

        /// <summary>For viewing pleasure.</summary>
        public override string ToString()
        {
            return $"PlayChannel: Name:{Name} ChannelNumber:{ChannelNumber} Mode:{Mode} Events:{Events.Count} MaxTick:{MaxTick}";
        }
    }
}
