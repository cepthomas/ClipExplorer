using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using NAudio.Midi;
using NBagOfTricks;
using System.Windows.Forms;



//Each of the other markers (Intro A, Main B, etc) defines musical patterns that are triggered by
//the keying chords. Intros play only once when triggered and then turn control over to the next
//section selected by the panel buttons. Main sections (A, B, C, and D) repeat until the style is
//stopped or an Ending or an Intro is selected. Ending sections play once and the style is
//stopped. Fill Ins are triggered manually, or play automatically (if Auto Fill is On) when a new
//main section is selected.

///// <summary>All the midi events by part/channel groups. This is the verbatim content of the file with no processing.</summary>
//Dictionary<int, List<MidiEvent>> _events = new Dictionary<int, List<MidiEvent>>();







namespace ClipExplorer
{
    //class MidiEvents
    //{
    //    public List<string> GetAllPatterns()
    //    {
    //    }
    //    public List<int> GetAllPatterns()
    //    {
    //    }
    //}


    //class MidiEventX
    //{

    //}


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
        #endregion

        #region Fields
        /// <summary>All the midi events by part/channel groups. This is the verbatim content of the file with no processing.</summary>
        Dictionary<int, List<MidiEvent>> _events = new Dictionary<int, List<MidiEvent>>();




        Dictionary<(string pattern, int channel), List<MidiEvent>> _eventsX = new Dictionary<(string pattern, int channel), List<MidiEvent>>();




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
            _events.Clear();
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
                    //Contents.Add($"Section:::{sectionName}");

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
                            //Contents.Add($"Done:::!!");
                            Loggo(-1, "Done", "!!!");

