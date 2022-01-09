using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using NAudio.Midi;
using NBagOfTricks;


namespace ClipExplorer
{
    /// <summary>
    /// Reads in and processes standard midi files. Timestamps are from original file.
    /// Note: NAudio midi event channel numbers are 1-based.
    /// FUTURE Doesn't support multiple tracks. Would it be useful?
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

        /// <summary>Channel/patch info: key is 1-based channel number, value is 0-based patch.</summary>
        public Dictionary<int, int> Channels { get; private set; } = new();
        #endregion

        #region Properties set by client
        /// <summary>Sometimes drums are not on the default channel.</summary>
        public int DrumChannel { get; set; } = MidiDefs.DEFAULT_DRUM_CHANNEL;

        /// <summary>Don't include some events.</summary>
        public bool IgnoreNoisy { get; set; } = true;
        #endregion

        #region Fields
        /// <summary>All the midi events by channel. This is the verbatim content of the file with no processing.</summary>
        readonly List<(int channel, MidiEvent evt)> _midiEvents = new();

        /// <summary>File contents in readable form as they appear in order. Useful for debug.</summary>
        readonly List<string> _allContents = new();

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
            Channels.Clear();
            DeltaTicksPerQuarterNote = 0;
            Tempo = 100;
            TimeSig = "";
            KeySig = "";

            _allContents.Clear();
            _allContents.Add($"Timestamp,Type,Channel,FilePos,Content");

            using var br = new BinaryReader(File.OpenRead(fn));
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

                    default:
                        Capture(-1, "Done", -1, "!!!");
                        done = true;
                        break;
                }
            }
        }

        /// <summary>
        /// Helper to get an event collection.
        /// </summary>
        /// <param name="channel">Specific channel.</param>
        /// <returns>The collection or null if invalid.</returns>
        public IEnumerable<MidiEvent> GetEvents(int channel)
        {
            IEnumerable<MidiEvent> ret = _midiEvents.Where(v => v.channel == channel).Select(v => v.evt);
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

            // Style midi section is always type 0.
            //if (MidiFileType != 0)
            //{
            //    throw new FormatException($"This is type {MidiFileType} - must be 0");
            //}

            // Midi file type.
            Tracks = (int)Read(br, 2);
            // if (Tracks != 1)
            // {
            //     throw new FormatException($"This has {Tracks} tracks - must be 1");
            // }

            DeltaTicksPerQuarterNote = (int)Read(br, 2);
        }

        /// <summary>
        /// Read a midi track chunk.
        /// </summary>
        /// <param name="br"></param>
        /// <returns></returns>
        int ReadMTrk(BinaryReader br)
        {
            uint chunkSize = Read(br, 4);
            long startPos = br.BaseStream.Position;
            int absoluteTime = 0;

            // Read all midi events.
            MidiEvent? me = null; // current
            while (br.BaseStream.Position < startPos + chunkSize)
            {
                _lastPos = br.BaseStream.Position;

                me = MidiEvent.ReadNextEvent(br, me);
                absoluteTime += me.DeltaTime;
                me.AbsoluteTime = absoluteTime;

                if (!Channels.ContainsKey(me.Channel))
                {
                    Channels.Add(me.Channel, -1);
                }

                switch (me)
                {
                    ///// Standard midi events /////
                    case NoteOnEvent evt:
                        AddMidiEvent(evt);
                        Capture(evt.AbsoluteTime, "NoteOn", evt.Channel, evt.ToString());
                        break;

                    case NoteEvent evt:
                        AddMidiEvent(evt);
                        Capture(evt.AbsoluteTime, "NoteOff", evt.Channel, evt.ToString());
                        break;

                    case ControlChangeEvent evt:
                        if (!IgnoreNoisy)
                        {
                            AddMidiEvent(evt);
                            Capture(evt.AbsoluteTime, "ControlChange", evt.Channel, evt.ToString());
                        }
                        break;

                    case PitchWheelChangeEvent evt:
                        if (!IgnoreNoisy)
                        {
                            
                            //AddMidiEvent(evt);
                        }
                        break;

                    case PatchChangeEvent evt:
                        Channels[evt.Channel] = evt.Patch;
                        AddMidiEvent(evt);
                        Capture(evt.AbsoluteTime, "PatchChangeEvent", evt.Channel, evt.ToString());
                        break;

                    case TempoEvent evt:
                        Tempo = (int)Math.Round(evt.Tempo);
                        Capture(evt.AbsoluteTime, "SetTempo", evt.Channel, evt.Tempo.ToString());
                        AddMidiEvent(evt);
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
                _midiEvents.Add((evt.Channel, evt));
            }

            return absoluteTime;
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
        /// Makes csv dumps of some events grouped by channel.
        /// </summary>
        /// <returns></returns>
        public List<string> GetReadableGrouped()
        {
            List<string> meta = new()
            {
                $"---Meta---",
                $"Meta,Value",
                $"MidiFileType,{MidiFileType}",
                $"DeltaTicksPerQuarterNote,{DeltaTicksPerQuarterNote}",
                //$"StartAbsoluteTime,{StartAbsoluteTime}",
                $"Tracks,{Tracks}"
            };

            List<string> notes = new()
            {
                $"",
                $"---Notes---",
                "Time,Event,Channel,Pattern,NoteNum,NoteName,Velocity,Duration",
            };

            List<string> other = new()
            {
                $"",
                $"---Other---",
                "Time,Event,Channel,Pattern,Val1,Val2,Val3",
            };

            foreach (var me in _midiEvents)
            {
                // Boilerplate.
                string ntype = me.evt.GetType().ToString().Replace("NAudio.Midi.", "");
                string sc = $"{me.evt.AbsoluteTime},{ntype},{me.evt.Channel}";

                switch (me.evt)
                {
                    case NoteOnEvent evt:
                        int len = evt.OffEvent is null ? 0 : evt.NoteLength; // NAudio NoteLength bug.
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

            List<string> ret = new();
            ret.AddRange(meta);
            ret.AddRange(notes);
            ret.AddRange(other);

            return ret;
        }

        /// <summary>
        /// Output part or all of the file to a new midi file.
        /// </summary>
        /// <param name="fn">Where to put the midi.</param>
        /// <param name="info">Extra info to add to midi file.</param>
        public void ExportMidi(string fn, string info)
        {
            MidiEventCollection mecoll = new(1, DeltaTicksPerQuarterNote);
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

            // Collect the midi events ordered by timestamp.
            IEnumerable<MidiEvent> evts = _midiEvents.Select(v => v.evt).OrderBy(v => v.AbsoluteTime);
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
            _allContents.Add($"{timestamp},{etype},{channel},{_lastPos},{content.Replace(',', '_')}");
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
