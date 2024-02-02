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
            ObjectEnums.BonusType bonusType = ObjectEnums.BonusType.None,
            bool isSync = false
            ) : base(measure, tick, position, size, bonusType, isSync)
        {
            Direction = direction;
        }

        public override ObjectEnums.NoteType NoteType
        {
            get
            {
                switch (Direction)
                {
                    case SnapDirection.Forward:
                        return ObjectEnums.NoteType.SnapForward;
                    case SnapDirection.Backward:
                        return ObjectEnums.NoteType.SnapBackward;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(Direction), $"Unexpected SnapDirection value {Direction}");
                }
            }
        }

        public enum SnapDirection
        {
            Forward,
            Backward,
        }
    }
}
