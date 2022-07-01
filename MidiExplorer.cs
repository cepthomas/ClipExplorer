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
    /// There are some limitations: Windows multimedia timer has 1 msec resolution at best. This causes a trade-off between
    /// ppq resolution and accuracy. The timer is also inherently wobbly.
    /// </summary>
    public partial class MidiExplorer : UserControl, IExplorer
    {
        #region Fields
        /// <summary>My logger.</summary>
        readonly Logger _logger = LogManager.CreateLogger("MidiExplorer");

        /// <summary>Midi player.</summary>
        readonly MidiPlayer _player;

        /// <summary>The internal channel objects.</summary>
        readonly ChannelManager _channelManager = new();

        /// <summary>All the channel controls.</summary>
        readonly List<PlayerControl> _playerControls = new();

        /// <summary>The fast timer.</summary>
        readonly MmTimerEx _mmTimer = new();

        /// <summary>Midi events from the input file.</summary>
        MidiDataFile _mdata = new();
        #endregion

        #region Events
        /// <inheritdoc />
        public event EventHandler? PlaybackCompleted;

        ///// <inheritdoc />
        //public event EventHandler<LogEventArgs>? Log;
        #endregion

        #region Properties
        /// <inheritdoc />
        public bool Valid { get { return _player.Valid; } }

        /// <inheritdoc />
        public double Volume { get { return _player.Volume; } set { _player.Volume = value; } }

        /// <inheritdoc />
        public bool Playing { get { return _player.Playing; } }
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

            _player = new(Common.Settings.MidiSettings.MidiOutDevice, _channelManager);

            // Init UI.
            toolStrip1.Renderer = new NBagOfUis.CheckBoxRenderer() { SelectedColor = Common.Settings.ControlColor };

            // Time controls.
            barBar.ProgressColor = Common.Settings.ControlColor;
            sldTempo.DrawColor = Common.Settings.ControlColor;
            sldTempo.Resolution = Common.Settings.TempoResolution;

            // Hook up some simple UI handlers.
            btnKillMidi.Click += (_, __) => { _player.KillAll(); };
            btnLogMidi.Click += (_, __) => { _player.LogMidi = btnLogMidi.Checked; };
            sldTempo.ValueChanged += (_, __) => { SetTimer(); };
            barBar.CurrentTimeChanged += (_, __) => { _player.CurrentSubdiv = barBar.Current.TotalSubdivs; };

            // Set up timer.
            sldTempo.Value = Common.Settings.MidiSettings.DefaultTempo;
            SetTimer();

            // Init channels and selectors.
            _channelManager.ForEach(ch => ch.IsDrums = ch.ChannelNumber == MidiDefs.DEFAULT_DRUM_CHANNEL);
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

            _player.Run(false);
            _player.Dispose();

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
                _channelManager.Reset();
                _mdata = new MidiDataFile();

                // Process the file. Set the default tempo from preferences.
                _mdata.Read(fn, Common.Settings.MidiSettings.DefaultTempo, false);

                // Init new stuff with contents of file/pattern.
                lbPatterns.Items.Clear();
                var pnames = _mdata.GetPatternNames();

                switch (pnames.Count)
                {
                    case 0:
                        _logger.Error($"Something wrong with this file: {fn}");
                        ok = false;
                        break;

                    case 1:
                        var pinfo = _mdata.GetPattern(pnames[0]);
                        LoadPattern(pinfo!);
                        break;

                    default: // style has multiple patterns.
                        pnames.ForEach(pn =>
                        {
                            var p = _mdata.GetPattern(pn);
                            switch (p!.PatternName)
                            {
                                // These don't contain a pattern.
                                case "SFF1":
                                case "SFF2":
                                case "SInt": // Initial patches are in here.
                                    break;

                                case "":
                                    _logger.Error("Well, this should never happen!");
                                    break;

                                default:
                                    lbPatterns.Items.Add(p.PatternName);
                                    break;
                            }
                        });
                        break;
                }

                Rewind();
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
            _player.Run(true);
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
            _player.CurrentSubdiv = 0;
            barBar.Current = new(0);
        }
        #endregion

        #region Misc functions
        /// <inheritdoc />
        public bool SettingsChanged()
        {
            bool ok = true;

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
                // Bump time. Check for end of play. Client will take care of transport control.
                barBar.IncrementCurrent(1);
                if (_player.DoNextStep())
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
        /// The user clicked something in one of the player controls.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Control_ChannelChangeEvent(object? sender, ChannelChangeEventArgs e)
        {
            PlayerControl chc = (PlayerControl)sender!;

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
            _playerControls.ForEach(c => Controls.Remove(c));
            _playerControls.Clear();

            // Create the new controls.
            int x = sldTempo.Right + 5;
            int y = sldTempo.Top;

            // For scaling subdivs to internal.
            MidiTimeConverter mt = new(_mdata.DeltaTicksPerQuarterNote, Common.Settings.MidiSettings.DefaultTempo);
            sldTempo.Value = pinfo.Tempo;

            for (int i = 0; i < MidiDefs.NUM_CHANNELS; i++)
            {
                int chnum = i + 1;

                var chEvents = pinfo.GetFilteredEvents(new() { chnum }).Where(e => e.MidiEvent is NoteEvent || e.MidiEvent is NoteOnEvent);

                // Is this channel pertinent?
                if (chEvents.Any())
                {
                    _channelManager.SetEvents(chnum, chEvents);

                    // Make new control.
                    PlayerControl control = new()
                    {
                        Location = new(x, y),
                        BorderStyle = BorderStyle.FixedSingle
                    };

                    // Bind to internal channel object.
                    _channelManager.Bind(chnum, control);

                    // Now init the control - after binding!
                    control.Patch = pinfo.Patches[i];
                    control.ChannelChangeEvent += Control_ChannelChangeEvent;
                    Controls.Add(control);
                    _playerControls.Add(control);

                    // Adjust positioning.
                    y += control.Height + 5;

                    // Send patch maybe. These can change per pattern.
                    _player.SendPatch(chnum, pinfo.Patches[i]);
                }
            }

            barBar.Start = new(0);
            barBar.End = new(_channelManager.TotalSubdivs - 1);
            barBar.Length = new(_channelManager.TotalSubdivs);
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
            _playerControls.ForEach(ctl => ctl.IsDrums =
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
                _playerControls.Where(cc => cc.Selected).ForEach(cc => channels.Add(cc.BoundChannel));
                if (!channels.Any()) // grab them all.
                {
                    _playerControls.ForEach(cc => channels.Add(cc.BoundChannel));
                }

                switch (stext)
                {
                    case "All":
                        {
                            var newfn = MakeExportFileName(Common.OutPath, _mdata.FileName, "all", "csv");
                            MidiExport.ExportAllEvents(newfn, patterns, channels, MakeMeta());
                            _logger.Info($"Exported to {newfn}");
                        }
                        break;

                    case "Pattern":
                        {
                            foreach (var pattern in patterns)
                            {
                                var newfn = MakeExportFileName(Common.OutPath, _mdata.FileName, pattern.PatternName, "csv");
                                MidiExport.ExportGroupedEvents(newfn, pattern, channels, MakeMeta(), true); //includeAll
                                _logger.Info($"Exported pattern {pattern.PatternName} to {newfn}");
                            }
                        }
                        break;

                    case "Midi":
                        foreach (var pattern in patterns)
                        {
                            var newfn = MakeExportFileName(Common.OutPath, _mdata.FileName, pattern.PatternName, "mid");
                            MidiExport.ExportMidi(newfn, pattern, channels, MakeMeta());
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

        /// <summary>
        /// Create a new clean filename for export. Creates path if it doesn't exist.
        /// </summary>
        /// <param name="path">Export path</param>
        /// <param name="baseFn">Root of the new file name</param>
        /// <param name="mod">Modifier</param>
        /// <param name="ext">File extension</param>
        /// <returns></returns>
        public string MakeExportFileName(string path, string baseFn, string mod, string ext)
        {
            string name = Path.GetFileNameWithoutExtension(baseFn);

            // Clean the file name.
            name = name.Replace('.', '-').Replace(' ', '_');
            mod = mod == "" ? "default" : mod.Replace(' ', '_');
            var newfn = Path.Join(path, $"{name}_{mod}.{ext}");
            return newfn;
        }

        /// <summary>
        /// Utility to contain midi file meta info.
        /// </summary>
        /// <returns></returns>
        Dictionary<string, int> MakeMeta()
        {
            Dictionary<string, int> meta = new()
            {
                { "MidiFileType", _mdata.MidiFileType },
                { "DeltaTicksPerQuarterNote", _mdata.DeltaTicksPerQuarterNote },
                { "NumTracks", _mdata.NumTracks }
            };

            return meta;
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
