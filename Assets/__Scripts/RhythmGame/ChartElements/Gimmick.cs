using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaturnGame.RhythmGame
{
    [System.Serializable]
    public class Gimmick : TimedChartElement
    {
        public Gimmick (int measure, int tick, ObjectEnums.GimmickType gimmickType, object value1 = null, object value2 = null) : base(measure, tick)
        {
            Measure = measure;
            Tick = tick;
            GimmickType = gimmickType;

            switch (GimmickType)
            {
                default:
                    break;

                case ObjectEnums.GimmickType.BeatsPerMinute:
                    BeatsPerMinute = Convert.ToSingle(value1);
                    break;

                case ObjectEnums.GimmickType.TimeSignature:
                    TimeSig = new TimeSignature(Convert.ToInt32(value1), Convert.ToInt32(value2));
                    break;

                case ObjectEnums.GimmickType.HiSpeed:
                    HiSpeed = Convert.ToSingle(value1);
                    break;
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
                    GimmickType = ObjectEnums.GimmickType.BeatsPerMinute;
                    BeatsPerMinute = Convert.ToSingle(value1);
                    break;

                case 3:
                    GimmickType = ObjectEnums.GimmickType.TimeSignature;
                    TimeSig = new TimeSignature(Convert.ToInt32(value1), Convert.ToInt32(value2));
                    break;

                case 5:
                    GimmickType = ObjectEnums.GimmickType.HiSpeed;
                    HiSpeed = Convert.ToSingle(value1);
                    break;

                case 6:
                    GimmickType = ObjectEnums.GimmickType.ReverseEffectStart;
                    break;

                case 7:
                    GimmickType = ObjectEnums.GimmickType.ReverseEffectEnd;
                    break;

                case 8:
                    GimmickType = ObjectEnums.GimmickType.ReverseNoteEnd;
                    break;

                case 9:
                    GimmickType = ObjectEnums.GimmickType.StopStart;
                    break;

                case 10:
                    GimmickType = ObjectEnums.GimmickType.StopEnd;
                    break;

                default:
                    GimmickType = ObjectEnums.GimmickType.None;
                    break;
            }
        }

        public Gimmick (int measure, int tick, float bpm, TimeSignature timeSig) : base(measure, tick)
        {
            Measure = measure;
            Tick = tick;
            BeatsPerMinute = bpm;
            TimeSig = timeSig;
        }

        public ObjectEnums.GimmickType GimmickType;
        public float BeatsPerMinute;
        public TimeSignature TimeSig;
        public float HiSpeed;
    }
}
