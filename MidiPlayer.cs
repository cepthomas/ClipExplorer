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
        /// <summary>Only 4/4 time supported.</summary>
        const int BEATS_PER_BAR = 4;

        /// <summary>Our internal ppq aka resolution - used for sending realtime midi messages.</summary>
        const int PPQ = 32;
        #endregion

        #region Fields
        /// <summary>Midi output device.</summary>
        MidiOut _midiOut = null;

        /// <summary>The fast timer.</summary>
        MmTimerEx _mmTimer = new MmTimerEx();

        /// <summary>Indicates whether or not the midi is playing.</summary>
        bool _running = false;

        /// <summary>Midi events from the input file.</summary>
        MidiFile _mfile;

        /// <summary>All the channels.</summary>
        readonly PlayChannel[] _playChannels = new PlayChannel[MidiDefs.NUM_CHANNELS];

        /// <summary>Requested tempo from file. Use default if not supplied.</summary>
        double _tempo = Common.Settings.DefaultTempo;

        /// <summary>Some midi files have drums on a different channel so allow the user to re-map.</summary>
        int _drumChannel = MidiDefs.DEFAULT_DRUM_CHANNEL;
        #endregion

        #region Events
        /// <inheritdoc />
        public event EventHandler PlaybackCompleted;

        /// <inheritdoc />
        public event EventHandler<LogEventArgs> Log;
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
        void MidiPlayer_Load(object sender, EventArgs e)
        {
            // Init internal structure.
            for (int i = 0; i < _playChannels.Count(); i++)
            {
                _playChannels[i] = new PlayChannel() { ChannelNumber = i + 1 };
            }

            // Fill patch list.
            for (int i = 0; i <= MidiDefs.MAX_MIDI; i++)
            {
                cmbPatchList.Items.Add(MidiDefs.GetInstrumentDef(i));
            }
            cmbPatchList.SelectedIndex = 0;

            SettingsChanged();

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
            cgChannels.AddStateType((int)PlayChannel.PlayMode.Normal, Color.Black, Color.AliceBlue);
            cgChannels.AddStateType((int)PlayChannel.PlayMode.Solo, Color.Black, Color.LightGreen);
            cgChannels.AddStateType((int)PlayChannel.PlayMode.Mute, Color.Black, Color.Salmon);

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

        #region File functions
        /// <inheritdoc />
        public bool OpenFile(string fn)
        {
            bool ok = true;

            using (new WaitCursor())
            {
                // Clean up first.
                cgChannels.Clear();
                Rewind();

                // Process the file.
                _mfile = new MidiFile { IgnoreNoisy = true };
                _mfile.ProcessFile(fn);

                // Do things with things.
                _mfile.Channels.ForEach(ch => _playChannels[ch.Key].Patch = ch.Value);
                _tempo = _mfile.Tempo;

                lbPatterns.Items.Clear();
                foreach(var p in _mfile.AllPatterns)
                {
                    switch(p)
                    {
                        case "SFF1": // patches in here
                        case "SFF2":
                        case "SInt":
                            break;

                        default:
                            lbPatterns.Items.Add(p);
                            break;
                    }
                }

                if (lbPatterns.Items.Count > 0)
                {
                    lbPatterns.SelectedIndex = 0;
                }
                else
                {
                    GetPatternEvents(null);
                }

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
            if(!_running)
            {
                // Downshift to time increments compatible with this system.
                MidiTime mt = new MidiTime()
                {
                    InternalPpq = PPQ,
                    MidiPpq = _mfile.DeltaTicksPerQuarterNote,
                    Tempo = _tempo
                };
                
                //msecPerSubdiv = 60.0 / _tempo;
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
            _running = false;

            _mmTimer.Stop();

            // Send midi stop all notes just in case.
            for (int i = 0; i < _playChannels.Count(); i++)
            {
                if (_playChannels[i] != null && _playChannels[i].Valid)
                {
                    Kill(i);
                }
            }
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

            barBar.BeatsPerBar = BEATS_PER_BAR;
            barBar.SubdivsPerBeat = PPQ;
            barBar.Snap = Common.Settings.Snap;

            return ok;
        }

        /// <inheritdoc />
        public List<string> Dump()
        {
            _mfile.DrumChannel = _drumChannel;
            //return _mfile.GetReadableGrouped();
            return _mfile.GetReadableContents();
        }

        /// <inheritdoc />
        public void Export()
        {
            string newfn;
            string pattern;
            string info;

            if (_mfile.Filename.EndsWith(".sty"))
            {
                pattern = lbPatterns.SelectedItem.ToString().Replace(' ', '_');
                newfn = _mfile.Filename.Replace(".sty", $"_{pattern}.mid");
                info = $"Export {pattern} from {_mfile.Filename}";
            }
            else // .mid
            {
                pattern = "";
                newfn = _mfile.Filename.Replace(".mid", $"_export.mid");
                info = $"Export from {_mfile.Filename}";
            }

            using (SaveFileDialog dumpDlg = new SaveFileDialog() { Title = "Export midi", FileName = newfn })
            {
                if (dumpDlg.ShowDialog() == DialogResult.OK)
                {
                    _mfile.ExportMidi(newfn, pattern, info);
                }
            }
        }
        #endregion

        #region Midi send callback
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
                            if (ch.Events.ContainsKey(barBar.Current.TotalSubdivs))
                            {
                                foreach (var mevt in ch.Events[barBar.Current.TotalSubdivs])
                                {
                                    switch (mevt)
                                    {
                                        case NoteOnEvent evt:
                                            if (ch.ChannelNumber == _drumChannel && evt.Velocity == 0)
                                            {
                                                // Skip drum noteoffs as windows GM doesn't like them.
                                            }
                                            else
                                            {
                                                // Adjust volume and maybe drum channel. Also NAudio NoteLength bug.
                                                NoteOnEvent ne = new NoteOnEvent(
                                                    evt.AbsoluteTime,
                                                    ch.ChannelNumber == _drumChannel ? MidiDefs.DEFAULT_DRUM_CHANNEL : evt.Channel,
                                                    evt.NoteNumber,
                                                    (int)(evt.Velocity * Volume),
                                                    evt.OffEvent == null ? 0 : evt.NoteLength);

                                                MidiSend(ne);
                                            }
                                            break;

                                        case NoteEvent evt:
                                            if(ch.ChannelNumber == _drumChannel)
                                            {
                                                // Skip drum noteoffs as windows GM doesn't like them.
                                            }
                                            else
                                            {
                                                MidiSend(evt);
                                            }
                                            break;

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

            if (chkLogMidi.Checked)
            {
                LogMessage("MIDI_SEND", evt.ToString());
            }
        }
        #endregion

        #region Private Functions
        /// <summary>
        /// Get requested events.
        /// </summary>
        /// <param name="pattern">Specific pattern.</param>
        void GetPatternEvents(string pattern)
        {
            // Init internal structure.
            _playChannels.ForEach(pc => pc.Reset());

            // Downshift to time increments compatible with this system.
            MidiTime mt = new MidiTime()
            {
                InternalPpq = PPQ,
                MidiPpq = _mfile.DeltaTicksPerQuarterNote,
                Tempo = _tempo
            };

            // Bin events by channel. Scale to internal ppq.
            foreach (var ch in _mfile.Channels)
            {
                _playChannels[ch.Key].Patch = ch.Value;

                foreach (var te in _mfile.GetEvents(pattern, ch.Key))
                {
                    if (te.Channel - 1 < MidiDefs.NUM_CHANNELS) // midi is one-based
                    {
                        // Scale to internal.
                        long subdiv = mt.MidiToInternal(te.AbsoluteTime);

                        // Add to our collection.
                        _playChannels[te.Channel - 1].AddEvent((int)subdiv, te);
                    }
                };
            }

            // Figure out times.
            int lastSubdiv = _playChannels.Max(pc => pc.MaxSubdiv);
            // Round up to bar.
            int floor = lastSubdiv / (PPQ * 4); // 4/4 only.
            lastSubdiv = (floor + 1) * (PPQ * 4);
            sldTempo.Value = _tempo;

            barBar.Length = new BarSpan(lastSubdiv);
            barBar.Start = BarSpan.Zero;
            barBar.End = barBar.Length - BarSpan.OneSubdiv;
            barBar.Current = BarSpan.Zero;
        }

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
        /// Populate the click grid.
        /// </summary>
        void InitChannelsGrid()
        {
            cgChannels.Clear();

            for (int i = 0; i < _playChannels.Count(); i++)
            {
                var pc = _playChannels[i];

                // Make a name for UI.
                pc.Name = $"Ch:({i + 1}) ";

                if (i + 1 == _drumChannel)
                {
                    pc.Name += $"Drums";
                }
                else if (pc.Patch == -1)
                {
                    pc.Name += $"NoPatch";
                }
                else
                {
                    pc.Name += MidiDefs.GetInstrumentDef(pc.Patch);
                }

                // Maybe add to UI.
                if (pc.Valid && pc.HasNotes)
                {
                    cgChannels.AddIndicator(pc.Name, i);
                }
            }

            cgChannels.Show(2, cgChannels.Width / 2, 20);
        }
        #endregion

        #region UI event handlers
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Channels_IndicatorEvent(object sender, IndicatorEventArgs e)
        {
            int channel = e.Id;
            PlayChannel pch = _playChannels[channel];
            //LogMessage($"Click:{channel}");

            switch (pch.Mode)
            {
                case PlayChannel.PlayMode.Normal:
                    pch.Mode = PlayChannel.PlayMode.Solo;
                    // Mute any other non-solo channels.
                    for (int i = 0; i < MidiDefs.NUM_CHANNELS; i++)
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

            cgChannels.SetIndicator(channel, (int)pch.Mode);
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
        void BarBar_CurrentTimeChanged(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Sometimes drums are on channel 1, usually if it's the only channel in a clip file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DrumsOn1_CheckedChanged(object sender, EventArgs e)
        {
            _drumChannel = chkDrumsOn1.Checked ? 1 : MidiDefs.DEFAULT_DRUM_CHANNEL;

            // Update UI.
            InitChannelsGrid();
        }

        /// <summary>
        /// Validate selections and send patch now.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Patch_Click(object sender, EventArgs e)
        {
            bool valid = int.TryParse(txtPatchChannel.Text, out int pch);
            if (valid && pch >= 1 && pch <= MidiDefs.NUM_CHANNELS)
            {
                PatchChangeEvent evt = new PatchChangeEvent(0, pch, cmbPatchList.SelectedIndex);
                MidiSend(evt);

                // Update UI.
                _playChannels[pch - 1].Patch = cmbPatchList.SelectedIndex;
                InitChannelsGrid();
            }
            else
            {
                //txtPatchChannel.Text = "";
                LogMessage("ERROR", "Invalid patch channel");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Patterns_SelectedIndexChanged(object sender, EventArgs e)
        {
            GetPatternEvents(lbPatterns.SelectedItem.ToString());
            InitChannelsGrid();

            // Might need to update the patches.
            foreach(var ch in _mfile.Channels)
            {
                if(ch.Value != -1)
                {
                    PatchChangeEvent evt = new PatchChangeEvent(0, ch.Key, ch.Value);
                    MidiSend(evt);
                }
            }

            Rewind();
            Play();
        }
    }
    #endregion

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
        /// Clean up before reading another pattern.
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
