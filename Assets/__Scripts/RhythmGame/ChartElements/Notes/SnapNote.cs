using System;

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

        private static HitWindow[] _forwardHitWindows = {
            new HitWindow(-5 * _FRAMEMS, 7 * _FRAMEMS, RhythmGame.Judgement.Marvelous),
            new HitWindow(-8 * _FRAMEMS, 10 * _FRAMEMS, RhythmGame.Judgement.Great),
            new HitWindow(-10 * _FRAMEMS, 10 * _FRAMEMS, RhythmGame.Judgement.Good),
        };
        private static HitWindow[] _backwardHitWindows = {
            new HitWindow(-7 * _FRAMEMS, 5 * _FRAMEMS, RhythmGame.Judgement.Marvelous),
            new HitWindow(-10 * _FRAMEMS, 8 * _FRAMEMS, RhythmGame.Judgement.Great),
            new HitWindow(-10 * _FRAMEMS, 10 * _FRAMEMS, RhythmGame.Judgement.Good),
        };
        public override HitWindow[] HitWindows => Direction switch
        {
            SnapDirection.Forward => _forwardHitWindows,
            SnapDirection.Backward => _backwardHitWindows,
            _ => throw new ArgumentOutOfRangeException(nameof(Direction))
        };

        public enum SnapDirection
        {
            Forward,
            Backward,
        }
    }
}
