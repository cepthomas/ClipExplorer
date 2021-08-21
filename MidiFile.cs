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
    /// <summary>Reads in and processes standard midi or yahama style files.</summary>
    public class MidiFile
    {
        #region Properties gleaned from the file
        /// <summary>Channel info: key is number, value is name.</summary>
        public Dictionary<int, string> Channels { get; private set; } = new Dictionary<int, string>();

        /// <summary>Resolution for all events.</summary>
        public int DeltaTicksPerQuarterNote { get; private set; } = 0;

        /// <summary>Tempo, if supplied by file. Defaults to 100 if missing.</summary>
        public double Tempo { get; private set; } = 100.0;

        /// <summary>Time signature, if supplied by file.</summary>
        public string TimeSig { get; private set; } = "";

        /// <summary>Key signature, if supplied by file.</summary>
        public string KeySig { get; private set; } = "";

        /// <summary>File contents in order.</summary>
        public List<string> AllFileContents { get; private set; } = new List<string>();

        /// <summary>All patterns in the file.</summary>
        public List<string> AllPatterns { get; private set; } = new List<string>();
        #endregion

        #region Fields
        /// <summary>All the midi events by pattern/channel groups. This is the verbatim content of the file with no processing.</summary>
        List<(string pattern, int channel, MidiEvent evt)> _midiEvents = new List<(string pattern, int channel, MidiEvent evt)>();

        /// <summary>Current pattern.</summary>
        string _currentPattern = "";

        /// <summary>Save this for maybe logging.</summary>
        long _lastPos = 0;
        #endregion

        #region Public methods
        /// <summary>
        /// Read a file.
        /// </summary>
        /// <param name="fileName"></param>
        public void ProcessFile(string fileName)
        {
            // Init everything.
            _midiEvents.Clear();
            DeltaTicksPerQuarterNote = 0;
            Tempo = 100;
            TimeSig = "";
            KeySig = "";

            using (var br = new BinaryReader(File.OpenRead(fileName)))
            {
                bool done = false;

                while (!done)
                {
                    var sectionName = Encoding.UTF8.GetString(br.ReadBytes(4));

                    Loggo(-1, "Section", sectionName);

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

                        case "FNRc": // MDB(Music Finder) section
                            ReadFNRc(br);
                            break;

                        default:
                            Loggo(-1, "Done", "!!!");
                            done = true;
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Helper to get an event collection.
        /// </summary>
        /// <param name="channel"></param>
        /// <returns>The collection or null if invalid.</returns>
        public IEnumerable<MidiEvent> GetEvents(string pattern, int channel)
        {
            return _midiEvents.Where(v => v.pattern == pattern && v.channel == channel).Select(v => v.evt);
        }
        #endregion

        #region Section parsers
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

            uint fileFormat = Read(br, 2);

            // Style midi section is always type 0 - only one track.
            if (fileFormat != 0)
            {
                throw new FormatException($"This is type {fileFormat} - must be 0");
            }

            // Midi type 0 - only one track.
            uint tracks = Read(br, 2);
            if (tracks != 1)
            {
                throw new FormatException($"This has {tracks} tracks - must be 1");
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
            string chdesc = "???";

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

                switch (me.CommandCode)
                {
                    ///// Standard midi events /////
                    case MidiCommandCode.NoteOn:
                        {
                            NoteOnEvent evt = me as NoteOnEvent;
                            AddMidiEvent(evt);
                            Loggo(evt.AbsoluteTime, "NoteOn", evt.ToString());
                        }
                        break;

                    case MidiCommandCode.NoteOff:
                        {
                            NoteEvent evt = me as NoteEvent;
                            AddMidiEvent(evt);
                            Loggo(evt.AbsoluteTime, "NoteOff", evt.ToString());
                        }
                        break;

                    case MidiCommandCode.ControlChange:
                        {
                            ControlChangeEvent evt = me as ControlChangeEvent;
                            AddMidiEvent(evt);
                            Loggo(evt.AbsoluteTime, "ControlChange", evt.ToString());
                        }
                        break;

                    case MidiCommandCode.PitchWheelChange:
                        {
                            PitchWheelChangeEvent evt = me as PitchWheelChangeEvent;
                            //AddMidiEvent(evt);
                        }
                        break;

                    case MidiCommandCode.PatchChange:
                        {
                            PatchChangeEvent evt = me as PatchChangeEvent;
                            chdesc = PatchChangeEvent.GetPatchName(evt.Patch);
                            AddMidiEvent(evt);
                            Loggo(evt.AbsoluteTime, "PatchChangeEvent", evt.ToString());
                        }
                        break;

                    case MidiCommandCode.Sysex:
                        {
                            SysexEvent evt = me as SysexEvent;
                            string s = evt.ToString().Replace(Environment.NewLine, " ");
                            Loggo(evt.AbsoluteTime, "Sysex", s);
                        }
                        break;

                    ///// Meta events /////
                    case MidiCommandCode.MetaEvent when (me as MetaEvent).MetaEventType == MetaEventType.TrackSequenceNumber:
                        {
                            TrackSequenceNumberEvent evt = me as TrackSequenceNumberEvent;
                            chnum = evt.Channel;
                            Loggo(evt.AbsoluteTime, "TrackSequenceNumber", evt.ToString());
                        }
                        break;

                    case MidiCommandCode.MetaEvent when (me as MetaEvent).MetaEventType == MetaEventType.SequenceTrackName:
                        {
                            TextEvent evt = me as TextEvent;
                            Loggo(evt.AbsoluteTime, "SequenceTrackName", evt.Text);
                        }
                        break;

                    case MidiCommandCode.MetaEvent when (me as MetaEvent).MetaEventType == MetaEventType.Marker:
                        {
                            // Indicates start of a new midi pattern.
                            TextEvent evt = me as TextEvent;
                            Loggo(evt.AbsoluteTime, "Marker", evt.Text);
                            _currentPattern = evt.Text;
                            AllPatterns.Add(_currentPattern);
                            absoluteTime = 0;
                        }
                        break;

                    case MidiCommandCode.MetaEvent when (me as MetaEvent).MetaEventType == MetaEventType.EndTrack:
                        {
                            // Indicates end of current midi track.
                            MetaEvent evt = me as MetaEvent;
                            Loggo(evt.AbsoluteTime, "EndTrack", evt.ToString());
                        }
                        break;

                    case MidiCommandCode.MetaEvent when (me as MetaEvent).MetaEventType == MetaEventType.SetTempo:
                        {
                            TempoEvent evt = me as TempoEvent;
                            Tempo = evt.Tempo;
                            Loggo(evt.AbsoluteTime, "SetTempo", evt.Tempo.ToString());
                        }
                        break;

                    case MidiCommandCode.MetaEvent when (me as MetaEvent).MetaEventType == MetaEventType.TimeSignature:
                        {
                            TimeSignatureEvent evt = me as TimeSignatureEvent;
                            TimeSig = evt.TimeSignature;
                            Loggo(evt.AbsoluteTime, "TimeSignature", evt.TimeSignature);
                        }
                        break;

                    case MidiCommandCode.MetaEvent when (me as MetaEvent).MetaEventType == MetaEventType.KeySignature:
                        {
                            KeySignatureEvent evt = me as KeySignatureEvent;
                            KeySig = evt.ToString();
                            Loggo(evt.AbsoluteTime, "KeySignature", evt.ToString());
                        }
                        break;

                    case MidiCommandCode.MetaEvent when (me as MetaEvent).MetaEventType == MetaEventType.TextEvent:
                        {
                            TextEvent evt = me as TextEvent;
                            Loggo(evt.AbsoluteTime, "TextEvent", evt.Text);
                        }
                        break;

                    default:
                        //Other MidiCommandCodes:
                        //AutoSensing = 254,
                        //ChannelAfterTouch = 208,
                        //ContinueSequence = 251,
                        //Eox = 247,
                        //KeyAfterTouch = 160,
                        //StartSequence = 250,
                        //StopSequence = 252,
                        //TimingClock = 248,

                        // Other MetaEventType:
                        //Copyright = 2,
                        //CuePoint = 7,
                        //DeviceName = 9,
                        //Lyric = 5,
                        //MidiChannel = 32,
                        //MidiPort = 33,
                        //ProgramName = 8,
                        //SequencerSpecific = 127
                        //SmpteOffset = 84,
                        //TrackInstrumentName = 4,

                        Loggo(-1, "Other", $"{me.GetType()} {me}");
                        break;
                }

                //_currentPart
            }

            ///// Local function. /////
            void AddMidiEvent(MidiEvent evt)
            {
                //if (!_events.ContainsKey(evt.Channel))
                //{
                //    _events.Add(evt.Channel, new List<MidiEvent>());
                //}

                if (!Channels.ContainsKey(evt.Channel))
                {
                    Channels.Add(evt.Channel, evt.Channel == 10 ? "Drums" : chdesc);
                }

                //_events[evt.Channel].Add(evt);

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
            // The information in the CASM section is necessary if the midi section does not follow the rules
            // for “simple” style files, which do not necessarily need a CASM section (see chapter 5.2.1 for
            // the rules). The CASM section gives instructions to the instrument on how to deal with the midi data.
            // This includes:
            // - Assigning the sixteen possible midi channels to 8 accompaniment channels which are
            //   available to a style in the instrument (9 = sub rhythm, 10 = rhythm, 11 = bass, 12 = chord 1,
            //   13 = chord 2, 14 = pad, 15 = phrase 1, 16 = phrase 2). More than one midi channel
            //   may be assigned to an accompaniment channel.
            // - Allowing the PSR to edit the source channel in StyleCreator. This setting is overridden by
            //   the instrument if the style has > 1 midi source channel assigned to an accompaniment
            //   channel. In this case the source channels are not editable.
            // - Muting/enabling specific notes or chords to trigger the accompaniment. In practice, only
            //   chord choices are used.
            // - The key that is used in the midi channel. Styles often use different keys for the midi data.
            //   Styles without a CASM must be in the key of CMaj7.
            // - How the chords and notes are transposed as chords are changed and how notes held
            //   through chord changes are reproduced.
            // - The range of notes generated by the style.
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
            // swallow for now
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
            // swallow for now
            br.ReadBytes((int)chunkSize);
        }

        /// <summary>
        /// Read the Cntt section of a style file.
        /// </summary>
        /// <param name="br"></param>
        void ReadCntt(BinaryReader br)
        {
            uint chunkSize = Read(br, 4);
            // swallow for now
            br.ReadBytes((int)chunkSize);
        }

        /// <summary>
        /// Read the OTSc section of a style file.
        /// </summary>
        /// <param name="br"></param>
        void ReadOTSc(BinaryReader br)
        {
            uint chunkSize = Read(br, 4);
            // swallow for now
            br.ReadBytes((int)chunkSize);
        }

        /// <summary>
        /// Read the FNRc section of a style file.
        /// </summary>
        /// <param name="br"></param>
        void ReadFNRc(BinaryReader br)
        {
            uint chunkSize = Read(br, 4);
            // swallow for now
            br.ReadBytes((int)chunkSize);
        }
        #endregion

        #region Helpers
        /// <summary>
        /// 
        /// </summary>
        /// <param name="timestamp"></param>
        /// <param name="etype"></param>
        /// <param name="content"></param>
        void Loggo(long timestamp, string etype, string content)
        {
            AllFileContents.Add($"{timestamp}:::{etype}:::{_lastPos}:::{content}");
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
