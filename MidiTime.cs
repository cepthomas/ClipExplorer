using System;
using System.Collections.Generic;
using NBagOfTricks.Utils;


namespace ClipExplorer
{
    /// <summary>
    /// Unit of time.
    /// </summary>
    public class MidiTime
    {
        #region Constants
        /// <summary>Only 4/4 time supported.</summary>
        public const int BEATS_PER_BAR = 4;

        /// <summary>Subdivision setting aka resolution. 4 means 1/16 notes, 8 means 1/32 notes.</summary>
        public const int SUBBEATS_PER_BEAT = 4;
        #endregion

        #region Properties
        /// <summary>From 0 to N.</summary>
        public int Bar { get; set; } = 0;

        /// <summary>From 0 to 3.</summary>
        public int Beat { get; set; } = 0;

        /// <summary>From 0 to SUBBEATS_PER_BEAT-1.</summary>
        public int SubBeat { get; set; } = 0;

        /// <summary>Total subBeats for the unit of time.</summary>
        public int TotalBeats { get { return Bar * BEATS_PER_BAR + Beat; } }

        /// <summary>Total subBeats for the unit of time.</summary>
        public int TotalSubBeats { get { return (Bar * BEATS_PER_BAR + Beat) * SUBBEATS_PER_BEAT + SubBeat; } }
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor.
        /// </summary>
        public MidiTime()
        {
            Bar = 0;
            Beat = 0;
            SubBeat = 0;
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        public MidiTime(MidiTime other)
        {
            Bar = other.Bar;
            Beat = other.Beat;
            SubBeat = other.SubBeat;
        }

        /// <summary>
        /// Constructor from discrete parts.
        /// </summary>
        /// <param name="beat"></param>
        /// <param name="subBeat">Sub to set - can be negative.</param>
        public MidiTime(int bar, int beat, int subBeat)
        {
            if (beat < 0)
            {
                //throw new Exception("Negative value is invalid");
                beat = 0;
            }

            if (subBeat >= 0)
            {
                Beat = beat + subBeat / SUBBEATS_PER_BEAT;
                SubBeat = subBeat % SUBBEATS_PER_BEAT;
            }
            else
            {
                subBeat = Math.Abs(subBeat);
                Beat = beat - (subBeat / SUBBEATS_PER_BEAT) - 1;
                SubBeat = SUBBEATS_PER_BEAT - (subBeat % SUBBEATS_PER_BEAT);
            }
        }

        /// <summary>
        /// Constructor from total subBeats.
        /// </summary>
        /// <param name="subBeats"></param>
        public MidiTime(int subBeats)
        {
            if (subBeats < 0)
            {
                throw new Exception("Negative value is invalid");
            }

            Bar = subBeats / BEATS_PER_BAR;
            Beat = subBeats % (BEATS_PER_BAR * SUBBEATS_PER_BEAT);
            SubBeat = subBeats % SUBBEATS_PER_BEAT;
        }

        ///// <summary>
        ///// Constructor from total subBeats.
        ///// </summary>
        ///// <param name="subBeats"></param>
        //public MidiTime(long subBeats) : this((int)subBeats)
        //{
        //}

        ///// <summary>
        ///// Constructor from Beat.SubBeat representation as a double.
        ///// </summary>
        ///// <param name="tts"></param>
        //public MidiTime(double tts)
        //{
        //    if (tts < 0)
        //    {
        //        throw new Exception("Negative value is invalid");
        //    }

        //    var (integral, fractional) = MathUtils.SplitDouble(tts);
        //    Beat = (int)integral;

        //    if (fractional >= SUBBEATS_PER_BEAT)
        //    {
        //        throw new Exception("Invalid subBeat value");
        //    }

        //    SubBeat = (int)(fractional * 100);
        //}
        #endregion



#if OVERRIDES
        // The GetHashCode() method should reflect the Equals logic; the rules are:
        // if two things are equal (Equals(...) == true) then they must return the same value for GetHashCode()
        // if the GetHashCode() is equal, it is not necessary for them to be the same; this is a collision, and Equals will 
        // be called to see if it is a real equality or not.


        #region Overrides and operators for custom classes

        // public override bool Equals(object obj)
        // {
        //     MidiTime fooItem = obj as MidiTime;
        //     if (fooItem == null) 
        //     {
        //        return false;
        //     }
        //     return fooItem.FooId == this.FooId;
        // }

        public override bool Equals(object other)
        {
            if (other is null) { return false; }
            if (ReferenceEquals(this, other)) { return true; }
            return other.GetType() == GetType() && Equals((MidiTime)other);
        }

        public bool Equals(MidiTime other)
        {
            // Equals() method compares only contents
            if (other is null) { return false; }
            if (ReferenceEquals(this, other)) { return true; }
            return Beat.Equals(other.Beat) && SubBeat.Equals(other.SubBeat);
        }

        public static bool operator ==(MidiTime obj1, MidiTime obj2)
        {
            // == Operator compares the reference identity
            if (ReferenceEquals(obj1, obj2)) { return true; }
            if (obj1 is null) { return false; }
            if (obj2 is null) { return false; }
            return (obj1.Bar == obj2.Bar && obj1.Beat == obj2.Beat && obj1.SubBeat == obj2.SubBeat);
        }

        public static bool operator !=(MidiTime obj1, MidiTime obj2)
        {
            return !(obj1 == obj2);
        }

        public static bool operator >(MidiTime t1, MidiTime t2)
        {
            return t1 is null || t2 is null || t1.TotalSubBeats > t2.TotalSubBeats;
        }

        public static bool operator >=(MidiTime t1, MidiTime t2)
        {
            return t1 is null || t2 is null || t1.TotalSubBeats >= t2.TotalSubBeats;
        }

        public static bool operator <(MidiTime t1, MidiTime t2)
        {
            return t1 is null || t2 is null || t1.TotalSubBeats < t2.TotalSubBeats;
        }

        public static bool operator <=(MidiTime t1, MidiTime t2)
        {
            return t1 is null || t2 is null || t1.TotalSubBeats <= t2.TotalSubBeats;
        }

        public static MidiTime operator +(MidiTime t1, MidiTime t2)
        {
            return new MidiTime(t1.TotalSubBeats + t2.TotalSubBeats);
        }

        public static MidiTime operator -(MidiTime t1, MidiTime t2)
        {
            return new MidiTime(t1.TotalSubBeats - t2.TotalSubBeats);
        }

        public override int GetHashCode()
        {
            return TotalSubBeats;
        }
        #endregion
#endif

        #region Public functions
        /// <summary>
        /// Move to the next subBeat and update clock.
        /// </summary>
        /// <returns>True if it's a new beat.</returns>
        public bool Advance()
        {
            bool newTick = false;
            SubBeat++;

            if(SubBeat >= SUBBEATS_PER_BEAT)
            {
                Beat++;
                SubBeat = 0;
                newTick = true;
            }

            return newTick;
        }

        /// <summary>
        /// Go back jack.
        /// </summary>
        public void Reset()
        {
            Beat = 0;
            SubBeat = 0;
        }

        /// <summary>
        /// Round up to next beat.
        /// </summary>
        public void RoundUp()
        {
            if(SubBeat != 0)
            {
                Beat++;
                SubBeat = 0;
            }
        }

        /// <summary>
        /// For viewing pleasure.
        /// </summary>
        public override string ToString()
        {
            return $"{Bar:00}.{Beat:00}.{SubBeat:00}";
        }
        #endregion
    }
}
