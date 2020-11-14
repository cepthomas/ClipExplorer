using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.Midi;
using NBagOfTricks.Utils;


namespace ClipExplorer
{
    public class MidiPlayer : IDisposable
    {
        // TODOC Mute/solo.


        public const int MAX_MIDI = 127;
        public const int MAX_CHANNELS = 16;
        public const int MAX_PITCH = 16383;

        TimeSpan _stepTime = new TimeSpan();

        /// <summary>Midi output device.</summary>
        MidiOut _midiOut = null;

        /// <summary>Notes to stop later.</summary>
 //       List<StepNoteOff> _stops = new List<StepNoteOff>();

        //public string DeviceName { get; private set; } = "???";

        public MidiEventCollection mevts = null;

        /// <summary>Human readable midi file contents.</summary>
        public List<string> Contents { get; private set; } = new List<string>();

        //MidiFile.Export(fileName, mevts);


        public bool Init(string devName)
        {
            bool inited = false;

            //DeviceName = "Invalid"; // default

            try
            {
                if (_midiOut != null)
                {
                    _midiOut.Dispose();
                    _midiOut = null;
                }

                // Figure out which device.
                for (int device = 0; device < MidiOut.NumberOfDevices; device++)
                {
                    if (devName == MidiOut.DeviceInfo(device).ProductName)
                    {
                        _midiOut = new MidiOut(device);
                        inited = true;
                        //DeviceName = devName;
                        inited = true;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                //LogMsg(DeviceLogCategory.Error, $"Init midi out failed: {ex.Message}");
                inited = false;
            }

            return inited;
        }

        public void Dispose()
        {
            _midiOut?.Dispose();
            _midiOut = null;
        }

        public void Housekeep()
        {
            // Send any stops due.
            //           _stops.ForEach(s => { s.Expiry--; if (s.Expiry < 0) SendXXX(s); });

            // Reset.
            //           _stops.RemoveAll(s => s.Expiry < 0);
        }


        public void LoadFile(string fileName)
        {
            var mfile = new NAudio.Midi.MidiFile(fileName, true);

            mevts = mfile.Events;

        }





        /// <summary>
        /// Output next time/step.
        /// </summary>
        /// <param name="e">Information about updates required.</param>
        public void NextStep(MmTimerEx.TimerEventArgs e)
        {
            bool running = true;

            //if (running && e.ElapsedTimers.Contains("TODOC"))
            //{
            //    // get neb steps
            //    GetSteps(_stepTime).ForEach(s => PlayStep(s));

            //    ///// Bump time.
            //    _stepTime.Advance();

            //}

            //// Process any lingering noteoffs etc.
            //_outputs.ForEach(o => o.Value?.Housekeep());
            //_inputs.ForEach(i => i.Value?.Housekeep());

            /////// Local common function /////
            //void PlayStep(Step step)
            //{
            //    if (_script.Channels.Count > 0)
            //    {
            //        NChannel channel = _script.Channels.Where(t => t.ChannelNumber == step.ChannelNumber).First();

            //        // Is it ok to play now?
            //        bool _anySolo = _script.Channels.Where(t => t.State == ChannelState.Solo).Count() > 0;
            //        bool play = channel != null && (channel.State == ChannelState.Solo || (channel.State == ChannelState.Normal && !_anySolo));

            //        if (play)
            //        {
            //            (step.Device as NOutput).SendXXX(step);
            //        }
            //    }
            //}
        }

        /// <summary>
        /// User has changed a channel value. Interested in solo/mute and volume.
        /// </summary>
        void ChannelChange_Event(object sender, EventArgs e)
        {
            //ChannelControl ch = sender as ChannelControl;
            //_nppVals.SetValue(ch.BoundChannel.Name, "volume", ch.BoundChannel.Volume);

            //// Check for solos.
            //bool _anySolo = _script.Channels.Where(c => c.State == ChannelState.Solo).Count() > 0;

            //if (_anySolo)
            //{
            //    // Kill any not solo.
            //    _script.Channels.ForEach(c =>
            //    {
            //        if (c.State != ChannelState.Solo && c.Device != null)
            //        {
            //            c.Device.Kill(c.ChannelNumber);
            //        }
            //    });
            //}
        }


        /////////////////////////////////////////////////////////////////

        // converts from neb to midi
        //public bool SendXXX(Step step)
        //{
        //    bool ret = true;

        //    if (_midiOut != null)
        //    {
        //        List<int> msgs = new List<int>();
        //        int msg = 0;

        //        switch (step)
        //        {
        //            case StepNoteOn stt:
        //                {
        //                    NoteEvent evt = new NoteEvent(0,
        //                        stt.ChannelNumber,
        //                        MidiCommandCode.NoteOn,
        //                        (int)MathUtils.Constrain(stt.NoteNumber, 0, MAX_MIDI),
        //                        (int)(MathUtils.Constrain(stt.VelocityToPlay, 0, 1.0) * MAX_MIDI));
        //                    msg = evt.GetAsShortMessage();

        //                    if (stt.Duration.TotalTicks > 0) // specific duration
        //                    {
        //                        // Remove any lingering note offs and add a fresh one.
        //                        _stops.RemoveAll(s => s.NoteNumber == stt.NoteNumber && s.ChannelNumber == stt.ChannelNumber);

        //                        _stops.Add(new StepNoteOff()
        //                        {
        //                            ChannelNumber = stt.ChannelNumber,
        //                            NoteNumber = MathUtils.Constrain(stt.NoteNumber, 0, MAX_MIDI),
        //                            Expiry = stt.Duration.TotalTicks
        //                        });
        //                    }
        //                }
        //                break;

        //            case StepNoteOff stt:
        //                {
        //                    NoteEvent evt = new NoteEvent(0,
        //                        stt.ChannelNumber,
        //                        MidiCommandCode.NoteOff,
        //                        (int)MathUtils.Constrain(stt.NoteNumber, 0, MAX_MIDI),
        //                        0);
        //                    msg = evt.GetAsShortMessage();
        //                }
        //                break;

        //            case StepControllerChange stt:
        //                {
        //                    //if (stt.ControllerId == ScriptDefinitions.TheDefinitions.NoteControl)
        //                    //{
        //                    //    // Shouldn't happen, ignore.
        //                    //}
        //                    //else if (stt.ControllerId == ScriptDefinitions.TheDefinitions.PitchControl)
        //                    //{
        //                    //    PitchWheelChangeEvent pevt = new PitchWheelChangeEvent(0,
        //                    //        stt.ChannelNumber,
        //                    //        (int)MathUtils.Constrain(stt.Value, 0, MidiUtils.MAX_PITCH));
        //                    //    msg = pevt.GetAsShortMessage();
        //                    //}
        //                    //else // CC
        //                    {
        //                        ControlChangeEvent nevt = new ControlChangeEvent(0,
        //                            stt.ChannelNumber,
        //                            (MidiController)stt.ControllerId,
        //                            (int)MathUtils.Constrain(stt.Value, 0, MAX_MIDI));
        //                        msg = nevt.GetAsShortMessage();
        //                    }
        //                }
        //                break;

        //            case StepPatch stt:
        //                {
        //                    PatchChangeEvent evt = new PatchChangeEvent(0,
        //                        stt.ChannelNumber,
        //                        stt.PatchNumber);
        //                    msg = evt.GetAsShortMessage();
        //                }
        //                break;

        //            default:
        //                break;
        //        }

        //        if (msg != 0)
        //        {
        //            _midiOut.Send(msg);
        //        }
        //    }

        //    return ret;
        //}

        public void Kill(int? channel)
        {
            //if (channel is null)
            //{
            //    for (int i = 0; i < MAX_CHANNELS; i++)
            //    {
            //        SendXXX(new StepControllerChange()
            //        {
            //            ChannelNumber = i + 1,
            //            ControllerId = (int)MidiController.AllNotesOff
            //        });
            //    }
            //}
            //else
            //{
            //    SendXXX(new StepControllerChange()
            //    {
            //        ChannelNumber = channel.Value,
            //        ControllerId = (int)MidiController.AllNotesOff
            //    });
            //}
        }


        ///////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////

        // if (MidiOut.NumberOfDevices > 0)
        // {
        //     for (int device = 0; device < MidiOut.NumberOfDevices; device++)
        //     {
        //         mdText.Add($"- {MidiOut.DeviceInfo(device).ProductName}");
        //     }
        // }


        /// <summary>
        /// Common func.
        /// </summary>
        // void SetSpeedTimerPeriod()
        // {
        //     double secPerBeat = 60 / potSpeed.Value; // aka beat
        //     double msecPerBeat = 1000 * secPerBeat / Time.TICKS_PER_BEAT;
        //     _timer.SetTimer("NEB", (int)msecPerBeat);
        // }


        // Dictionary<int, string> channels = new Dictionary<int, string>();
        // _script.Channels.ForEach(t => channels.Add(t.ChannelNumber, t.Name));

        // // Convert bpm to sec per beat.
        // double beatsPerSec = potSpeed.Value / 60;
        // double secPerBeat = 1 / beatsPerSec;

        // MidiUtils.ExportMidi(_script.Steps, fn, channels, secPerBeat, "Converted from " + _fn);

    }


    public class Client : IDisposable
    {
        /// <summary>Fast timer.</summary>
        MmTimerEx _timer = new MmTimerEx();

        TimeSpan _stepTime = new TimeSpan();

        MidiPlayer _player = new MidiPlayer();

        public Client()
        {
            _timer.TimerElapsedEvent += TimerElapsedEvent;
            _timer.Start();
        }

        public void Dispose()
        {
            _timer?.Stop();
            _timer?.Dispose();
            _timer = null;
        }

        /// <summary>
        /// Multimedia timer tick handler.
        /// </summary>
        void TimerElapsedEvent(object sender, MmTimerEx.TimerEventArgs e)
        {
            _player.NextStep(e);

            //// Kick over to main UI thread.
            //BeginInvoke((MethodInvoker)delegate ()
            //{
            //    if (_script != null)
            //    {
            //        NextStep(e);
            //    }
            //});
        }
    }


#if TODOC_MAIN_STUFF

    public class MainFormXXX : Form
    {
        /// <summary>Fast timer.</summary>
        MmTimerEx _timer = new MmTimerEx();

        void MainFormXXX_Load(object sender, EventArgs e)
        {
            // Fast mm timer.
            _timer = new MmTimerEx();
//            SetSpeedTimerPeriod();
            _timer.TimerElapsedEvent += TimerElapsedEvent;
            _timer.Start();
        }

        /// <summary>
        /// Resource clean up.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer?.Stop();
                _timer?.Dispose();
                _timer = null;
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Multimedia timer tick handler.
        /// </summary>
        void TimerElapsedEvent(object sender, MmTimerEx.TimerEventArgs e)
        {
            // Kick over to main UI thread.
            BeginInvoke((MethodInvoker)delegate ()
            {
                //if (_script != null)
                {
                    NextStep(e);
                }
            });
        }

        /// <summary>
        /// Output next time/step.
        /// </summary>
        /// <param name="e">Information about updates required.</param>
        void NextStep(MmTimerEx.TimerEventArgs e)
        {
            if (chkPlay.Checked && e.ElapsedTimers.Contains("NEB") && !_needCompile)
            {
                // Kick the script. Note: Need exception handling here to protect from user script errors.
                try
                {
                    _script.Step();
                }
                catch (Exception ex)
                {
                    ProcessScriptRuntimeError(ex);
                }

                // Process any sequence steps.
                _script.Steps.GetSteps(_stepTime).ForEach(s => PlayStep(s));

                ///// Bump time.
                _stepTime.Advance();

            }

            // Process any lingering noteoffs etc.
            _outputs.ForEach(o => o.Value?.Housekeep());
            _inputs.ForEach(i => i.Value?.Housekeep());

            ///// Local common function /////
            void PlayStep(Step step)
            {
                if(_script.Channels.Count > 0)
                {
                    NChannel channel = _script.Channels.Where(t => t.ChannelNumber == step.ChannelNumber).First();

                    // Is it ok to play now?
                    bool _anySolo = _script.Channels.Where(t => t.State == ChannelState.Solo).Count() > 0;
                    bool play = channel != null && (channel.State == ChannelState.Solo || (channel.State == ChannelState.Normal && !_anySolo));

                    if (play)
                    {
                        if (step is StepInternal)
                        {
                            // Note: Need exception handling here to protect from user script errors.
                            try
                            {
                                (step as StepInternal).ScriptFunction();
                            }
                            catch (Exception ex)
                            {
                                ProcessScriptRuntimeError(ex);
                            }
                        }
                        else
                        {
                            if (step.Device is NOutput)
                            {
                                // Maybe tweak values.
                                if (step is StepNoteOn)
                                {
                                    (step as StepNoteOn).Adjust(sldVolume.Value, channel.Volume);
                                }
                                (step.Device as NOutput).Send(step);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// User has changed a channel value. Interested in solo/mute and volume.
        /// </summary>
        void ChannelChange_Event(object sender, EventArgs e)
        {
            ChannelControl ch = sender as ChannelControl;
            _nppVals.SetValue(ch.BoundChannel.Name, "volume", ch.BoundChannel.Volume);

            // Check for solos.
            bool _anySolo = _script.Channels.Where(c => c.State == ChannelState.Solo).Count() > 0;

            if (_anySolo)
            {
                // Kill any not solo.
                _script.Channels.ForEach(c =>
                {
                    if (c.State != ChannelState.Solo && c.Device != null)
                    {
                        c.Device.Kill(c.ChannelNumber);
                    }
                });
            }
        }


        // if (MidiOut.NumberOfDevices > 0)
        // {
        //     for (int device = 0; device < MidiOut.NumberOfDevices; device++)
        //     {
        //         mdText.Add($"- {MidiOut.DeviceInfo(device).ProductName}");
        //     }
        // }


        /// <summary>
        /// Common func.
        /// </summary>
        // void SetSpeedTimerPeriod()
        // {
        //     double secPerBeat = 60 / potSpeed.Value; // aka beat
        //     double msecPerBeat = 1000 * secPerBeat / Time.TICKS_PER_BEAT;
        //     _timer.SetTimer("NEB", (int)msecPerBeat);
        // }


        // Dictionary<int, string> channels = new Dictionary<int, string>();
        // _script.Channels.ForEach(t => channels.Add(t.ChannelNumber, t.Name));

        // // Convert bpm to sec per beat.
        // double beatsPerSec = potSpeed.Value / 60;
        // double secPerBeat = 1 / beatsPerSec;

        // MidiUtils.ExportMidi(_script.Steps, fn, channels, secPerBeat, "Converted from " + _fn);

    }
#endif

#if TODOC_ORIGINAL
        public bool Send(Step step)
        {
            bool ret = true;

            // Critical code section.
            lock (_lock)
            {
                if(_midiOut != null)
                {
                    List<int> msgs = new List<int>();
                    int msg = 0;

                    switch (step)
                    {
                        case StepNoteOn stt:
                            {
                                NoteEvent evt = new NoteEvent(0,
                                    stt.ChannelNumber,
                                    MidiCommandCode.NoteOn,
                                    (int)MathUtils.Constrain(stt.NoteNumber, 0, MidiUtils.MAX_MIDI),
                                    (int)(MathUtils.Constrain(stt.VelocityToPlay, 0, 1.0) * MidiUtils.MAX_MIDI));
                                msg = evt.GetAsShortMessage();

                                if (stt.Duration.TotalTicks > 0) // specific duration
                                {
                                    // Remove any lingering note offs and add a fresh one.
                                    _stops.RemoveAll(s => s.NoteNumber == stt.NoteNumber && s.ChannelNumber == stt.ChannelNumber);

                                    _stops.Add(new StepNoteOff()
                                    {
                                        Device = stt.Device,
                                        ChannelNumber = stt.ChannelNumber,
                                        NoteNumber = MathUtils.Constrain(stt.NoteNumber, 0, MidiUtils.MAX_MIDI),
                                        Expiry = stt.Duration.TotalTicks
                                    });
                                }
                            }
                            break;

                        case StepNoteOff stt:
                            {
                                NoteEvent evt = new NoteEvent(0,
                                    stt.ChannelNumber,
                                    MidiCommandCode.NoteOff,
                                    (int)MathUtils.Constrain(stt.NoteNumber, 0, MidiUtils.MAX_MIDI),
                                    0);
                                msg = evt.GetAsShortMessage();
                            }
                            break;

                        case StepControllerChange stt:
                            {
                                if (stt.ControllerId == ScriptDefinitions.TheDefinitions.NoteControl)
                                {
                                    // Shouldn't happen, ignore.
                                }
                                else if (stt.ControllerId == ScriptDefinitions.TheDefinitions.PitchControl)
                                {
                                    PitchWheelChangeEvent pevt = new PitchWheelChangeEvent(0,
                                        stt.ChannelNumber,
                                        (int)MathUtils.Constrain(stt.Value, 0, MidiUtils.MAX_PITCH));
                                    msg = pevt.GetAsShortMessage();
                                }
                                else // CC
                                {
                                    ControlChangeEvent nevt = new ControlChangeEvent(0,
                                        stt.ChannelNumber,
                                        (MidiController)stt.ControllerId,
                                        (int)MathUtils.Constrain(stt.Value, 0, MidiUtils.MAX_MIDI));
                                    msg = nevt.GetAsShortMessage();
                                }
                            }
                            break;

                        case StepPatch stt:
                            {
                                PatchChangeEvent evt = new PatchChangeEvent(0,
                                    stt.ChannelNumber,
                                    stt.PatchNumber);
                                msg = evt.GetAsShortMessage();
                            }
                            break;

                        default:
                            break;
                    }

                    if(msg != 0)
                    {
                        _midiOut.Send(msg);
                        LogMsg(DeviceLogCategory.Send, step.ToString());
                    }
                }
            }

            return ret;
        }

        public void Kill(int? channel)
        {
            if(channel is null)
            {
                for (int i = 0; i < MidiUtils.MAX_CHANNELS; i++)
                {
                    Send(new StepControllerChange()
                    {
                        Device = this,
                        ChannelNumber = i + 1,
                        ControllerId = (int)MidiController.AllNotesOff
                    });
                }
            }
            else
            {
                Send(new StepControllerChange()
                {
                    Device = this,
                    ChannelNumber = channel.Value,
                    ControllerId = (int)MidiController.AllNotesOff
                });
            }
        }
#endif


}
