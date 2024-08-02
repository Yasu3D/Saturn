using System;
using System.Linq;
using JetBrains.Annotations;

namespace SaturnGame.RhythmGame
{
[Serializable]
public class SwipeNote : Note
{
    public SwipeDirection Direction;

    [NotNull]
    public static SwipeNote CreateSwipe(
        int measure,
        int tick,
        int position,
        int size,
        int id,
        SwipeDirection direction,
        NoteBonusType bonusType = NoteBonusType.None,
        bool isSync = false)
    {
        return size == 60
            ? new FullCircleSwipeNote(measure, tick, id, direction, bonusType, isSync)
            : new SwipeNote(measure, tick, position, size, id, direction, bonusType, isSync);
    }

    private SwipeNote(
        int measure,
        int tick,
        int position,
        int size,
        int id,
        SwipeDirection direction,
        NoteBonusType bonusType = NoteBonusType.None,
        bool isSync = false
    ) : base(measure, tick, position, size, id, bonusType, isSync)
    {
        if (size == 60 && GetType() == typeof(SwipeNote))
            throw new Exception("Use CreateSwipe for full circle swipes");
        Direction = direction;
    }

    private static HitWindow[] hitWindows =
    {
        new(-5 * FrameMs, 5 * FrameMs, RhythmGame.Judgement.Marvelous),
        new(-8 * FrameMs, 10 * FrameMs, RhythmGame.Judgement.Great),
        new(-10 * FrameMs, 10 * FrameMs, RhythmGame.Judgement.Good),
    };

    public override HitWindow[] HitWindows => hitWindows;

    public enum SwipeDirection
    {
        Clockwise,
        Counterclockwise,
    }

    // The swipe algorithm depends on the concept of an "average offset".
    // Here, "offset" is the distance from the start of the swipe to a segment within the swipe.
    // Note that offset increases in the direction of the swipe.
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
            int anglePos = Direction switch
            {
                SwipeDirection.Clockwise => SaturnMath.Modulo(Right - anglePosOffset, 60),
                SwipeDirection.Counterclockwise => SaturnMath.Modulo(Left + anglePosOffset, 60),
                _ => throw new ArgumentOutOfRangeException(nameof(Direction)),
            };
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

    // The minAverageOffset is the minimum average offset that we have seen on any frame within the swipe.
    private double? minAverageOffset;

    public virtual void MaybeUpdateMinAverageOffset(TouchState touchState)
    {
        // If note is not touched, this will be null
        if (minAverageOffset is null || AverageTouchOffset(touchState) < minAverageOffset.Value)
            minAverageOffset = AverageTouchOffset(touchState);
    }

    // A note is swiped if difference between the current average offset and the min average offset is more than 1.9.
    // 1.9 is chosen based on testing with the original game. It should surely be less than 2.
    // If the  difference is more than 30, the swipe is not counted, since this would be more than
    // half the circle around. It's more likely that this is some other kind of input.
    // (E.g. touching the left then right edge of a >30 size note.)
    public virtual bool Swiped(TouchState touchState)
    {
        if (minAverageOffset == null) return false;
        double? curAverageOffset = AverageTouchOffset(touchState);
        if (curAverageOffset == null) return false;
        double anglePosDiff = curAverageOffset.Value - minAverageOffset.Value;
        return anglePosDiff is > 1.9 and < 30;
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
            int id,
            SwipeDirection direction,
            NoteBonusType bonusType,
            bool isSync
        ) : base(measure, tick, 0, 60, id, direction, bonusType, isSync)
        {
            virtualNotes = new SwipeNote[]
            {
                new(measure, tick, 0, 30, -1, direction, bonusType, isSync),
                new(measure, tick, 15, 30, -1, direction, bonusType, isSync),
                new(measure, tick, 30, 30, -1, direction, bonusType, isSync),
                new(measure, tick, 45, 30, -1, direction, bonusType, isSync),
            };
        }

        // Four "virtual" notes of size 30 cover each hemisphere - top, left, bottom, right.
        private readonly SwipeNote[] virtualNotes;

        public override void MaybeUpdateMinAverageOffset(TouchState touchState)
        {
            foreach (SwipeNote virtualNote in virtualNotes)
                virtualNote.MaybeUpdateMinAverageOffset(touchState);
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
