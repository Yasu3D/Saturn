using System;
using System.Linq;

namespace SaturnGame.RhythmGame
{
    [Serializable]
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

        private static HitWindow[] forwardHitWindows = {
            new(-5 * FrameMS, 7 * FrameMS, RhythmGame.Judgement.Marvelous),
            new(-8 * FrameMS, 10 * FrameMS, RhythmGame.Judgement.Great),
            new(-10 * FrameMS, 10 * FrameMS, RhythmGame.Judgement.Good),
        };
        private static HitWindow[] backwardHitWindows = {
            new(-7 * FrameMS, 5 * FrameMS, RhythmGame.Judgement.Marvelous),
            new(-10 * FrameMS, 8 * FrameMS, RhythmGame.Judgement.Great),
            new(-10 * FrameMS, 10 * FrameMS, RhythmGame.Judgement.Good),
        };
        public override HitWindow[] HitWindows => Direction switch
        {
            SnapDirection.Forward => forwardHitWindows,
            SnapDirection.Backward => backwardHitWindows,
            _ => throw new ArgumentOutOfRangeException(nameof(Direction)),
        };

        public enum SnapDirection
        {
            Forward,
            Backward,
        }

        private bool CheckDepthChangeInRange(TouchState prevTouchState, TouchState curTouchState, int rangeLeft, int rangeSize)
        {
            int? prevMin = null;
            int? curMin = null;
            int? prevMax = null;
            int? curMax = null;
            foreach (int offset in Enumerable.Range(0, rangeSize))
            {
                int anglePos = SaturnMath.Modulo(rangeLeft + offset, 60);
                foreach (int depthPos in Enumerable.Range(0, 4))
                {
                    if (prevTouchState.IsPressed(anglePos, depthPos))
                    {
                        if (prevMin is null || depthPos < prevMin) prevMin = depthPos;

                        if (prevMax is null || depthPos > prevMax) prevMax = depthPos;
                    }

                    if (curTouchState.IsPressed(anglePos, depthPos))
                    {
                        if (curMin is null || depthPos < curMin) curMin = depthPos;

                        if (curMax is null || depthPos > curMax) curMax = depthPos;
                    }
                }
            }

            switch (Direction)
            {
                case SnapDirection.Forward:
                {
                    if (curMin is not null && prevMin is not null && curMin > prevMin) return true;

                    if (curMax is not null && prevMax is not null && curMax > prevMax) return true;

                    return false;
                }
                case SnapDirection.Backward:
                {
                    if (curMin is not null && prevMin is not null && curMin < prevMin) return true;

                    if (curMax is not null && prevMax is not null && curMax < prevMax) return true;

                    return false;
                }
                default:
                {
                    throw new Exception($"Unknown enum value {Direction}");
                }
            }
        }
        public bool Snapped(TouchState prevTouchState, TouchState curTouchState)
        {
            // Check if we have moved up/down on any specific anglePos.
            bool hitSpecific = Enumerable.Range(0, Size)
                .Select(offset => SaturnMath.Modulo(Left + offset, 60))
                .Any(anglePos => CheckDepthChangeInRange(prevTouchState, curTouchState, anglePos, 1));

            // Check if we have moved up/down on any range of two adjacent anglePos
            return hitSpecific || Enumerable.Range(0, Size - 1)
                .Select(offset => SaturnMath.Modulo(Left + offset, 60))
                .Any(rangeLeft => CheckDepthChangeInRange(prevTouchState, curTouchState, rangeLeft, 2));
        }
    }
}
