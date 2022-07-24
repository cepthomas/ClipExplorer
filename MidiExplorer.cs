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
using NBagOfTricks.Slog;
using NBagOfUis;
using MidiLib;


namespace ClipExplorer
{
    /// <summary>
    /// A "good enough" midi player.
    /// </summary>
    public partial class MidiExplorer : UserControl, IExplorer
    {
        #region Fields
        /// <summary>My logger.</summary>
        readonly Logger _logger = LogManager.CreateLogger("MidiExplorer");

        /// <summary>Midi output.</summary>
        readonly IOutputDevice _outputDevice = new NullOutputDevice();

        /// <summary>All the channels - key is user assigned name.</summary>
        readonly Dictionary<string, Channel> _channels = new();

        /// <summary>All the channel controls.</summary>
        readonly List<ChannelControl> _channelControls = new();

        /// <summary>The fast timer.</summary>
        readonly MmTimerEx _mmTimer = new();

        /// <summary>Midi events from the input file.</summary>
        MidiDataFile _mdata = new();
        #endregion

        #region Events
        /// <inheritdoc />
        public event EventHandler? PlaybackCompleted;
        #endregion

        #region Properties
        /// <inheritdoc />
        public bool Valid { get { return _outputDevice is not NullOutputDevice; } }

        /// <inheritdoc />
        public double Volume { get; set; }

        /// <inheritdoc />
        public bool Playing { get { return _mmTimer.Running; } }
        #endregion

        #region Lifecycle
        /// <summary>
        /// Normal constructor.
        /// </summary>
        public MidiExplorer()
        {
            InitializeComponent();

            // Init settings.
            UpdateSettings();

            // Init UI.
            toolStrip1.Renderer = new NBagOfUis.CheckBoxRenderer() { SelectedColor = Common.Settings.ControlColor };

            // Time controls.
            barBar.ProgressColor = Common.Settings.ControlColor;
            sldTempo.DrawColor = Common.Settings.ControlColor;
            sldTempo.Resolution = Common.Settings.TempoResolution;

            // Hook up some simple UI handlers.
            btnKillMidi.Click += (_, __) => { _channels.Values.ForEach(ch => ch.Kill()); };
            btnLogMidi.Click += (_, __) => { _outputDevice.LogEnable = btnLogMidi.Checked; };
            sldTempo.ValueChanged += (_, __) => { SetTimer(); };

            // Set up timer.
            sldTempo.Value = Common.Settings.MidiSettings.DefaultTempo;
            SetTimer();

            // Set up output device.
            foreach (var dev in Common.Settings.MidiSettings.OutputDevices)
            {
                // Try midi.
                _outputDevice = new MidiOutput(dev.DeviceName);
                if (_outputDevice.Valid)
                {
                    break;
                }
            }
            if (!_outputDevice.Valid)
            {
                _logger.Error($"Something wrong with your output device:{_outputDevice.DeviceName}");
            }

            // Init channels and selectors.
            cmbDrumChannel1.Items.Add("NA");
            cmbDrumChannel2.Items.Add("NA");
            for (int i = 1; i <= MidiDefs.NUM_CHANNELS; i++)
            {
                cmbDrumChannel1.Items.Add(i);
                cmbDrumChannel2.Items.Add(i);
            }
            cmbDrumChannel1.SelectedIndex = MidiDefs.DEFAULT_DRUM_CHANNEL;
            cmbDrumChannel2.SelectedIndex = 0;

            Visible = false;
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

            _outputDevice.Dispose();

            // Wait a bit in case there are some lingering events.
            System.Threading.Thread.Sleep(100);

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
                _mdata = new MidiDataFile();

                // Process the file. Set the default tempo from preferences.
                _mdata.Read(fn, Common.Settings.MidiSettings.DefaultTempo, false);

                // Init new stuff with contents of file/pattern.
                lbPatterns.Items.Clear();
                var pnames = _mdata.GetPatternNames();

                if (pnames.Count > 0)
                {
                    pnames.ForEach(pn => { lbPatterns.Items.Add(pn); });
                }
                else
                {
                    throw new InvalidOperationException($"Something wrong with this file: {fn}");
                }

                Rewind();

                // Pick first.
                lbPatterns.SelectedIndex = 0;

                // Set up timer default.
                sldTempo.Value = 100;
            
                midiToolStripMenuItem.Enabled = _mdata.IsStyleFile;
            }
            catch (Exception ex)
            {
                _logger.Error($"Couldn't open the file: {fn} because: {ex.Message}");
                ok = false;
            }

