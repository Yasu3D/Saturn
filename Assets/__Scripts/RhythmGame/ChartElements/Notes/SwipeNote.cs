using System;
using UnityEngine;

namespace SaturnGame.RhythmGame
{
    [System.Serializable]
    public class SwipeNote : Note
    {
        public SwipeDirection Direction;

        public SwipeNote(
            int measure,
            int tick,
            int position,
            int size,
            SwipeDirection direction,
            NoteBonusType bonusType = NoteBonusType.None,
            bool isSync = false
            ) : base(measure, tick, position, size, bonusType, isSync)
        {
            Direction = direction;
        }

        // TODO: fix
        public override HitWindow[] HitWindows => baseHitWindows;

        public enum SwipeDirection
        {
            Clockwise,
            Counterclockwise,
        }
    }
}
