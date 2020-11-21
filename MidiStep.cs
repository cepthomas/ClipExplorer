using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio.Midi;
using NBagOfTricks.Utils;


namespace ClipExplorer
{
    /// <summary>
    /// Container for raw midi event and internal stuff.
    /// </summary>
    public class MidiStep
    {
        public MidiEvent RawEvent { get; set; } = null;

        /// <summary>The possibly modified Volume.</summary>
        public double VelocityToPlay { get; set; } = 0.5;

        /// <summary>For viewing pleasure.</summary>
        // public override string ToString()
        // {
        //     return $"channel:{ChannelNumber}";
        // }

        // public void Adjust(double masterVolume, double channelVolume)
        // {
        //     // Maybe alter note velocity.
        //     if (Device is NOutput)
        //     {
        //         NOutput nout = Device as NOutput;
        //         double vel = Velocity * channelVolume * masterVolume;
        //         VelocityToPlay = MathUtils.Constrain(vel, 0, 1.0);
        //     }
        // }

        // /// <summary>For viewing pleasure.</summary>
        // public override string ToString()
        // {
        //     return $"StepNoteOn: {base.ToString()} note:{NoteNumber:F2} vel:{VelocityToPlay:F2} dur:{Duration}";
        // }

    }


    /// <summary>A collection of Steps.</summary>
    public class MidiStepCollection
    {
        #region Fields
        ///<summary>The main collection of Steps. The key is the time to send the list.</summary>
        Dictionary<MidiTime, List<MidiStep>> _steps = new Dictionary<MidiTime, List<MidiStep>>();
        #endregion

        #region Properties
        ///<summary>Gets a collection of the times in _steps.</summary>
        public IEnumerable<MidiTime> Times { get { return _steps.Keys.OrderBy(k => k.TotalSubBeats); } }

        ///<summary>The duration of the whole thing.</summary>
        public int MaxBeat { get; private set; } = 0;
        #endregion

        #region Functions
        /// <summary>
        /// Add a step at the given time.
        /// </summary>
        /// <param name="time"></param>
        /// <param name="step"></param>
        public void AddStep(MidiTime time, MidiStep step)
        {
            if (!_steps.ContainsKey(time))
            {
                _steps.Add(time, new List<MidiStep>());
            }
            _steps[time].Add(step);

            MaxBeat = Math.Max(MaxBeat, time.Beat);
        }

        ///// <summary>
        ///// Concatenate another collection to this.
        ///// </summary>
        ///// <param name="stepsToAdd"></param>
        //public void Add(MidiStepCollection stepsToAdd)
        //{
        //    foreach(KeyValuePair<MidiTime, List<MidiStep>> kv in stepsToAdd._steps)
        //    {
        //        kv.Value.ForEach(s => AddStep(kv.Key, s));
        //    }
        //}

        /// <summary>
        /// Get the steps for the given time.
        /// </summary>
        public IEnumerable<MidiStep> GetSteps(MidiTime time)
        {
            return _steps.ContainsKey(time) ? _steps[time] : new List<MidiStep>();
        }

        ///// <summary>
        ///// Delete the steps at the given time.
        ///// </summary>
        //public void DeleteSteps(MidiTime time)
        //{
        //    _steps.Remove(time);
        //}

        ///// <summary>
        ///// Cleanse me.
        ///// </summary>
        //public void Clear()
        //{
        //    _steps.Clear();
        //}

        /// <summary>
        /// Display the content steps.
        /// </summary>
        public override string ToString()
        {
            return $"Times:{_steps.Keys.Count} TotalSteps:{_steps.Values.Sum(v => v.Count)}";

            //StringBuilder sb = new StringBuilder();
            //foreach (MidiTime time in Times)
            //{
            //    foreach (MidiStep step in GetSteps(time))
            //    {
            //        sb.Append($"{time} {step}{Environment.NewLine}");
            //    }
            //}
            //return sb.ToString();
        }
        #endregion
    }
}