                            done = true;
                            break;
                    }
                }
            }

            Clipboard.SetText(string.Join(Environment.NewLine, AllFileContents));
        }

        /// <summary>
        /// Helper to get an event collection.
        /// </summary>
        /// <param name="channel"></param>
        /// <returns>The collection or null if invalid.</returns>
        public IList<MidiEvent> GetEvents(int channel)
        {
            _events.TryGetValue(channel, out List<MidiEvent> ret);
            return ret;
        }
        #endregion

        #region Section parsers
        /// <summary>
        /// Read the midi section of a style file.
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


        void Loggo(long timestamp, string etype, string content)
        {

            AllFileContents.Add($"{timestamp}:::{etype}:::{_lastPos}:::{content}");

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
            string chname = "???";

            uint chunkSize = Read(br, 4);
            long startPos = br.BaseStream.Position;
            int absoluteTime = 0;

            // Read all midi events. https://www.csie.ntu.edu.tw/~r92092/ref/midi/
            MidiEvent me = null; // current
            while (br.BaseStream.Position < startPos + chunkSize)
            {
                _lastPos = br.BaseStream.Position;

                me = MidiEvent.ReadNextEvent(br, me);
                absoluteTime += me.DeltaTime;
                me.AbsoluteTime = absoluteTime;

                switch (me.CommandCode)
                {
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
//                            AddMidiEvent(evt);
                        }
                        break;

                    case MidiCommandCode.PatchChange:
                        {
                            PatchChangeEvent evt = me as PatchChangeEvent;
                            chname = PatchChangeEvent.GetPatchName(evt.Patch);
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





                    case MidiCommandCode.MetaEvent when (me as MetaEvent).MetaEventType == MetaEventType.TrackSequenceNumber:
                        {
                            TrackSequenceNumberEvent evt = me as TrackSequenceNumberEvent;
                            chnum = evt.Channel;
                         //   Contents.Add($"TrackSequenceNumber:::{evt.Channel}");
                            Loggo(evt.AbsoluteTime, "TrackSequenceNumber", evt.ToString());
                        }
                        break;

                    case MidiCommandCode.MetaEvent when (me as MetaEvent).MetaEventType == MetaEventType.SequenceTrackName:
                        {
                            TextEvent evt = me as TextEvent;
                            chname = evt.Text;
                          //  Contents.Add($"SequenceTrackName:::{evt.Text} Channel:{evt.Channel}");
                            Loggo(evt.AbsoluteTime, "SequenceTrackName", evt.Text);
                        }
                        break;

                    case MidiCommandCode.MetaEvent when (me as MetaEvent).MetaEventType == MetaEventType.Marker:
                        {
                            string s = me.GetType().ToString();


                            // Indicates start of a new midi part. Bin per channel.
                            TextEvent evt = me as TextEvent;
                            //_currentPart = (me as TextEvent).Text;
                            //      Contents.Add($"Marker:::{me}");
                            Loggo(evt.AbsoluteTime, "Marker", evt.Text);
                            absoluteTime = 0;
                        }
                        break;

                    case MidiCommandCode.MetaEvent when (me as MetaEvent).MetaEventType == MetaEventType.EndTrack:
                        {
                            // Indicates end of current midi track.
                            MetaEvent evt = me as MetaEvent;
                            //  Contents.Add($"EndTrack:::{me}");
                            Loggo(evt.AbsoluteTime, "EndTrack", evt.ToString());
                        }
                        break;

                    case MidiCommandCode.MetaEvent when (me as MetaEvent).MetaEventType == MetaEventType.SetTempo:
                        {
                            TempoEvent evt = me as TempoEvent;
                            Tempo = evt.Tempo;
                         //   Contents.Add($"SetTempo:::{evt}");
                            Loggo(evt.AbsoluteTime, "SetTempo", evt.Tempo.ToString());
                        }
                        break;

                    case MidiCommandCode.MetaEvent when (me as MetaEvent).MetaEventType == MetaEventType.TimeSignature:
                        {
                            TimeSignatureEvent evt = me as TimeSignatureEvent;
                            TimeSig = evt.TimeSignature;
                          //  Contents.Add($"TimeSignature:::{evt}");
                            Loggo(evt.AbsoluteTime, "TimeSignature", evt.TimeSignature);
                        }
                        break;

                    case MidiCommandCode.MetaEvent when (me as MetaEvent).MetaEventType == MetaEventType.KeySignature:
                        {
                            KeySignatureEvent evt = me as KeySignatureEvent;
                            KeySig = evt.ToString();
                          //  Contents.Add($"KeySignature:::{evt}");
                            Loggo(evt.AbsoluteTime, "KeySignature", evt.ToString());
                        }
                        break;

                    case MidiCommandCode.MetaEvent when (me as MetaEvent).MetaEventType == MetaEventType.TextEvent:
                        {
                            // TODO there is one of these after "real" markers of interest - description of the section.
                            TextEvent evt = me as TextEvent;
                            Loggo(evt.AbsoluteTime, "TextEvent", evt.Text);
                        }
                        break;

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

                    default:
                        //Contents.Add($"{me.GetType()} {me}");
                        Loggo(-1, "Other", $"{me.GetType()} {me}");
                        break;
                }
            }

            ///// Local function. /////
            void AddMidiEvent(MidiEvent evt)
            {
                if (!_events.ContainsKey(evt.Channel))
                {
                    _events.Add(evt.Channel, new List<MidiEvent>());
                }

                if (!Channels.ContainsKey(evt.Channel))
                {
                    Channels.Add(evt.Channel, evt.Channel == 10 ? "Drums" : chname);
                }

                _events[evt.Channel].Add(evt);
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

        ///// <summary>
        ///// Endian support.
        ///// </summary>
        ///// <param name="i">Number to fix.</param>
        ///// <returns>Fixed number.</returns>
        //public UInt32 FixEndian(UInt32 i)
        //{
        //    if (BitConverter.IsLittleEndian)
        //    {
        //        return ((i & 0xFF000000) >> 24) | ((i & 0x00FF0000) >> 8) | ((i & 0x0000FF00) << 8) | ((i & 0x000000FF) << 24);
        //    }
        //    else
        //    {
        //        return i;
        //    }
        //}

        ///// <summary>
        ///// Endian support.
        ///// </summary>
        ///// <param name="i">Number to fix.</param>
        ///// <returns>Fixed number.</returns>
        //public UInt16 FixEndian(UInt16 i)
        //{
        //    if (BitConverter.IsLittleEndian)
        //    {
        //        return (UInt16)(((i & 0xFF00) >> 8) | ((i & 0x00FF) << 8));
        //    }
        //    else
        //    {
        //        return i;
        //    }
        //}
        #endregion
    }


    /* TODO other neb to add?
    public class MidiUtils
    {
        public const int MAX_MIDI = 127;
        public const int MAX_CHANNELS = 16;
        public const int MAX_PITCH = 16383;

        /// <summary>
        /// Convert neb steps to midi file.
        /// </summary>
        /// <param name="steps"></param>
        /// <param name="midiFileName"></param>
        /// <param name="channels">Map of channel number to channel name.</param>
        /// <param name="bpm">Beats per minute.</param>
        /// <param name="info">Extra info to add to midi file.</param>
        public static void ExportMidi(StepCollection steps, string midiFileName, Dictionary<int, string> channels, double bpm, string info)
        {
            int exportPpq = 96;

            // Events per track.
            Dictionary<int, IList<MidiEvent>> trackEvents = new Dictionary<int, IList<MidiEvent>>();

            ///// Meta file stuff.
            MidiEventCollection events = new MidiEventCollection(1, exportPpq);

            ///// Add Header chunk stuff.
            IList<MidiEvent> lhdr = events.AddTrack();
            //lhdr.Add(new TimeSignatureEvent(0, 4, 2, (int)ticksPerClick, 8));
            //TimeSignatureEvent me = new TimeSignatureEvent(long absoluteTime, int numerator, int denominator, int ticksInMetronomeClick, int no32ndNotesInQuarterNote);
            //  - numerator of the time signature (as notated).
            //  - denominator of the time signature as a negative power of 2 (ie 2 represents a quarter-note, 3 represents an eighth-note, etc).
            //  - number of MIDI clocks between metronome clicks.
            //  - number of notated 32nd-notes in a MIDI quarter-note (24 MIDI Clocks). The usual value for this parameter is 8.

            //lhdr.Add(new KeySignatureEvent(0, 0, 0));
            //  - number of flats (-ve) or sharps (+ve) that identifies the key signature (-7 = 7 flats, -1 = 1 //flat, 0 = key of C, 1 = 1 sharp, etc).
            //  - major (0) or minor (1) key.
            //  - abs time.

            // Tempo.
            lhdr.Add(new TempoEvent(0, 0) { Tempo = bpm });

            // General info.
            lhdr.Add(new TextEvent("Midi file created by Nebulator.", MetaEventType.TextEvent, 0));
            lhdr.Add(new TextEvent(info, MetaEventType.TextEvent, 0));

            lhdr.Add(new MetaEvent(MetaEventType.EndTrack, 0, 0));

            ///// Make one midi event collection per track.
            foreach (int channel in channels.Keys)
            {
                IList<MidiEvent> le = events.AddTrack();
                trackEvents.Add(channel, le);
                le.Add(new TextEvent(channels[channel], MetaEventType.SequenceTrackName, 0));
                // >> 0 SequenceTrackName G.MIDI Acou Bass
            }

            // Make a transformer.
            MidiTime mt = new MidiTime()
            {
                InternalPpq = Time.SUBDIVS_PER_BEAT,
                MidiPpq = exportPpq,
                Tempo = bpm
            };

            // Run through the main steps and create a midi event per.
            foreach (Time time in steps.Times)
            {
                long mtime = mt.InternalToMidi(time.TotalSubdivs);

                foreach (Step step in steps.GetSteps(time))
                {
                    MidiEvent evt = null;

                    switch (step)
                    {
                        case StepNoteOn stt:
                            evt = new NoteEvent(mtime,
                                stt.ChannelNumber,
                                MidiCommandCode.NoteOn,
                                (int)MathUtils.Constrain(stt.NoteNumber, 0, MidiUtils.MAX_MIDI),
                                (int)(MathUtils.Constrain(stt.VelocityToPlay, 0, 1.0) * MidiUtils.MAX_MIDI));
                            trackEvents[step.ChannelNumber].Add(evt);

                            if (stt.Duration.TotalSubdivs > 0) // specific duration
                            {
                                evt = new NoteEvent(mtime + mt.InternalToMidi(stt.Duration.TotalSubdivs),
                                    stt.ChannelNumber,
                                    MidiCommandCode.NoteOff,
                                    (int)MathUtils.Constrain(stt.NoteNumber, 0, MidiUtils.MAX_MIDI),
                                    0);
                                trackEvents[step.ChannelNumber].Add(evt);
                            }
                            break;

                        case StepNoteOff stt:
                            evt = new NoteEvent(mtime,
                                stt.ChannelNumber,
                                MidiCommandCode.NoteOff,
                                (int)MathUtils.Constrain(stt.NoteNumber, 0, MidiUtils.MAX_MIDI),
                                0);
                            trackEvents[step.ChannelNumber].Add(evt);
                            break;

                        case StepControllerChange stt:
                            if (stt.ControllerId == ScriptDefinitions.TheDefinitions.NoteControl)
                            {
                                // Shouldn't happen, ignore.
                            }
                            else if (stt.ControllerId == ScriptDefinitions.TheDefinitions.PitchControl)
                            {
                                evt = new PitchWheelChangeEvent(mtime,
                                    stt.ChannelNumber,
                                    (int)MathUtils.Constrain(stt.Value, 0, MidiUtils.MAX_MIDI));
                                trackEvents[step.ChannelNumber].Add(evt);
                            }
                            else // CC
                            {
                                evt = new ControlChangeEvent(mtime,
                                    stt.ChannelNumber,
                                    (MidiController)stt.ControllerId,
                                    (int)MathUtils.Constrain(stt.Value, 0, MidiUtils.MAX_MIDI));
                                trackEvents[step.ChannelNumber].Add(evt);
                            }
                            break;

                        case StepPatch stt:
                            evt = new PatchChangeEvent(mtime,
                                stt.ChannelNumber,
                                stt.PatchNumber);
                            trackEvents[step.ChannelNumber].Add(evt);
                            break;

                        default:
                            break;
                    }
                }
            }

            // Finish up channels with end marker.
            foreach (IList<MidiEvent> let in trackEvents.Values)
            {
                long ltime = let.Last().AbsoluteTime;
                let.Add(new MetaEvent(MetaEventType.EndTrack, 0, ltime));
            }

            MidiFile.Export(midiFileName, events);
        }

        /// <summary>
        /// Read a midi or style file into text that can be placed in a neb file.
        /// It attempts to clean up any issues in the midi event data e.g. note on/off mismatches.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>Collection of strings for pasting into a neb file.</returns>
        public static List<string> ImportFile(string fileName)
        {
            FileParser fpars = new FileParser();
            fpars.ProcessFile(fileName);

            List<string> defs = new List<string>
            {
                $"Tempo:{fpars.Tempo}",
                $"TimeSig:{fpars.TimeSig}",
                $"DeltaTicksPerQuarterNote:{fpars.DeltaTicksPerQuarterNote}",
                $"KeySig:{fpars.KeySig}"
            };

            foreach (KeyValuePair<int, string> kv in fpars.Channels)
            {
                int chnum = kv.Key;
                string chname = kv.Value;

                defs.Add("");
                defs.Add($"================================================================================");
                defs.Add($"====== Channel {chnum} {chname} ");
                defs.Add($"================================================================================");

                // Current note on events that are waiting for corresponding note offs.
                LinkedList<NoteOnEvent> ons = new LinkedList<NoteOnEvent>();

                // Collected and processed events.
                List<NoteOnEvent> validEvents = new List<NoteOnEvent>();

                // Make a transformer.
                MidiTime mt = new MidiTime()
                {
                    InternalPpq = Time.SUBDIVS_PER_BEAT,
                    MidiPpq = fpars.DeltaTicksPerQuarterNote,
                    Tempo = fpars.Tempo
                };

                foreach (MidiEvent evt in fpars.GetEvents(chnum))
                {
                    // Run through each group of events
                    switch (evt)
                    {
                        case NoteOnEvent onevt:
                            {
                                if (onevt.OffEvent != null)
                                {
                                    // Self contained - just save it.
                                    validEvents.Add(onevt);
                                    // Reset it.
                                    ons.AddLast(onevt);
                                }
                                else if (onevt.Velocity == 0)
                                {
                                    // It's actually a note off - handle as such. Locate the initiating note on.

                                    var on = ons.First(o => o.NoteNumber == onevt.NoteNumber);
                                    if (on != null)
                                    {
                                        // Found it.
                                        on.OffEvent = new NoteEvent(onevt.AbsoluteTime, onevt.Channel, MidiCommandCode.NoteOff, onevt.NoteNumber, 0);
                                        validEvents.Add(on);
                                        ons.Remove(on); // reset
                                    }
                                    else
                                    {
                                        // hmmm...
                                        fpars.Leftovers.Add($"NoteOff: NoteOnEvent with vel=0: {onevt}");
                                    }
                                }
                                else
                                {
                                    // True note on - save it until note off shows up.
                                    ons.AddLast(onevt);
                                }
                            }
                            break;

                        case NoteEvent nevt:
                            {
                                if (nevt.CommandCode == MidiCommandCode.NoteOff || nevt.Velocity == 0)
                                {
                                    // It's actually a note off - handle as such. Locate the initiating note on.
                                    var on = ons.First(o => o.NoteNumber == nevt.NoteNumber);
                                    if (on != null)
                                    {
                                        // Found it.
                                        on.OffEvent = new NoteEvent(nevt.AbsoluteTime, nevt.Channel, MidiCommandCode.NoteOff, nevt.NoteNumber, 0);
                                        validEvents.Add(on);
                                        ons.Remove(on); // reset
                                    }
                                    else
                                    {
                                        // hmmm... see below
                                        fpars.Leftovers.Add($"NoteOff: NoteEvent in part {nevt}");
                                    }
                                }
                                // else ignore.
                            }
                            break;
                    }

                    // Check for note tracking leftovers.
                    foreach (NoteOnEvent on in ons)
                    {
                        if (on != null)
                        {
                            Time when = new Time(mt.MidiToInternal(on.AbsoluteTime));
                            // ? fpars.Leftovers.Add($"Orphan NoteOn: {when} {on.Channel} {on.NoteNumber}");
                        }
                    }

                    // Process the collected valid events.
                    if (validEvents.Count > 0)
                    {
                        validEvents.Sort((a, b) => a.AbsoluteTime.CompareTo(b.AbsoluteTime));
                        long duration = validEvents.Last().AbsoluteTime; // - validEvents.First().AbsoluteTime;
                        Time tdur = new Time(mt.MidiToInternal(duration));
                        tdur.RoundUp();

                        // Process each set of notes at each discrete play time.
                        foreach (IEnumerable<NoteOnEvent> nevts in validEvents.GroupBy(e => e.AbsoluteTime))
                        {
                            List<int> notes = new List<int>(nevts.Select(n => n.NoteNumber));
                            //notes.Sort();

                            NoteOnEvent noevt = nevts.ElementAt(0);
                            Time when = new Time(mt.MidiToInternal(noevt.AbsoluteTime));
                            Time dur = new Time(mt.MidiToInternal(noevt.NoteLength));
                            double vel = (double)noevt.Velocity / MAX_MIDI;

                            if (chnum == 10)
                            {
                                // Drums - one line per hit.
                                foreach (int d in notes)
                                {
                                    string sdrum = NoteUtils.FormatDrum(d);
                                    defs.Add($"{{ {when}, {sdrum}, {vel:0.00} }},");
                                }
                            }
                            else
                            {
                                // Instrument - note(s) or chord.
                                foreach (string sn in NoteUtils.FormatNotes(notes))
                                {
                                    defs.Add($"{{ {when}, {sn}, {vel:0.00}, {dur} }},");
                                }
                            }
                        }

                        validEvents.Clear();
                    }
                }
            }

            defs.Add("");

            List<string> all = new List<string>();

            all.AddRange(defs);

            if(fpars.Leftovers.Count > 0)
            {
                all.Add($"");
                all.Add($"================================================================================");
                all.Add($"====== Leftovers ");
                all.Add($"================================================================================");

                all.AddRange(fpars.Leftovers);
            }
            return all;
        }
    }
    */
}
