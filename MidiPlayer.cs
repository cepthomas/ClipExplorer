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
using NBagOfUis;
using MidiLib;


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
        /// <summary>Only 4/4 time supported.</summary>
        const int BEATS_PER_BAR = 4;

        /// <summary>Our internal ppq aka resolution - used for sending realtime midi messages.</summary>
        const int PPQ = 32;
        #endregion


        /// <summary>Midi player.</summary>
        Player _player = new();

        /// <summary>The fast timer.</summary>
        readonly MmTimerEx _mmTimer = new();

        /// <summary>Midi events from the input file.</summary>
        MidiData _mdata = new();

        ///// <summary>All the channel controls.</summary>
        //readonly List<ChannelControl> _channelControls = new();

        ///// <summary>Where to export to.</summary>
        //string _exportPath = "???";

        ///// <summary>My midi out.</summary>
        //readonly string _midiOutDevice = "???";





        #region Fields
        /// <summary>The internal channel objects.</summary>
        ChannelCollection _allChannels = new();
        
        ///// <summary>Midi output device.</summary>
        //MidiOut? _midiOut = null;

        ///// <summary>The fast timer.</summary>
        //readonly MmTimerEx _mmTimer = new();

        ///// <summary>Midi events from the input file.</summary>
        //MidiFile? _mfile;

        ///// <summary>All the channels. Index is 0-based channel number.</summary>
        //readonly PlayChannel[] _playChannels = new PlayChannel[MidiDefs.NUM_CHANNELS];

        /// <summary>Requested tempo from file. Use default if not supplied.</summary>
        double _tempo = Common.Settings.DefaultTempo;

        ///// <summary>Some midi files have drums on a different channel so allow the user to re-map.</summary>
        //int _drumChannel = MidiDefs.DEFAULT_DRUM_CHANNEL;
        #endregion

        #region Events
        /// <inheritdoc />
        public event EventHandler? PlaybackCompleted;

        /// <inheritdoc />
        public event EventHandler<LogEventArgs>? Log;
        #endregion

        #region Properties
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
        void MidiPlayer_Load(object? sender, EventArgs e)
        {
            //// Init internal structure.
            //for (int i = 0; i < _playChannels.Length; i++)
            //{
            //    _playChannels[i] = new PlayChannel() { ChannelNumber = i + 1 };
            //}

            SettingsChanged();

            _player = new(Common.Settings.MidiOutDevice, _allChannels);
            _mdata.ExportPath = Common.Settings.ExportPath;

            // Set up the channel/mute/solo grid.
            gridChannels.AddStateType((int)ChannelState.Normal, Color.Black, Color.AliceBlue);
            gridChannels.AddStateType((int)ChannelState.Solo, Color.Black, Color.LightGreen);
            gridChannels.AddStateType((int)ChannelState.Mute, Color.Black, Color.Salmon);

            barBar.ProgressColor = Common.Settings.ControlColor;
            barBar.CurrentTimeChanged += BarBar_CurrentTimeChanged;

            sldTempo.DrawColor = Common.Settings.ControlColor;

            chkLogMidi.FlatAppearance.CheckedBackColor = Common.Settings.ControlColor;

            // Init channels and selectors.
            _allChannels.ForEach(ch => ch.IsDrums = ch.ChannelNumber == MidiDefs.DEFAULT_DRUM_CHANNEL);
            cmbDrumChannel.Items.Add("NA");
            for (int i = 1; i <= MidiDefs.NUM_CHANNELS; i++)
            {
                cmbDrumChannel.Items.Add(i);
            }
            cmbDrumChannel.SelectedIndex = MidiDefs.DEFAULT_DRUM_CHANNEL;
            cmbDrumChannel.SelectedIndexChanged += DrumChannel_SelectedIndexChanged;

            btnKill.Click += (_, __) => _player.KillAll();
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
            _player?.Dispose();

            _mmTimer.Stop();
            _mmTimer.Dispose();

            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
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
                _allChannels.Reset();
                gridChannels.Clear();
                Rewind();

                // Process the file. This creates all channels.
                _mdata.Read(fn, Common.Settings.DefaultTempo, false);

                // Assumed always at least one.
                var pinfo = _mdata.AllPatterns[0];
                _tempo = pinfo.Tempo;

                // For scaling subdivs to internal.
                MidiTime mt = new()
                {
                    InternalPpq = PPQ,
                    MidiPpq = _mdata.DeltaTicksPerQuarterNote,
                    Tempo = _tempo
                };

                for (int i = 0; i < MidiDefs.NUM_CHANNELS; i++)
                {
                    int chnum = i + 1;

                    var chEvents = _mdata.AllEvents.
                        Where(e => e.PatternName == pinfo.PatternName && e.ChannelNumber == chnum && (e.MidiEvent is NoteEvent || e.MidiEvent is NoteOnEvent)).
                        OrderBy(e => e.AbsoluteTime);

                    if (chEvents.Any())
                    {
                        _allChannels.SetEvents(chnum, chEvents, mt);
                        // Send patch maybe. These can change per pattern.
                        _player.SendPatch(chnum, pinfo.Patches[i]);
                    }
                }

                barBar.Length = new BarSpan(_allChannels.TotalSubdivs);
                barBar.Start = BarSpan.Zero;
                barBar.End = barBar.Length - BarSpan.OneSubdiv;
                barBar.Current = BarSpan.Zero;

                InitChannelsGrid();
            }

            return ok;
        }
        #endregion

        #region Play functions
        /// <inheritdoc />
        public void Play()
        {
            // Start or restart?
            if (!_mmTimer.Running)
            {
                SetTimer();
                //// Downshift to time increments compatible with this system.
                //MidiTime mt = new()
                //{
                //    InternalPpq = PPQ,
                //    MidiPpq = _mfile!.DeltaTicksPerQuarterNote,
                //    Tempo = _tempo
                //};

                ////msecPerSubdiv = 60.0 / _tempo;
                //int period = mt.RoundedInternalPeriod();

                //// Create periodic timer.
                //_mmTimer.SetTimer(period, MmTimerCallback);
                _mmTimer.Start();
                _player.Run(true);
            }
            else
            {
                Rewind();
            }
        }

        /// <inheritdoc />
        public void Stop()
        {
            _mmTimer.Stop();
            _player.Run(false);
            // Send midi stop all notes just in case.
            _player.KillAll();
        }

        /// <inheritdoc />
        public void Rewind()
        {
            barBar.Current = BarSpan.Zero;
        }
        #endregion

        #region Misc functions
        /// <inheritdoc />
        public bool SettingsChanged()
        {
            bool ok = true;

            barBar.ZeroBased = Common.Settings.ZeroBased;
            barBar.BeatsPerBar = BEATS_PER_BAR;
            barBar.SubdivsPerBeat = PPQ;
            barBar.Snap = Common.Settings.Snap;

            sldTempo.Resolution = Common.Settings.TempoResolution;

            return ok;
        }
        #endregion

        #region Midi send
        /// <summary>
        /// Multimedia timer callback. Synchronously outputs the next midi events.
        /// </summary>
        void MmTimerCallback(double totalElapsed, double periodElapsed)
        {
            try
            {
                _player.DoNextStep();

                //// Bump over to main thread.
                //this.InvokeIfRequired(_ => UpdateState());
                // Bump time. Check for end of play. Client will take care of transport control.
                if (barBar.IncrementCurrent(1))
                {
                    PlaybackCompleted?.Invoke(this, new EventArgs());
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="evt"></param>
        //void MidiSend(MidiEvent evt)
        //{
        //    _midiOut?.Send(evt.GetAsShortMessage());

        //    if (chkLogMidi.Checked)
        //    {
        //        LogMessage("SND", evt.ToString());
        //    }
        //}
        #endregion

        #region Private Functions
        ///// <summary>
        ///// Get requested events.
        ///// </summary>
        //void GetEvents()
        //{
        //    // Init internal structure.
        //    _playChannels.ForEach(pc => pc.Reset());

        //    // Downshift to time increments compatible with this application.
        //    MidiTime mt = new()
        //    {
        //        InternalPpq = PPQ,
        //        MidiPpq = _mfile!.DeltaTicksPerQuarterNote,
        //        Tempo = _tempo
        //    };

        //    // Bin events by channel. Scale to internal ppq.
        //    foreach (var ch in _mfile.Channels)
        //    {
        //        _playChannels[ch.Key - 1].Patch = ch.Value;
        //        var pevts = _mfile.GetEvents(ch.Key);

        //        foreach (var te in pevts)
        //        {
        //            if (te.Channel - 1 < MidiDefs.NUM_CHANNELS) // midi is one-based
        //            {
        //                // Scale to internal.
        //                long subdiv = mt.MidiToInternal(te.AbsoluteTime);

        //                // Add to our collection.
        //                _playChannels[te.Channel - 1].AddEvent((int)subdiv, te);
        //            }
        //        };
        //    }

        //    // Figure out times.
        //    int lastSubdiv = _playChannels.Max(pc => pc.MaxSubdiv);
        //    //// Round up to bar.
        //    //int floor = lastSubdiv / (PPQ * 4); // 4/4 only.
        //    //lastSubdiv = (floor + 1) * (PPQ * 4);
        //    sldTempo.Value = _tempo;

        //    barBar.Length = new BarSpan(lastSubdiv);
        //    barBar.Start = BarSpan.Zero;
        //    barBar.End = barBar.Length - BarSpan.OneSubdiv;
        //    barBar.Current = BarSpan.Zero;
        //}

        /// <summary>
        /// Logger.
        /// </summary>
        /// <param name="cat"></param>
        /// <param name="msg"></param>
        void LogMessage(string cat, string msg)
        {
            Log?.Invoke(this, new LogEventArgs(cat, msg));
        }

        ///// <summary>
        ///// Send all notes off.
        ///// </summary>
        ///// <param name="channel">1-based channel</param>
        //void Kill(int channel)
        //{
        //    ControlChangeEvent nevt = new(0, channel, MidiController.AllNotesOff, 0);
        //    MidiSend(nevt);
        //}

        ///// <summary>
        ///// Send all notes off.
        ///// </summary>
        //void KillAll()
        //{
        //    // Send midi stop all notes just in case.
        //    for (int i = 0; i < MidiDefs.NUM_CHANNELS; i++)
        //    {
        //        Kill(i + 1);
        //    }

        //    //// Send midi stop all notes just in case.
        //    //for (int i = 0; i < _playChannels.Count(); i++)
        //    //{
        //    //    if (_playChannels[i] is not null && _playChannels[i].Valid)
        //    //    {
        //    //        Kill(i);
        //    //    }
        //    //}
        //}

        /// <summary>
        /// Populate the click grid.
        /// </summary>
        void InitChannelsGrid()
        {
            gridChannels.Clear();
            int i = 0;

            foreach(var ch in _allChannels)
            {
                string name = "";

                if(ch.Patch >= 0)
                {
                    name = MidiDefs.GetInstrumentName(ch.Patch);
                }

                if (ch.IsDrums)
                {
                    name = "Drums";
                }

                if(name != "")
                {
                    gridChannels.AddIndicator($"Ch{i + 1} {name}", i);
                }
                i++;
            }

            gridChannels.Show(2, gridChannels.Width / 2, 20);
        }
        #endregion

        #region UI event handlers
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Channels_IndicatorEvent(object? sender, IndicatorEventArgs e)
        {
            int chind = e.Id + 1;
            int chnum = e.Id + 1;

            // Update internal state.
            var newState = (ChannelState)e.State;
            _allChannels.SetChannelState(chnum, newState);

            // Update UI.
            bool anySolo = _allChannels.AnySolo;

            switch (newState)
            {
                case ChannelState.Normal:
                    break;

                case ChannelState.Solo:
                    // Mute any other non-solo channels.
                    foreach (var ch in _allChannels)
                    {
                        if (ch.ChannelNumber != chnum && ch.State != ChannelState.Solo)
                        {
                            _player.Kill(chnum);
                        }
                    }
                    break;

                case ChannelState.Mute:
                    _player.Kill(chnum);
                    break;
            }
            gridChannels.SetIndicator(chind, (int)newState);
        }

        /// <summary>
        /// User changed tempo.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Tempo_ValueChanged(object? sender, EventArgs e)
        {
            _tempo = (int)sldTempo.Value;
            SetTimer();
        }

        /// <summary>
        /// Convert tempo to period and set mm timer.
        /// </summary>
        void SetTimer()
        {
            MidiTime mt = new()
            {
                InternalPpq = PPQ,
                MidiPpq = _mdata.DeltaTicksPerQuarterNote,
                Tempo = sldTempo.Value
            };

            double period = mt.RoundedInternalPeriod();
            _mmTimer.SetTimer((int)Math.Round(period), MmTimerCallback);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void BarBar_CurrentTimeChanged(object? sender, EventArgs e)
        {
        }

        /// <summary>
        /// Sometimes drums are on channel 1, usually if it's the only channel in a clip file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DrumChannel_SelectedIndexChanged(object? sender, EventArgs e)
        {
            int drumChannel = cmbDrumChannel.SelectedIndex;
            _allChannels.ForEach(ch => ch.IsDrums = ch.ChannelNumber == drumChannel);

            // Update UI.
            InitChannelsGrid();
        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //void Kill_Click(object? sender, EventArgs e)
        //{
        //    KillAll();
        //}
    }
    #endregion

    /// <summary>Channel events and other properties.</summary>
    public class PlayChannel_XXX
    {
        #region Properties
        /// <summary>Actual 1-based midi channel number for UI.</summary>
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

        ///<summary>The main collection of events. The key is the subdiv/time.</summary>
        public Dictionary<int, List<MidiEvent>> Events { get; set; } = new Dictionary<int, List<MidiEvent>>();

        ///<summary>The duration of the whole channel.</summary>
        public int MaxSubdiv { get; private set; } = 0;
        #endregion

        /// <summary>Add an event at the given subdiv.</summary>
        /// <param name="subdiv"></param>
        /// <param name="evt"></param>
        public void AddEvent(int subdiv, MidiEvent evt)
        {
            if (!Events.ContainsKey(subdiv))
            {
                Events.Add(subdiv, new List<MidiEvent>());
            }
            Events[subdiv].Add(evt);
            MaxSubdiv = Math.Max(MaxSubdiv, subdiv);
            HasNotes |= evt is NoteEvent;
        }

        /// <summary>
        /// Clean up before reading again.
        /// </summary>
        public void Reset()
        {
            HasNotes = false;
            Events.Clear();
            MaxSubdiv = 0;
        }

        /// <summary>For viewing pleasure.</summary>
        public override string ToString()
        {
            return $"PlayChannel: Name:{Name} ChannelNumber:{ChannelNumber} Mode:{Mode} Events:{Events.Count} MaxSubdiv:{MaxSubdiv}";
        }
    }
}