            return ok;
        }
        #endregion

        #region Play functions
        /// <inheritdoc />
        public void Play()
        {
            _mmTimer.Start();
        }

        /// <inheritdoc />
        public void Stop()
        {
            _mmTimer.Stop();
            // Send midi stop all notes just in case.
            _channels.Values.ForEach(ch => ch.Kill());
        }

        /// <inheritdoc />
        public void Rewind()
        {
            barBar.Current = new(0);
        }
        #endregion

        #region Misc functions
        /// <inheritdoc />
        public bool UpdateSettings()
        {
            sldTempo.Resolution = Common.Settings.TempoResolution;
            return true;
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
                // Bump time. Check for end of play. Client will take care of transport control.
                if (DoNextStep())
                {
                    PlaybackCompleted?.Invoke(this, new EventArgs());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Synchronously outputs the next midi events. Does solo/mute.
        /// This is running on the background thread.
        /// </summary>
        /// <returns>True if sequence completed.</returns>
        public bool DoNextStep()
        {
            bool done = false;

            // Any soloes?
            bool anySolo = _channels.AnySolo();

            // Process each channel.
            foreach (var ch in _channels.Values)
            {
                // Look for events to send. Any explicit solos?
                if (ch.State == ChannelState.Solo || (!anySolo && ch.State == ChannelState.Normal))
                {
                    // Process any sequence steps.
                    var playEvents = ch.GetEvents(barBar.Current.TotalSubdivs);
                    foreach (var mevt in playEvents)
                    {
                        switch (mevt)
                        {
                            case NoteOnEvent evt:
                                if (ch.IsDrums && evt.Velocity == 0)
                                {
                                    // Skip drum noteoffs as windows GM doesn't like them.
                                }
                                else
                                {
                                    // Adjust volume. Redirect drum channel to default.
                                    NoteOnEvent ne = new(
                                        evt.AbsoluteTime,
                                        ch.IsDrums ? MidiDefs.DEFAULT_DRUM_CHANNEL : evt.Channel,
                                        evt.NoteNumber,
                                        Math.Min((int)(evt.Velocity * Volume * ch.Volume), MidiDefs.MAX_MIDI),
                                        evt.OffEvent is null ? 0 : evt.NoteLength); // Fix NAudio NoteLength bug.

                                    ch.SendEvent(ne);
                                }
                                break;

                            case NoteEvent evt: // aka NoteOff
                                if (ch.IsDrums)
                                {
                                    // Skip drum noteoffs as windows GM doesn't like them.
                                }
                                else
                                {
                                    ch.SendEvent(evt);
                                }
                                break;

                            default:
                                // Everything else as is.
                                ch.SendEvent(mevt);
                                break;
                        }
                    }
                }
            }

            // Bump time. Check for end of play.
            done = barBar.IncrementCurrent(1);

            return done;
        }
        #endregion

        #region UI event handlers
        /// <summary>
        /// The user clicked something in one of the player controls.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Control_ChannelChangeEvent(object? sender, ChannelChangeEventArgs e)
        {
            Channel channel = ((ChannelControl)sender!).BoundChannel;

            if (e.StateChange)
            {
                switch (channel.State)
                {
                    case ChannelState.Normal:
                        break;

                    case ChannelState.Solo:
                        // Mute any other non-solo channels.
                        _channels.Values.ForEach(ch =>
                        {
                            if (channel.ChannelNumber != ch.ChannelNumber && channel.State != ChannelState.Solo)
                            {
                                channel.Kill();
                            }
                        });
                        break;

                    case ChannelState.Mute:
                        channel.Kill();
                        break;
                }
            }

            if (e.PatchChange && channel.Patch >= 0)
            {
                channel.SendPatch();
            }
        }
        #endregion

        #region Process patterns
        /// <summary>
        /// Load the requested pattern and create controls.
        /// </summary>
        /// <param name="pinfo"></param>
        void LoadPattern(PatternInfo pinfo)
        {
            Stop();

            // Clean out our current elements.
            _channelControls.ForEach(c =>
            {
                Controls.Remove(c);
                c.Dispose();
            });
            _channelControls.Clear();
            _channels.Clear();

            // Load the new one.
            if (pinfo is null)
            {
                _logger.Error($"Invalid pattern!");
            }
            else
            {
                // Create the new controls.
                int x = sldTempo.Right + 5;
                int y = sldTempo.Top;

                // For scaling subdivs to internal.
                MidiTimeConverter mt = new(_mdata.DeltaTicksPerQuarterNote, Common.Settings.MidiSettings.DefaultTempo);
                sldTempo.Value = pinfo.Tempo;

                foreach(var (number, patch) in pinfo.GetChannels(true, true))
                {
                    // Get events for the channel.
                    var chEvents = pinfo.GetFilteredEvents(new List<int>() { number });

                    // Is this channel pertinent?
                    if (chEvents.Any())
                    {
                        // Make new channel.
                        Channel channel = new()
                        {
                            ChannelName = $"chan{number}",
                            ChannelNumber = number,
                            Device = _outputDevice,
                            DeviceId = _outputDevice.DeviceName,
                            Volume = MidiLibDefs.VOLUME_DEFAULT,
                            State = ChannelState.Normal,
                            Patch = patch,
                            IsDrums = number == MidiDefs.DEFAULT_DRUM_CHANNEL,
                            Selected = false,
                        };
                        _channels.Add(channel.ChannelName, channel);
                        channel.SetEvents(chEvents);

                        // Make new control and bind to channel.
                        ChannelControl control = new()
                        {
                            Location = new(x, y),
                            BorderStyle = BorderStyle.FixedSingle,
                            BoundChannel = channel
                        };
                        control.ChannelChangeEvent += Control_ChannelChangeEvent;
                        Controls.Add(control);
                        _channelControls.Add(control);

                        // Good time to send initial patch.
                        channel.SendPatch();

                        // Adjust positioning.
                        y += control.Height + 5;
                    }
                }

                // Set timer.
                sldTempo.Value = pinfo.Tempo;
            }

            // Update bar.
            var tot = _channels.TotalSubdivs();
            barBar.Start = new(0);
            barBar.End = new(tot - 1);
            barBar.Length = new(tot);
            barBar.Current = new(0);

            UpdateDrumChannels();
        }

        /// <summary>
        /// Load pattern selection.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Patterns_SelectedIndexChanged(object? sender, EventArgs e)
        {
            var pinfo = _mdata.GetPattern(lbPatterns.SelectedItem.ToString()!);

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
                // Get selected patterns.
                List<string> patternNames = new();
                if (lbPatterns.Items.Count == 1)
                {
                    patternNames.Add(lbPatterns.Items[0].ToString()!);
                }
                else if (lbPatterns.CheckedItems.Count > 0)
                {
                    foreach (var p in lbPatterns.CheckedItems)
                    {
                        patternNames.Add(p.ToString()!);
                    }
                }
                else
                {
                    _logger.Warn("Please select at least one pattern");
                    return;
                }
                List<PatternInfo> patterns = new();
                patternNames.ForEach(p => patterns.Add(_mdata.GetPattern(p)!));

                // Get selected channels.
                List<Channel> channels = new();
                _channelControls.Where(cc => cc.Selected).ForEach(cc => channels.Add(cc.BoundChannel));
                if (!channels.Any()) // grab them all.
                {
                    _channelControls.ForEach(cc => channels.Add(cc.BoundChannel));
                }

                switch (stext.ToLower())
                {
                    case "csv":
                        {
                            var newfn = MiscUtils.MakeExportFileName(Common.OutPath, _mdata.FileName, "all", "csv");
                            MidiExport.ExportCsv(newfn, patterns, channels, _mdata.GetGlobal());
                            _logger.Info($"Exported to {newfn}");
                        }
                        break;

                    case "midi":
                        foreach (var pattern in patterns)
                        {
                            var newfn = MiscUtils.MakeExportFileName(Common.OutPath, _mdata.FileName, pattern.PatternName, "mid");
                            MidiExport.ExportMidi(newfn, pattern, channels, _mdata.GetGlobal());
                            _logger.Info($"Export midi to {newfn}");
                        }
                        break;

                    default:
                        _logger.Error($"Ooops: {stext}");
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"{ex.Message}");
            }
        }
        #endregion

        #region Misc functions
        /// <summary>
        /// Convert tempo to period and set mm timer accordingly.
        /// </summary>
        void SetTimer()
        {
            MidiTimeConverter mt = new(_mdata.DeltaTicksPerQuarterNote, sldTempo.Value);
            double period = mt.RoundedInternalPeriod();
            _mmTimer.SetTimer((int)Math.Round(period), MmTimerCallback);
        }
        #endregion
    }
}
