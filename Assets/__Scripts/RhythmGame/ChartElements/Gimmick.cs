using System;

namespace SaturnGame.RhythmGame
{
    // TODO: create separate classes for HiSpeed, TimeSignature, and BeatsPerMinute as well
    [System.Serializable]
    public class Gimmick : ChartElement
    {
        public Gimmick (int measure, int tick, GimmickType gimmickType, object value1 = null, object value2 = null) : base(measure, tick)
        {
            Measure = measure;
            Tick = tick;
            Type = gimmickType;

            switch (Type)
            {
                default:
                {
                    break;
                }

                case GimmickType.BeatsPerMinute:
                {
                    BeatsPerMinute = Convert.ToSingle(value1);
                    break;
                }

                case GimmickType.TimeSignature:
                {
                    TimeSig = new TimeSignature(Convert.ToInt32(value1), Convert.ToInt32(value2));
                    break;
                }

                case GimmickType.HiSpeed:
                {
                    HiSpeed = Convert.ToSingle(value1);
                    break;
                }
            }
        }

        public Gimmick (int measure, int tick, int gimmickID, object value1 = null, object value2 = null) : base(measure, tick)
        {
            Measure = measure;
            Tick = tick;

            // assign gimmickType and values
            switch (gimmickID)
            {
                case 2:
                {
                    Type = GimmickType.BeatsPerMinute;
                    BeatsPerMinute = Convert.ToSingle(value1);
                    break;
                }

                case 3:
                {
                    Type = GimmickType.TimeSignature;
                    TimeSig = new TimeSignature(Convert.ToInt32(value1), Convert.ToInt32(value2));
                    break;
                }

                case 5:
                {
                    Type = GimmickType.HiSpeed;
                    HiSpeed = Convert.ToSingle(value1);
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
            Measure = measure;
            Tick = tick;
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
            Note,
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
