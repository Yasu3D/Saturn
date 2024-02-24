using System;
using System.Linq;
using UnityEngine;

namespace SaturnGame.RhythmGame
{
    [System.Serializable]
    public class SwipeNote : Note
    {
        public SwipeDirection Direction;

        public static SwipeNote CreateSwipe(
            int measure,
            int tick,
            int position,
            int size,
            SwipeDirection direction,
            NoteBonusType bonusType = NoteBonusType.None,
            bool isSync = false)
        {
            if (size == 60)
            {
                return new FullCircleSwipeNote(measure, tick, direction, bonusType, isSync);
            }
            return new SwipeNote(measure, tick, position, size, direction, bonusType, isSync);
        }

        protected SwipeNote(
            int measure,
            int tick,
            int position,
            int size,
            SwipeDirection direction,
            NoteBonusType bonusType = NoteBonusType.None,
            bool isSync = false
            ) : base(measure, tick, position, size, bonusType, isSync)
        {
            if (size == 60 && GetType() == typeof(SwipeNote))
            {
                throw new Exception("Use CreateSwipe for full circle swipes");
            }
            Direction = direction;
        }

        private static HitWindow[] _hitWindows = {
            new HitWindow(-5 * _FRAMEMS, 5 * _FRAMEMS, RhythmGame.Judgement.Marvelous),
            new HitWindow(-8 * _FRAMEMS, 10 * _FRAMEMS, RhythmGame.Judgement.Great),
            new HitWindow(-10 * _FRAMEMS, 10 * _FRAMEMS, RhythmGame.Judgement.Good),
        };
        public override HitWindow[] HitWindows => _hitWindows;

        public enum SwipeDirection
        {
            Clockwise,
            Counterclockwise,
        }

        // The swipe algorithm depends on the concept of an "average offset".
        // Here, "offset" is the distance from the left side of the note to a segment within the note.
        // The average offset is the average of offset values for all the segments that are touched in a given touchState.
        // So, given a touchState, we look at all touched segments within the note, calculate their offsets, and average them.
        // Note that if multiple segments with the same anglePos but different depthPos are touched, they are all counted separately.
        // (This has the effect of giving more weight to anglePos that are touched at multiple depths.)
        // AverageTouchOffset should always be >= 0 and <= Size-1.
        protected double? AverageTouchOffset(TouchState touchState)
        {
            int anglePosSum = 0;
            int segmentCount = 0;
            for (int anglePosOffset = 0; anglePosOffset < Size; anglePosOffset++)
            {
                int anglePos = (anglePosOffset + Left) % 60;
                for (int depthPos = 0; depthPos < 4; depthPos++)
                {
                    if (touchState.IsPressed(anglePos, depthPos))
                    {
                        anglePosSum += anglePosOffset;
                        segmentCount++;
                    }
                }
            }
            return segmentCount > 0 ? anglePosSum / segmentCount : null;
        }

        // The startAverageOffset is the average offset at the first time the note is touched.
        // Warning: once the note is touched, no later average offset will be used for the comparison.
        // (This probably needs to be changed later, but needs some care.)
        public virtual bool HasStart => startAverageOffset != null;
        private double? startAverageOffset;
        public virtual void SetStart(TouchState touchState)
        {
            // If note is not touched, this will be null
            startAverageOffset = AverageTouchOffset(touchState);
        }

        // A note is swiped if difference between the current average offset and the start average offset is more than 1.9.
        // 1.9 is chosen based on testing with the original game. It should surely be less than 2.
        // If the absolute value of the difference is more than 30, the swipe is not counted, since this would be more than
        // half the circle around. It's more likely that this is some other kind of input.
        // (E.g. touching the left then right edge of a >30 size note.)
        public virtual bool Swiped(TouchState touchState)
        {
            if (startAverageOffset == null)
            {
                return false;
            }
            double? curAverageOffset = AverageTouchOffset(touchState);
            if (curAverageOffset == null)
            {
                return false;
            }
            double anglePosDiff = (double)curAverageOffset - (double)startAverageOffset;
            switch (Direction)
            {
                case SwipeDirection.Clockwise:
                    return anglePosDiff < -1.9 && anglePosDiff > -30;
                case SwipeDirection.Counterclockwise:
                    return anglePosDiff > 1.9 && anglePosDiff < 30;
                default:
                    throw new Exception($"Invalid SwipeDirection {Direction}");
            }
        }
        // The "difference of averages" approach is preferred because it should still work alright if the non-swiping hand is
        // statically holding within the note, regardless of the position of that hand.
        // Assuming the number of segments touched by each hand stays constant (kind of a big assumption I guess?)
        // then, this just reduces the difference by a factor of 2. But the algorithm still works.
        // Alternative algorithms such as "difference of min/max offset" can be broken in this case.

        // Full circle swipes have their own implementation of the swipe algorithm that permits swiping across the Position anglePos.
        private class FullCircleSwipeNote : SwipeNote
        {
            public FullCircleSwipeNote(
                int measure,
                int tick,
                SwipeDirection direction,
                NoteBonusType bonusType,
                bool isSync
                ) : base(measure, tick, 0, 60, direction, bonusType, isSync)
            {
                SwipeNote[] _virtualNotes = {
                    new SwipeNote(measure, tick, 0, 30, direction, bonusType, isSync),
                    new SwipeNote(measure, tick, 15, 30, direction, bonusType, isSync),
                    new SwipeNote(measure, tick, 30, 30, direction, bonusType, isSync),
                    new SwipeNote(measure, tick, 45, 30, direction, bonusType, isSync),
                };
                virtualNotes = _virtualNotes;
            }

            // Four "virtual" notes of size 30 cover each hemisphere - top, left, bottom, right.
            private SwipeNote[] virtualNotes;
            public override bool HasStart => virtualNotes.Any(note => note.HasStart);
            public override void SetStart(TouchState touchState)
            {
                foreach (var virtualNote in virtualNotes)
                {
                    if (!virtualNote.HasStart)
                    {
                        virtualNote.SetStart(touchState);
                    }
                }
            }

            public override bool Swiped(TouchState touchState)
            {
                return virtualNotes.Any(note => note.Swiped(touchState));
            }

            // I previously considered using two 60 size swipes at position 0 and 30, but this would fail in some plausible scenarios,
            // such as swiping with both hands, left 28 -> 32 and right 58 -> 2.
            // To break the 4-note approach, you would realistically have to have four hands, so it's probably fine.
            // This should be revisited if a better approach is discovered.
        }
    }

}
