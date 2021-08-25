using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using NAudio.Midi;
using NBagOfTricks;
using System.Windows.Forms;


namespace ClipExplorer
{
    /// <summary>
    /// Reads in and processes standard midi or yahama style files. Timestamps are from original file.
    /// TODO Doesn't support multiple tracks. Would it be useful?
    /// </summary>
    public class MidiFile
    {
        #region Properties gleaned from the file
        /// <summary>What is it.</summary>
        public string Filename { get; private set; } = "";

        /// <summary>What is it.</summary>
        public int MidiFileType { get; private set; } = 0;

        /// <summary>How many tracks.</summary>
        public int Tracks { get; private set; } = 0;

        /// <summary>Resolution for all events.</summary>
        public int DeltaTicksPerQuarterNote { get; private set; } = 0;

        /// <summary>Tempo, if supplied by file. Defaults to 100 if missing.</summary>
        public double Tempo { get; set; } = 100.0;

        /// <summary>Time signature, if supplied by file.</summary>
        public string TimeSig { get; private set; } = "";

        /// <summary>Key signature, if supplied by file.</summary>
        public string KeySig { get; private set; } = "";

        /// <summary>Channel info: key is number, value is patch.</summary>
        public Dictionary<int, int> Channels { get; private set; } = new Dictionary<int, int>();

        /// <summary>All patterns in the file.</summary>
        public List<string> AllPatterns { get; private set; } = new List<string>();
        #endregion

        #region Properties set by client
        /// <summary>Sometimes drums are not on the default channel.</summary>
        public int DrumChannel { get; set; } = MidiDefs.DEFAULT_DRUM_CHANNEL;

        /// <summary>Don't include some events.</summary>
        public bool IgnoreNoisy { get; set; } = true;
        #endregion

        #region Fields
        /// <summary>All the midi events by pattern/channel groups. This is the verbatim content of the file with no processing.</summary>
        readonly List<(string pattern, int channel, MidiEvent evt)> _midiEvents = new List<(string pattern, int channel, MidiEvent evt)>();

        /// <summary>Current pattern.</summary>
        string _currentPattern = "";

        /// <summary>File contents in readable form as they appear in order. Useful for debug.</summary>
        readonly List<string> _allContents = new List<string>();

        /// <summary>Save this for maybe logging.</summary>
        long _lastPos = 0;
        #endregion

