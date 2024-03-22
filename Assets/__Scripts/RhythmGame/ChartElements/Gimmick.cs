using System;
using System.Globalization;
using JetBrains.Annotations;

namespace SaturnGame.RhythmGame
{
    // TODO: create separate classes for HiSpeed, TimeSignature, and BeatsPerMinute as well
    [Serializable]
    public class Gimmick : ChartElement
    {
        public Gimmick(int measure, int tick, GimmickType gimmickType, [CanBeNull] object value1 = null,
            [CanBeNull] object value2 = null) : base(measure, tick)
        {
            Type = gimmickType;

            switch (Type)
            {
                case GimmickType.BeatsPerMinute:
                {
                    BeatsPerMinute = Convert.ToSingle(value1, CultureInfo.InvariantCulture);
                    break;
                }

                case GimmickType.TimeSignature:
                {
                    TimeSig = new TimeSignature(Convert.ToInt32(value1, CultureInfo.InvariantCulture),
                        Convert.ToInt32(value2, CultureInfo.InvariantCulture));
                    break;
                }

                case GimmickType.HiSpeed:
                {
                    HiSpeed = Convert.ToSingle(value1, CultureInfo.InvariantCulture);
                    break;
                }
            }
        }

        public Gimmick(int measure, int tick, int gimmickID, [CanBeNull] object value1 = null,
            [CanBeNull] object value2 = null) : base(measure, tick)
        {
            // assign gimmickType and values
            switch (gimmickID)
            {
                case 2:
                {
                    Type = GimmickType.BeatsPerMinute;
                    BeatsPerMinute = Convert.ToSingle(value1, CultureInfo.InvariantCulture);
                    break;
                }

                case 3:
                {
                    Type = GimmickType.TimeSignature;
                    TimeSig = new TimeSignature(Convert.ToInt32(value1, CultureInfo.InvariantCulture),
                        Convert.ToInt32(value2, CultureInfo.InvariantCulture));
                    break;
                }

                case 5:
                {
                    Type = GimmickType.HiSpeed;
                    HiSpeed = Convert.ToSingle(value1, CultureInfo.InvariantCulture);
                    break;
                }

                case 6:
                {
                    Type = GimmickType.ReverseEffectStart;
                    break;
                }

                case 7:
                {
                    Type = GimmickType.ReverseEffectEnd;
                    break;
                }

                case 8:
                {
                    Type = GimmickType.ReverseNoteEnd;
                    break;
                }

                case 9:
                {
                    Type = GimmickType.StopStart;
                    break;
                }

                case 10:
                {
                    Type = GimmickType.StopEnd;
                    break;
                }

                default:
                {
                    Type = GimmickType.None;
                    break;
                }
            }
        }

        public Gimmick (int measure, int tick, float bpm, TimeSignature timeSig) : base(measure, tick)
        {
            BeatsPerMinute = bpm;
            TimeSig = timeSig;
        }

        public GimmickType Type;
        public float BeatsPerMinute;
        public TimeSignature TimeSig;
        public float HiSpeed;

        public enum GimmickType
        {
            None,
            BeatsPerMinute,
            TimeSignature,
            HiSpeed,
            ReverseEffectStart,
            ReverseEffectEnd,
            ReverseNoteEnd,
            StopStart,
            StopEnd,
        }
    }
}
