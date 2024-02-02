using System;
using UnityEngine;

namespace SaturnGame.RhythmGame
{
    [System.Serializable]
    public class SwipeNote : SimpleNote
    {
        public SwipeDirection Direction;

        public SwipeNote(
            int measure,
            int tick,
            int position,
            int size,
            SwipeDirection direction,
            ObjectEnums.BonusType bonusType = ObjectEnums.BonusType.None,
            bool isSync = false
            ) : base(measure, tick, bonusType, position, size, isSync)
        {
            Direction = direction;
        }

        public override ObjectEnums.NoteType NoteType
        {
            get
            {
                switch (Direction)
                {
                    case SwipeDirection.Clockwise:
                        return ObjectEnums.NoteType.SwipeClockwise;
                    case SwipeDirection.Counterclockwise:
                        return ObjectEnums.NoteType.SwipeCounterclockwise;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(Direction), $"Unexpected SwipeDirection value {Direction}");
                }
            }
        }

        public enum SwipeDirection
        {
            Clockwise,
            Counterclockwise,
        }
    }
}
