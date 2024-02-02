using System;
using UnityEngine;

namespace SaturnGame.RhythmGame
{
    [System.Serializable]
    public class SnapNote : Note
    {
        public SnapDirection Direction;

        public SnapNote(
            int measure,
            int tick,
            int position,
            int size,
            SnapDirection direction,
            NoteBonusType bonusType = NoteBonusType.None,
            bool isSync = false
            ) : base(measure, tick, position, size, bonusType, isSync)
        {
            Direction = direction;
        }

        public enum SnapDirection
        {
            Forward,
            Backward,
        }
    }
}
