using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaturnGame.RhythmGame
{
    [System.Serializable]
    public class Gimmick : ChartObject
    {
        public Gimmick (int measure, int tick, ObjectEnums.GimmickType gimmickType, object value1 = null, object value2 = null)
        {
            Measure = measure;
            Tick = tick;
            GimmickType = gimmickType;

            switch (GimmickType)
            {
                default:
                    break;

                case ObjectEnums.GimmickType.BeatsPerMinute:
                    BeatsPerMinute = (float) value1;
                    break;

                case ObjectEnums.GimmickType.TimeSignature:
                    TimeSig = new TimeSignature((int) value1, (int) value2);
                    break;

                case ObjectEnums.GimmickType.HiSpeed:
                    HiSpeed = (float) value1;
                    break;
            }
        }

        public Gimmick (int measure, int tick, int gimmickID, object value1 = null, object value2 = null)
        {
            Measure = measure;
            Tick = tick;

            // assign gimmickType and values
            switch (gimmickID)
            {
                case 2:
                    GimmickType = ObjectEnums.GimmickType.BeatsPerMinute;
                    BeatsPerMinute = (float) value1;
                    break;

                case 3:
                    GimmickType = ObjectEnums.GimmickType.TimeSignature;
                    TimeSig = new TimeSignature((int) value1, (int) value2);
                    break;

                case 5:
                    GimmickType = ObjectEnums.GimmickType.HiSpeed;
                    HiSpeed = (float) value1;
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

        public Gimmick (int measure, int tick, float bpm, TimeSignature timeSig)
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
