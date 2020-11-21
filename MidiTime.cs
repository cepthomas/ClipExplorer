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
        /// <summary>Subdivision setting. 4 means 1/16 notes, 8 means 1/32 notes.</summary>
        public const int SUBBEATS_PER_BEAT  = 4;
        #endregion

        #region Properties
        /// <summary>From 0 to N.</summary>
        public int Beat { get; set; } = 0;

        /// <summary>From 0 to SUBBEATS_PER_BEAT-1.</summary>
        public int SubBeat { get; set; } = 0;

        /// <summary>Total subBeats for the unit of time.</summary>
        public int TotalSubBeats { get { return Beat * SUBBEATS_PER_BEAT + SubBeat; } }
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor.
        /// </summary>
        public MidiTime()
        {
            Beat = 0;
            SubBeat = 0;
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        public MidiTime(MidiTime other)
        {
            Beat = other.Beat;
            SubBeat = other.SubBeat;
        }

        /// <summary>
        /// Constructor from discrete parts.
        /// </summary>
        /// <param name="beat"></param>
        /// <param name="subBeat">Sub to set - can be negative.</param>
        public MidiTime(int beat, int subBeat)
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

            Beat = subBeats / SUBBEATS_PER_BEAT;
            SubBeat = subBeats % SUBBEATS_PER_BEAT;
        }

        /// <summary>
        /// Constructor from total subBeats.
        /// </summary>
        /// <param name="subBeats"></param>
        public MidiTime(long subBeats) : this((int)subBeats)
        {
        }

        /// <summary>
        /// Constructor from Beat.SubBeat representation as a double.
        /// </summary>
        /// <param name="tts"></param>
        public MidiTime(double tts)
        {
            if (tts < 0)
            {
                throw new Exception("Negative value is invalid");
            }

            var (integral, fractional) = MathUtils.SplitDouble(tts);
            Beat = (int)integral;

            if (fractional >= SUBBEATS_PER_BEAT)
            {
                throw new Exception("Invalid subBeat value");
            }

            SubBeat = (int)(fractional * 100);
        }
        #endregion

        #region Overrides and operators for custom classes
        // The Equality Operator (==) is the comparison operator and the Equals() method compares the contents of a string.
        // The == Operator compares the reference identity while the Equals() method compares only contents.

        public override bool Equals(object other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return other.GetType() == GetType() && Equals((MidiTime)other);
        }

        public bool Equals(MidiTime other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Beat.Equals(other.Beat) && SubBeat.Equals(other.SubBeat);
        }

        public static bool operator ==(MidiTime obj1, MidiTime obj2)
        {
            if (ReferenceEquals(obj1, obj2))
            {
                return true;
            }

            if (obj1 is null)
            {
                return false;
            }

            if (obj2 is null)
            {
                return false;
            }

            return (obj1.Beat == obj2.Beat && obj1.SubBeat == obj2.SubBeat);
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
            int beat = t1.Beat + t2.Beat + (t1.SubBeat + t2.SubBeat) / SUBBEATS_PER_BEAT;
            int incr = (t1.SubBeat + t2.SubBeat) % SUBBEATS_PER_BEAT;
            return new MidiTime(beat, incr);
        }

        public override int GetHashCode()
        {
            return TotalSubBeats;
        }
        #endregion

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
            return $"{Beat:00}.{SubBeat:00}";
        }
        #endregion
    }
}
