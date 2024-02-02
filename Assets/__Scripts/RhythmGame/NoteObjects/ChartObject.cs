using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaturnGame.RhythmGame
{
    [System.Serializable]
    public abstract class ChartObject
    {
        public ChartObject(int measure, int tick)
        {
            Measure = measure;
            Tick = tick;
        }

        public ChartObject Clone() {
            return (ChartObject)MemberwiseClone();
        }

        public int Measure;
        [Range(0, 1919)] public int Tick;
        public float TimeMs;
        public float ScaledVisualTime;
    }

    public class ObjectEnums
    {
        public enum NoteType
        {
            None,
            Touch,
            SnapForward,
            SnapBackward,
            SwipeClockwise,
            SwipeCounterclockwise,
            HoldStart,
            HoldSegment,
            HoldEnd,
            Chain,
            MaskAdd,
            MaskRemove,
            EndChart
        }
        public enum BonusType
        {
            None,
            Bonus,
            R_Note
        }
        // TODO: move into Gimmick class
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
            StopEnd
        }
    }
}
