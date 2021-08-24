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
    /// TODO Doesn't support multiple tracks. Would it be useful.
    /// </summary>
    public class MidiFile
    {
        #region Properties gleaned from the file
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
        /// <param name="fileName"></param>
        public void ProcessFile(string fileName)
        {
            // Init everything.
            _midiEvents.Clear();
            AllPatterns.Clear();
            //Patches.ForEach(p => p = -1);
            Channels.Clear();
            DeltaTicksPerQuarterNote = 0;
            Tempo = 100;
            TimeSig = "";
            KeySig = "";

            _allContents.Clear();
            _allContents.Add($"Timestamp,Type,Pattern,Channel,FilePos,Content");

            using (var br = new BinaryReader(File.OpenRead(fileName)))
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
        /// Dump the contents in a readable form. This is as they appear in the original file.
        /// </summary>
        /// <returns></returns>
        public List<string> GetReadableContents()
        {
            return _allContents;
        }

        /// <summary>
        /// Makes csv dumps of events grouped by pattern/channel.
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
                        string pname = evt.Channel == DrumChannel ? $"0" : $"{MidiDefs.GetInstrumentDef(evt.Patch)}"; // TODO kit?
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
        /// <param name="midiFileName">Where to put the midi.</param>
        /// <param name="info">Extra info to add to midi file.</param>
        public void ExportMidi(string midiFileName, string info)
        {
            if(AllPatterns.Count > 0)
            {
                ExportOneMidi(midiFileName, "", info);
            }
            else
            {
                AllPatterns.ForEach(p => ExportOneMidi(midiFileName, p, info));
            }
        }

        public void ExportOneMidi(string midiFileName, string pattern, string info) //TODO 
        {
            // Timestamp,Type             ,Content
            // -1       ,Section          ,MThd
            // -1       ,Section          ,MTrk
            // 0        ,TimeSignature    ,4/4
            // 0        ,SetTempo         ,70.00007000007
            // 0        ,PatchChangeEvent ,0 PatchChange Ch: 13 String Ensemble 1
            // 0        ,PatchChangeEvent ,0 PatchChange Ch: 11 String Ensemble 1
            //.................
            // 0        ,NoteOn           ,0 NoteOn Ch: 3 C2 Vel:100 Len: ?
            // 0        ,NoteOn           ,0 NoteOn Ch: 4 A#4 Vel:53 Len: ?
            // 0        ,NoteOn           ,0 NoteOn Ch: 4 D#5 Vel:65 Len: ?
            // 0        ,NoteOn           ,0 NoteOn Ch: 4 F5 Vel:66 Len: ?
            // 0        ,NoteOn           ,0 NoteOn Ch: 7 C5 Vel:60 Len: ?
            // 0        ,NoteOn           ,0 NoteOn Ch: 9 A5 Vel:40 Len: ?
            // 0        ,NoteOn           ,0 NoteOn Ch: 10 Bass Drum 1 Vel:72 Len: ?
            // 20       ,NoteOn           ,20 NoteOn Ch: 5 C5 Vel:47 Len: ?
            // 20       ,ControlChange    ,20 ControlChange Ch: 5 Controller Expression Value 98
            // 20       ,NoteOff          ,20 NoteOff Ch: 9 A5 Vel:64
            // 20       ,NoteOff          ,20 NoteOff Ch: 10 Bass Drum 1 Vel:64
            // 20       ,NoteOn           ,20 NoteOn Ch: 11 F4 Vel:47 Len: ?
            // 20       ,NoteOn           ,20 NoteOn Ch: 11 A#4 Vel:47 Len: ?
            // 20       ,NoteOn           ,20 NoteOn Ch: 11 D#5 Vel:48 Len: ?
            // 20       ,ControlChange    ,20 ControlChange Ch: 11 Controller Expression Value 98
            // ..............
            // 5760     ,NoteOn           ,5760 NoteOn Ch: 10 Electric Snare Vel:88 Len: ?
            // 5760     ,NoteOn           ,5760 NoteOn Ch: 10 Pedal Hi-Hat Vel:53 Len: ?
            // 5780     ,NoteOff          ,5780 NoteOff Ch: 10 Electric Snare Vel:64
            // 5780     ,NoteOff          ,5780 NoteOff Ch: 10 Pedal Hi-Hat Vel:64
            // 6720     ,NoteOn           ,6720 NoteOn Ch: 10 Electric Snare Vel:72 Len: ?
            // 6740     ,NoteOff          ,6740 NoteOff Ch: 10 Electric Snare Vel:64
            // 7200     ,NoteOn           ,7200 NoteOn Ch: 10 High Floor Tom Vel:73 Len: ?
            // 7220     ,NoteOff          ,7220 NoteOff Ch: 10 High Floor Tom Vel:64
            // 7660     ,NoteOn           ,7660 NoteOn Ch: 10 Crash Cymbal 1 Vel:58 Len: ?
            // 7680     ,NoteOff          ,7680 NoteOff Ch: 10 Crash Cymbal 1 Vel:64
            // 7680     ,EndTrack         ,7680 EndTrack




            int exportPpq = 96; // arbitrary
            MidiEventCollection events = new MidiEventCollection(1, exportPpq);

            ///// Header chunk stuff.
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
            lhdr.Add(new TempoEvent(0, 0) { Tempo = Tempo });

            // General info.
            lhdr.Add(new TextEvent(info, MetaEventType.TextEvent, 0));

            // Patches.
            // !!!!!!!!!!!!!!


            ///// Make one midi event collection per track.
            //foreach (int channel in channels.Keys)
            //{
            //    IList<MidiEvent> le = events.AddTrack();
            //    trackEvents.Add(channel, le);
            //    le.Add(new TextEvent(channels[channel], MetaEventType.SequenceTrackName, 0));
            //    // >> 0 SequenceTrackName G.MIDI Acou Bass
            //}

/*         
            // Run through the main steps and create a midi event per.
            foreach (Time time in steps.Times)
            {
                long mtime = mt.InternalToMidi(time.TotalSubdivs);
 Order by timestamp
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

            NAudio.Midi.MidiFile.Export(midiFileName, events);
*/
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