        #region Public methods
        /// <summary>
        /// Read a file.
        /// </summary>
        /// <param name="fn"></param>
        public void ProcessFile(string fn)
        {
            // Init everything.
            _midiEvents.Clear();
            Filename = fn;
            AllPatterns.Clear();
            //Patches.ForEach(p => p = -1);
            Channels.Clear();
            DeltaTicksPerQuarterNote = 0;
            Tempo = 100;
            TimeSig = "";
            KeySig = "";

            _allContents.Clear();
            _allContents.Add($"Timestamp,Type,Pattern,Channel,FilePos,Content");

            using (var br = new BinaryReader(File.OpenRead(fn)))
            {
                bool done = false;

                while (!done)
                {
                    var sectionName = Encoding.UTF8.GetString(br.ReadBytes(4));

                    Capture(-1, "Section", -1, sectionName);

                    switch (sectionName)
                    {
                        case "MThd":
                            ReadMThd(br);
                            break;

                        case "MTrk":
                            ReadMTrk(br);
                            break;

                        case "CASM":
                            ReadCASM(br);
                            break;

                        case "CSEG":
                            ReadCSEG(br);
                            break;

                        case "Sdec":
                            ReadSdec(br);
                            break;

                        case "Ctab":
                            ReadCtab(br);
                            break;

                        case "Cntt":
                            ReadCntt(br);
                            break;

                        case "OTSc": // One Touch Setting section
                            ReadOTSc(br);
                            break;

                        case "FNRc": // MDB (Music Finder) section
                            ReadFNRc(br);
                            break;

                        default:
                            Capture(-1, "Done", -1, "!!!");
                            done = true;
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Helper to get an event collection.
        /// </summary>
        /// <param name="channel">Specific channel or empty/null for all.</param>
        /// <returns>The collection or null if invalid.</returns>
        public IEnumerable<MidiEvent> GetEvents(string pattern, int channel)
        {
            IEnumerable<MidiEvent> ret = string.IsNullOrEmpty(pattern) ?
                _midiEvents.Where(v => v.channel == channel).Select(v => v.evt) :
                _midiEvents.Where(v => v.pattern == pattern && v.channel == channel).Select(v => v.evt);
            return ret;
        }
        #endregion

        #region Section readers
        /// <summary>
        /// Read the midi header section of a style file.
        /// </summary>
        /// <param name="br"></param>
        void ReadMThd(BinaryReader br)
        {
            uint chunkSize = Read(br, 4);

            if (chunkSize != 6)
            {
                throw new FormatException("Unexpected header chunk length");
            }

            MidiFileType = (int)Read(br, 2);

            //// Style midi section is always type 0.
            //if (MidiFileType != 0)
            //{
            //    throw new FormatException($"This is type {MidiFileType} - must be 0");
            //}

            // Midi file type.
            Tracks = (int)Read(br, 2);
            if (Tracks != 1)
            {
                //throw new FormatException($"This has {Tracks} tracks - must be 1");
            }

            DeltaTicksPerQuarterNote = (int)Read(br, 2);
        }

        /// <summary>
        /// Read a midi track chunk.
        /// </summary>
        /// <param name="br"></param>
        /// <returns></returns>
        int ReadMTrk(BinaryReader br)
        {
            // Defaults.
            int chnum = 0;
            //string chdesc = "???";

            uint chunkSize = Read(br, 4);
            long startPos = br.BaseStream.Position;
            int absoluteTime = 0;

            // Read all midi events.
            MidiEvent me = null; // current
            while (br.BaseStream.Position < startPos + chunkSize)
            {
                _lastPos = br.BaseStream.Position;

                me = MidiEvent.ReadNextEvent(br, me);
                absoluteTime += me.DeltaTime;
                me.AbsoluteTime = absoluteTime;
                if(!Channels.ContainsKey(me.Channel))
                {
                    Channels.Add(me.Channel, -1);
                }

                switch (me.CommandCode)
                {
                    ///// Standard midi events /////
                    case MidiCommandCode.NoteOn:
                        {
                            NoteOnEvent evt = me as NoteOnEvent;
                            AddMidiEvent(evt);
                            Capture(evt.AbsoluteTime, "NoteOn", evt.Channel, evt.ToString());
                        }
                        break;

                    case MidiCommandCode.NoteOff:
                        {
                            NoteEvent evt = me as NoteEvent;
                            AddMidiEvent(evt);
                            Capture(evt.AbsoluteTime, "NoteOff", evt.Channel, evt.ToString());
                        }
                        break;

                    case MidiCommandCode.ControlChange:
                        {
                            if (!IgnoreNoisy)
                            {
                                ControlChangeEvent evt = me as ControlChangeEvent;
                                AddMidiEvent(evt);
                                Capture(evt.AbsoluteTime, "ControlChange", evt.Channel, evt.ToString());
                            }
                        }
                        break;

                    case MidiCommandCode.PitchWheelChange:
                        {
                            if (!IgnoreNoisy)
                            {
                                PitchWheelChangeEvent evt = me as PitchWheelChangeEvent;
                                //AddMidiEvent(evt);
                            }
                        }
                        break;

                    case MidiCommandCode.PatchChange:
                        {
                            PatchChangeEvent evt = me as PatchChangeEvent;
                            //chdesc = PatchChangeEvent.GetPatchName(evt.Patch);
                            Channels[evt.Channel] = evt.Patch;
                            AddMidiEvent(evt);
                            Capture(evt.AbsoluteTime, "PatchChangeEvent", evt.Channel, evt.ToString());
                        }
                        break;

                    case MidiCommandCode.Sysex:
                        {
                            if(!IgnoreNoisy)
                            {
                                SysexEvent evt = me as SysexEvent;
                                string s = evt.ToString().Replace(Environment.NewLine, " ");
                                Capture(evt.AbsoluteTime, "Sysex", evt.Channel, s);
                            }
                        }
                        break;

                    ///// Meta events /////
                    case MidiCommandCode.MetaEvent when (me as MetaEvent).MetaEventType == MetaEventType.TrackSequenceNumber:
                        {
                            TrackSequenceNumberEvent evt = me as TrackSequenceNumberEvent;
                            chnum = evt.Channel;
                            Capture(evt.AbsoluteTime, "TrackSequenceNumber", evt.Channel, evt.ToString());
                        }
                        break;

                    case MidiCommandCode.MetaEvent when (me as MetaEvent).MetaEventType == MetaEventType.SequenceTrackName:
                        {
                            TextEvent evt = me as TextEvent;
                            Capture(evt.AbsoluteTime, "SequenceTrackName", evt.Channel, evt.Text);
                        }
                        break;

                    case MidiCommandCode.MetaEvent when (me as MetaEvent).MetaEventType == MetaEventType.Marker:
                        {
                            // Indicates start of a new midi pattern.
                            TextEvent evt = me as TextEvent;
                            Capture(evt.AbsoluteTime, "Marker", evt.Channel, evt.Text);
                            _currentPattern = evt.Text;
                            AllPatterns.Add(_currentPattern);
                            absoluteTime = 0;
                        }
                        break;

                    case MidiCommandCode.MetaEvent when (me as MetaEvent).MetaEventType == MetaEventType.EndTrack:
                        {
                            // Indicates end of current midi track.
                            MetaEvent evt = me as MetaEvent;
                            Capture(evt.AbsoluteTime, "EndTrack", evt.Channel, evt.ToString());
                            _currentPattern = "";
                        }
                        break;

                    case MidiCommandCode.MetaEvent when (me as MetaEvent).MetaEventType == MetaEventType.SetTempo:
                        {
                            TempoEvent evt = me as TempoEvent;
                            Tempo = evt.Tempo;
                            Capture(evt.AbsoluteTime, "SetTempo", evt.Channel, evt.Tempo.ToString());
                        }
                        break;

                    case MidiCommandCode.MetaEvent when (me as MetaEvent).MetaEventType == MetaEventType.TimeSignature:
                        {
                            TimeSignatureEvent evt = me as TimeSignatureEvent;
                            TimeSig = evt.TimeSignature;
                            Capture(evt.AbsoluteTime, "TimeSignature", evt.Channel, evt.TimeSignature);
                        }
                        break;

                    case MidiCommandCode.MetaEvent when (me as MetaEvent).MetaEventType == MetaEventType.KeySignature:
                        {
                            KeySignatureEvent evt = me as KeySignatureEvent;
                            KeySig = evt.ToString();
                            Capture(evt.AbsoluteTime, "KeySignature", evt.Channel, evt.ToString());
                        }
                        break;

                    case MidiCommandCode.MetaEvent when (me as MetaEvent).MetaEventType == MetaEventType.TextEvent:
                        {
                            TextEvent evt = me as TextEvent;
                            Capture(evt.AbsoluteTime, "TextEvent", evt.Channel, evt.Text);
                        }
                        break;

                    default:
                        // Other MidiCommandCodes: AutoSensing, ChannelAfterTouch, ContinueSequence, Eox, KeyAfterTouch, StartSequence, StopSequence, TimingClock
                        // Other MetaEventType: Copyright, CuePoint, DeviceName, Lyric, MidiChannel, MidiPort, ProgramName, SequencerSpecific, SmpteOffset, TrackInstrumentName
                        Capture(-1, "Other", -1, $"{me.GetType()} {me}");
                        break;
                }
            }

            ///// Local function. /////
            void AddMidiEvent(MidiEvent evt)
            {
                _midiEvents.Add((_currentPattern, evt.Channel, evt));
            }

            return absoluteTime;
        }

        /// <summary>
        /// Read the CASM section of a style file.
        /// </summary>
        /// <param name="br"></param>
        void ReadCASM(BinaryReader br)
        {
            uint chunkSize = Read(br, 4);
        }

        /// <summary>
        /// Read the CSEG section of a style file.
        /// </summary>
        /// <param name="br"></param>
        void ReadCSEG(BinaryReader br)
        {
            uint chunkSize = Read(br, 4);
        }

        /// <summary>
        /// Read the Sdec section of a style file.
        /// </summary>
        /// <param name="br"></param>
        void ReadSdec(BinaryReader br)
        {
            uint chunkSize = Read(br, 4);
            br.ReadBytes((int)chunkSize);
        }

        /// <summary>
        /// Read the Ctab section of a style file.
        /// </summary>
        /// <param name="br"></param>
        void ReadCtab(BinaryReader br)
        {
            // Has some key and chord info.
            uint chunkSize = Read(br, 4);
            br.ReadBytes((int)chunkSize);
        }

        /// <summary>
        /// Read the Cntt section of a style file.
        /// </summary>
        /// <param name="br"></param>
        void ReadCntt(BinaryReader br)
        {
            uint chunkSize = Read(br, 4);
            br.ReadBytes((int)chunkSize);
        }

        /// <summary>
        /// Read the OTSc section of a style file.
        /// </summary>
        /// <param name="br"></param>
        void ReadOTSc(BinaryReader br)
        {
            uint chunkSize = Read(br, 4);
            br.ReadBytes((int)chunkSize);
        }

        /// <summary>
        /// Read the FNRc section of a style file.
        /// </summary>
        /// <param name="br"></param>
        void ReadFNRc(BinaryReader br)
        {
            uint chunkSize = Read(br, 4);
            br.ReadBytes((int)chunkSize);
        }
        #endregion

        #region Output formatters
        /// <summary>
        /// Dump the contents in a csv readable form.
        /// This is as the events appear in the original file plus some other stuff for debugging.
        /// </summary>
        /// <returns></returns>
        public List<string> GetReadableContents()
        {
            return _allContents;
        }

        /// <summary>
        /// Makes csv dumps of some events grouped by pattern/channel.
        /// </summary>
        /// <returns></returns>
        public List<string> GetReadableGrouped()
        {
            List<string> meta = new List<string>
            {
                $"---Meta---",
                $"Meta,Value",
                $"MidiFileType,{MidiFileType}",
                $"DeltaTicksPerQuarterNote,{DeltaTicksPerQuarterNote}",
                //$"StartAbsoluteTime,{StartAbsoluteTime}",
                $"Tracks,{Tracks}"
            };

            List<string> notes = new List<string>()
            {
                $"",
                $"---Notes---",
                "Time,Event,Channel,Pattern,NoteNum,NoteName,Velocity,Duration",
            };

            List<string> other = new List<string>()
            {
                $"",
                $"---Other---",
                "Time,Event,Channel,Pattern,Val1,Val2,Val3",
            };

            foreach (var me in _midiEvents)
            {
                // Boilerplate.
                string ntype = me.evt.GetType().ToString().Replace("NAudio.Midi.", "");
                string sc = $"{me.evt.AbsoluteTime},{ntype},{me.evt.Channel},{me.pattern}";

                switch (me.evt)
                {
                    case NoteOnEvent evt:
                        int len = evt.OffEvent == null ? 0 : evt.NoteLength; // NAudio NoteLength bug.
                        string nname = evt.Channel == DrumChannel ? $"{MidiDefs.GetDrumDef(evt.NoteNumber)}" : $"{MidiDefs.NoteNumberToName(evt.NoteNumber)}";
                        notes.Add($"{sc},{evt.NoteNumber},{nname},{evt.Velocity},{len}");
                        break;

                    case NoteEvent evt: // used for NoteOff
                        notes.Add($"{sc},{evt.NoteNumber},,{evt.Velocity},");
                        break;

                    case TempoEvent evt:
                        meta.Add($"Tempo,{evt.Tempo}");
                        other.Add($"{sc},{evt.Tempo},{evt.MicrosecondsPerQuarterNote}");
                        break;

                    case TimeSignatureEvent evt:
                        other.Add($"{sc},{evt.TimeSignature},,");
                        break;

                    case KeySignatureEvent evt:
                        other.Add($"{sc},{evt.SharpsFlats},{evt.MajorMinor},");
                        break;

                    case PatchChangeEvent evt:
                        string pname = evt.Channel == DrumChannel ? $"" : $"{MidiDefs.GetInstrumentDef(evt.Patch)}"; // drum kit?
                        other.Add($"{sc},{evt.Patch},{pname},");
                        break;

                    case ControlChangeEvent evt:
                        other.Add($"{sc},{(int)evt.Controller},{MidiDefs.GetControllerDef((int)evt.Controller)},{evt.ControllerValue}");
                        break;

                    case PitchWheelChangeEvent evt:
                        other.Add($"{sc},{evt.Pitch},,");
                        break;

                    case TextEvent evt:
                        other.Add($"{sc},{evt.Text},,,");
                        break;

                    //case ChannelAfterTouchEvent:
                    //case SysexEvent:
                    //case MetaEvent:
                    //case RawMetaEvent:
                    //case SequencerSpecificEvent:
                    //case SmpteOffsetEvent:
                    //case TrackSequenceNumberEvent:
                    default:
                        break;
                }
            }

            List<string> ret = new List<string>();
            ret.AddRange(meta);
            ret.AddRange(notes);
            ret.AddRange(other);

            return ret;
        }

        /// <summary>
        /// Convert neb steps to midi file.
        /// </summary>
        /// <param name="fn">Where to put the midi.</param>
        /// <param name="pattern">Specific pattern if a style file.</param>
        /// <param name="info">Extra info to add to midi file.</param>
        public void ExportMidi(string fn, string pattern, string info)
        {
            int exportPpq = 96; // arbitrary

            MidiEventCollection mecoll = new MidiEventCollection(1, exportPpq);

            IList<MidiEvent> mevents = mecoll.AddTrack();

            // Tempo.
            mevents.Add(new TempoEvent(0, 0) { Tempo = Tempo });

            // General info.
            mevents.Add(new TextEvent(info, MetaEventType.TextEvent, 0));

            // Optional.
            //lhdr.Add(new TimeSignatureEvent(0, 4, 2, (int)ticksPerClick, 8));
            //lhdr.Add(new KeySignatureEvent(0, 0, 0));

            // Patches.
            Channels.Where(c => c.Value != -1).ForEach(c => mevents.Add(new PatchChangeEvent(0, c.Key, c.Value)));

            // Collect the midi events for this pattern ordered by timestamp.
            IEnumerable<MidiEvent> evts = string.IsNullOrEmpty(pattern) ?
                _midiEvents.Select(v => v.evt).OrderBy(v => v.AbsoluteTime) :
                _midiEvents.Where(v => v.pattern == pattern).Select(v => v.evt).OrderBy(v => v.AbsoluteTime);
            long ltime = evts.Last().AbsoluteTime;

            // Copy to output.
            evts.ForEach(e => mevents.Add(e));

            // End track.
            var endt = new MetaEvent(MetaEventType.EndTrack, 0, ltime);
            mevents.Add(endt);

            NAudio.Midi.MidiFile.Export(fn, mecoll);
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Save an event.
        /// </summary>
        /// <param name="timestamp"></param>
        /// <param name="etype"></param>
        /// <param name="channel"></param>
        /// <param name="content"></param>
        void Capture(long timestamp, string etype, int channel, string content)
        {
            _allContents.Add($"{timestamp},{etype},{_currentPattern},{channel},{_lastPos},{content.Replace(',', '_')}");
        }

        /// <summary>
        /// Read a number and adjust endianess.
        /// </summary>
        /// <param name="br"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        uint Read(BinaryReader br, int num)
        {
            uint i;

            _lastPos = br.BaseStream.Position;

            switch (num)
            {
                case 2:
                    i = br.ReadUInt16();
                    if (BitConverter.IsLittleEndian)
                    {
                        i = (UInt16)(((i & 0xFF00) >> 8) | ((i & 0x00FF) << 8));
                    }
                    break;

                case 4:
                    i = br.ReadUInt32();
                    if (BitConverter.IsLittleEndian)
                    {
                        i = ((i & 0xFF000000) >> 24) | ((i & 0x00FF0000) >> 8) | ((i & 0x0000FF00) << 8) | ((i & 0x000000FF) << 24);
                    }
                    break;

                default:
                    throw new FormatException("Unsupported read size");
            }

            return i;
        }
        #endregion
    }
}
