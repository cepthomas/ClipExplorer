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
    public partial class MidiExplorer : UserControl, IExplorer
    {
        #region Constants
        /// <summary>Only 4/4 time supported.</summary>
        const int BEATS_PER_BAR = 4;

        /// <summary>Our internal ppq aka resolution - used for sending realtime midi messages.</summary>
        const int PPQ = 32;
        #endregion

        #region Fields
        /// <summary>Midi player.</summary>
        readonly MidiPlayer _player;

        /// <summary>The internal channel objects.</summary>
        readonly ChannelCollection _allChannels = new();

        /// <summary>All the channel controls.</summary>
        readonly List<ChannelControl> _channelControls = new();

        /// <summary>The fast timer.</summary>
        readonly MmTimerEx _mmTimer = new();

        /// <summary>Midi events from the input file.</summary>
        readonly MidiData _mdata = new();
        #endregion

        #region Events
        /// <inheritdoc />
        public event EventHandler? PlaybackCompleted;

        /// <inheritdoc />
        public event EventHandler<LogEventArgs>? Log;
        #endregion

        #region Properties
        /// <inheritdoc />
        public double Volume { get { return _player.Volume; } set { _player.Volume = value; } }

        /// <inheritdoc />
        public PlayState State { get { return (PlayState)_player.State; } set { _player.State = (MidiState)value; } }
        #endregion

        #region Lifecycle
        /// <summary>
        /// Normal constructor.
        /// </summary>
        public MidiExplorer()
        {
            InitializeComponent();

            // Init settings.
            SettingsChanged();

            _player = new(Common.Settings.MidiOutDevice, _allChannels);

            // Init UI.
            toolStrip1.Renderer = new NBagOfUis.CheckBoxRenderer() { SelectedColor = Common.Settings.ControlColor };

            // Time controller.
            barBar.ZeroBased = Common.Settings.ZeroBased;
            barBar.BeatsPerBar = BEATS_PER_BAR;
            barBar.SubdivsPerBeat = PPQ;
            barBar.Snap = Common.Settings.Snap;
            barBar.ProgressColor = Common.Settings.ControlColor;
            barBar.CurrentTimeChanged += BarBar_CurrentTimeChanged;

            sldTempo.DrawColor = Common.Settings.ControlColor;
            sldTempo.Resolution = Common.Settings.TempoResolution;

            // Hook up some simple UI handlers.
            btnKillMidi.Click += (_, __) => { _player.KillAll(); };
            btnLogMidi.Click += (_, __) => { _player.LogMidi = btnLogMidi.Checked; };
            sldTempo.ValueChanged += (_, __) => { SetTimer(); };

            // Set up timer.
            sldTempo.Value = Common.Settings.DefaultTempo;
            SetTimer();

            // Init channels and selectors.
            _allChannels.ForEach(ch => ch.IsDrums = ch.ChannelNumber == MidiDefs.DEFAULT_DRUM_CHANNEL);
            cmbDrumChannel1.Items.Add("NA");
            cmbDrumChannel2.Items.Add("NA");
            for (int i = 1; i <= MidiDefs.NUM_CHANNELS; i++)
            {
                cmbDrumChannel1.Items.Add(i);
                cmbDrumChannel2.Items.Add(i);
            }
            cmbDrumChannel1.SelectedIndex = MidiDefs.DEFAULT_DRUM_CHANNEL;
            cmbDrumChannel1.SelectedIndexChanged += DrumChannel_SelectedIndexChanged;
            cmbDrumChannel2.SelectedIndex = 0;
            cmbDrumChannel2.SelectedIndexChanged += DrumChannel_SelectedIndexChanged;
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
            _mmTimer.Stop();
            _mmTimer.Dispose();

            _player?.Run(false);
            _player?.Dispose();

            if (disposing)
            {
                components?.Dispose();
            }

            base.Dispose(disposing);
        }
        #endregion

        #region File functions
        /// <inheritdoc />
        public bool OpenFile(string fn)
        {
            bool ok = true;

            try
            {
                // Reset stuff.
                cmbDrumChannel1.SelectedIndex = MidiDefs.DEFAULT_DRUM_CHANNEL;
                cmbDrumChannel2.SelectedIndex = 0;
                _allChannels.Reset();

                // Process the file. Set the default tempo from preferences.
                _mdata.Read(fn, Common.Settings.DefaultTempo, false);

                // Init new stuff with contents of file/pattern.
                lbPatterns.Items.Clear();

                if(_mdata.AllPatterns.Count == 0)
                {
                    LogMessage("ERR", $"Something wrong with this file: {fn}");
                    ok = false;
                }
                else if(_mdata.AllPatterns.Count == 1) // plain midi
                {
                    var pinfo = _mdata.AllPatterns[0];
                    LoadPattern(pinfo);
                }
                else // style - multiple patterns.
                {
                    foreach (var p in _mdata.AllPatterns)
                    {
                        switch (p.PatternName)
                        {
                            // These don't contain a pattern.
                            case "SFF1": // initial patches are in here
                            case "SFF2":
                            case "SInt":
                                break;

                            case "":
                                LogMessage("ERR", "Well, this should never happen!");
                                break;

                            default:
                                lbPatterns.Items.Add(p.PatternName);
                                break;
                        }
                    }

                    if (lbPatterns.Items.Count > 0)
                    {
                        lbPatterns.SelectedIndex = 0;
                    }
                }

                Rewind();

            }
            catch (Exception ex)
            {
                LogMessage("ERR", $"Couldn't open the file: {fn} because: {ex.Message}");
                ok = false;
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
        #endregion

        #region UI event handlers
        /// <summary>
        /// The user clicked something in one of the channel controls.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Control_ChannelChange(object? sender, ChannelControl.ChannelChangeEventArgs e)
        {
            ChannelControl chc = (ChannelControl)sender!;

            if (e.StateChange)
            {
                switch (chc.State)
                {
                    case ChannelState.Normal:
                        break;

                    case ChannelState.Solo:
                        // Mute any other non-solo channels.
                        for (int i = 0; i < MidiDefs.NUM_CHANNELS; i++)
                        {
                            int chnum = i + 1;
                            if (chnum != chc.ChannelNumber && chc.State != ChannelState.Solo)
                            {
                                _player.Kill(chnum);
                            }
                        }
                        break;

                    case ChannelState.Mute:
                        _player.Kill(chc.ChannelNumber);
                        break;
                }
            }

            if (e.PatchChange && chc.Patch >= 0)
            {
                _player.SendPatch(chc.ChannelNumber, chc.Patch);
            }
        }

        /// <summary>
        /// User changed tempo.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Tempo_ValueChanged(object? sender, EventArgs e)
        {
            //_tempo = (int)sldTempo.Value;
            SetTimer();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void BarBar_CurrentTimeChanged(object? sender, EventArgs e)
        {
        }
        #endregion

        #region Process patterns
        /// <summary>
        /// Load the requested pattern and create controls.
        /// </summary>
        /// <param name="pinfo"></param>
        void LoadPattern(PatternInfo pinfo)
        {
            _player.Reset();

            // Clean out our controls collection.
            _channelControls.ForEach(c => Controls.Remove(c));
            _channelControls.Clear();

            // Create the new controls.
            int lastSubdiv = 0;
            int x = sldTempo.Right + 5;
            int y = sldTempo.Top;

            // For scaling subdivs to internal.
            MidiLib.MidiTime mt = new()
            {
                InternalPpq = PPQ,
                MidiPpq = _mdata.DeltaTicksPerQuarterNote,
                Tempo = Common.Settings.DefaultTempo
            };

            sldTempo.Value = pinfo.Tempo;

            for (int i = 0; i < MidiDefs.NUM_CHANNELS; i++)
            {
                int chnum = i + 1;

                var chEvents = _mdata.AllEvents.
                    Where(e => e.PatternName == pinfo.PatternName && e.ChannelNumber == chnum && (e.MidiEvent is NoteEvent || e.MidiEvent is NoteOnEvent)).
                    OrderBy(e => e.AbsoluteTime);

                // Is this channel pertinent?
                if (chEvents.Any())
                {
                    _allChannels.SetEvents(chnum, chEvents, mt);

                    // Make new control.
                    ChannelControl control = new() { Location = new(x, y) };

                    // Bind to internal channel object.
                    _allChannels.Bind(chnum, control);

                    // Now init the control - after binding!
                    control.Patch = pinfo.Patches[i];
                    //control.IsDrums = GetDrumChannels().Contains(chnum);

                    control.ChannelChange += Control_ChannelChange;
                    Controls.Add(control);
                    _channelControls.Add(control);

                    lastSubdiv = Math.Max(lastSubdiv, control.MaxSubdiv);

                    // Adjust positioning.
                    y += control.Height + 5;

                    // Send patch maybe. These can change per pattern.
                    _player.SendPatch(chnum, pinfo.Patches[i]);
                }
            }

            barBar.Length = new BarSpan(lastSubdiv);
            barBar.Start = BarSpan.Zero;
            barBar.End = barBar.Length - BarSpan.OneSubdiv;
            barBar.Current = BarSpan.Zero;

            UpdateDrumChannels();
        }

        /// <summary>
        /// Load pattern selection.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Patterns_SelectedIndexChanged(object? sender, EventArgs e)
        {
            var pinfo = _mdata.AllPatterns.Where(p => p.PatternName == lbPatterns.SelectedItem.ToString()).First();

            LoadPattern(pinfo!);

            Rewind();

            if (Common.Settings.Autoplay)
            {
                Play();
            }
        }

        /// <summary>
        /// Pattern selection.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void AllOrNone_Click(object? sender, EventArgs e)
        {
            bool check = sender == btnAllPatterns;
            for(int i = 0; i < lbPatterns.Items.Count; i++)
            {
                lbPatterns.SetItemChecked(i, check);                
            }
        }
        #endregion

        #region Drum channel
        /// <summary>
        /// User changed the drum channel.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DrumChannel_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateDrumChannels();
        }

        /// <summary>
        /// Update all channels based on current UI.
        /// </summary>
        void UpdateDrumChannels()
        {
            _channelControls.ForEach(ctl => ctl.IsDrums =
                (ctl.ChannelNumber == cmbDrumChannel1.SelectedIndex) ||
                (ctl.ChannelNumber == cmbDrumChannel2.SelectedIndex));
        }
        #endregion

        #region Export
        /// <summary>
        /// Export current file to human readable or midi.
        /// </summary>
        void Export_Click(object? sender, EventArgs e)
        {
            var stext = ((ToolStripMenuItem)sender!).Text;

            try
            {
                // Collect filters.
                List<string> patternNames = new();
                foreach (var p in lbPatterns.CheckedItems)
                {
                    patternNames.Add(p.ToString()!);
                }

                List<int> channels = new();
                foreach (var cc in _channelControls.Where(c => c.Selected))
                {
                    channels.Add(cc.ChannelNumber);
                }

                switch (stext)
                {
                    case "All":
                        {
                            var s = _mdata.ExportAllEvents(Common.ExportPath, channels);
                            LogMessage("INF", $"Exported to {s}");
                        }
                        break;

                    case "Pattern":
                        {
                            if (_mdata.AllPatterns.Count == 1)
                            {
                                var s = _mdata.ExportGroupedEvents(Common.ExportPath, "", channels, true);
                                LogMessage("INF", $"Exported default to {s}");
                            }
                            else
                            {
                                foreach (var patternName in patternNames)
                                {
                                    var s = _mdata.ExportGroupedEvents(Common.ExportPath, patternName, channels, true);
                                    LogMessage("INF", $"Exported pattern {patternName} to {s}");
                                }
                            }
                        }
                        break;

                    case "Midi":
                        {
                            if (_mdata.AllPatterns.Count == 1)
                            {
                                // Use original ppq.
                                var s = _mdata.ExportMidi(Common.ExportPath, "", channels, _mdata.DeltaTicksPerQuarterNote);
                                LogMessage("INF", $"Export midi to {s}");
                            }
                            else
                            {
                                foreach (var patternName in patternNames)
                                {
                                    // Use original ppq.
                                    var s = _mdata.ExportMidi(Common.ExportPath, patternName, channels, _mdata.DeltaTicksPerQuarterNote);
                                    LogMessage("INF", $"Export midi to {s}");
                                }
                            }
                        }
                        break;

                    default:
                        LogMessage("ERR", $"Ooops: {stext}");
                        break;
                }
            }
            catch (Exception ex)
            {
                LogMessage("ERR", $"{ex.Message}");
            }
        }
        #endregion

        #region Misc functions
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
        #endregion
    }
}
