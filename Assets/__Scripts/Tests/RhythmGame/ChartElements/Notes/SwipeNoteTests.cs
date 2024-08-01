using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using SaturnGame;
using SaturnGame.RhythmGame;

public class SwipeNoteTests
{
    [Test]
    public void NormalSwipeCounts()
    {
        SwipeNote note = SwipeNote.CreateSwipe(0, 0, 0, 30, 1, SwipeNote.SwipeDirection.Counterclockwise);

        bool[,] firstSegments = new bool[60, 4];
        // 3x2 segments from (2, 1) to (4, 2) inclusive
        foreach (int anglePos in Enumerable.Range(2, 3))
        foreach (int depthPos in Enumerable.Range(1, 2))
            firstSegments[anglePos, depthPos] = true;
        TouchState firstTouch = new(firstSegments);
        // In ScoringManager, MaybeUpdateMinAverageOffset is called before Swiped, but the order _shouldn't_ matter.
        Assert.IsFalse(note.Swiped(firstTouch));
        note.MaybeUpdateMinAverageOffset(firstTouch);
        Assert.IsFalse(note.Swiped(firstTouch));

        bool[,] secondSegments = new bool[60, 4];
        // shift the range 5 counterclockwise, so now:
        // 3x2 segments from (7, 1) to (9, 2) inclusive
        foreach (int anglePos in Enumerable.Range(7, 3))
        foreach (int depthPos in Enumerable.Range(1, 2))
            secondSegments[anglePos, depthPos] = true;
        TouchState secondTouch = new(secondSegments);
        // In ScoringManager, MaybeUpdateMinAverageOffset is called before Swiped, but the order _shouldn't_ matter.
        Assert.IsTrue(note.Swiped(secondTouch));
        note.MaybeUpdateMinAverageOffset(secondTouch);
        Assert.IsTrue(note.Swiped(secondTouch));
    }

    [Test]
    public void SmallSwipeDoesntCount()
    {
        SwipeNote note = SwipeNote.CreateSwipe(0, 0, 0, 30, 1, SwipeNote.SwipeDirection.Counterclockwise);

        bool[,] firstSegments = new bool[60, 4];
        // 3x2 segments from (2, 1) to (4, 2) inclusive
        foreach (int anglePos in Enumerable.Range(2, 3))
        foreach (int depthPos in Enumerable.Range(1, 2))
            firstSegments[anglePos, depthPos] = true;
        TouchState firstTouch = new(firstSegments);
        // In ScoringManager, MaybeUpdateMinAverageOffset is called before Swiped, but the order _shouldn't_ matter.
        Assert.IsFalse(note.Swiped(firstTouch));
        note.MaybeUpdateMinAverageOffset(firstTouch);
        Assert.IsFalse(note.Swiped(firstTouch));

        bool[,] secondSegments = new bool[60, 4];
        // shift the range counterclockwise by only 1, so now:
        // 3x2 segments from (3, 1) to (5, 2) inclusive
        foreach (int anglePos in Enumerable.Range(3, 3))
        foreach (int depthPos in Enumerable.Range(1, 2))
            secondSegments[anglePos, depthPos] = true;
        TouchState secondTouch = new(secondSegments);
        // In ScoringManager, MaybeUpdateMinAverageOffset is called before Swiped, but the order _shouldn't_ matter.
        Assert.IsFalse(note.Swiped(secondTouch));
        note.MaybeUpdateMinAverageOffset(secondTouch);
        Assert.IsFalse(note.Swiped(secondTouch));
    }

    [Test]
    public void WrongDirectionSwipeDoesntCount()
    {
        // This is the same as NormalSwipeCounts, but the swipe direction is CW instead of CCW.
        SwipeNote note = SwipeNote.CreateSwipe(0, 0, 0, 30, 1, SwipeNote.SwipeDirection.Clockwise);

        bool[,] firstSegments = new bool[60, 4];
        // 3x2 segments from (2, 1) to (4, 2) inclusive
        foreach (int anglePos in Enumerable.Range(2, 3))
        foreach (int depthPos in Enumerable.Range(1, 2))
            firstSegments[anglePos, depthPos] = true;
        TouchState firstTouch = new(firstSegments);
        // In ScoringManager, MaybeUpdateMinAverageOffset is called before Swiped, but the order _shouldn't_ matter.
        Assert.IsFalse(note.Swiped(firstTouch));
        note.MaybeUpdateMinAverageOffset(firstTouch);
        Assert.IsFalse(note.Swiped(firstTouch));

        bool[,] secondSegments = new bool[60, 4];
        // shift the range 5 counterclockwise, so now:
        // 3x2 segments from (7, 1) to (9, 2) inclusive
        foreach (int anglePos in Enumerable.Range(7, 3))
        foreach (int depthPos in Enumerable.Range(1, 2))
            secondSegments[anglePos, depthPos] = true;
        TouchState secondTouch = new(secondSegments);
        // In ScoringManager, MaybeUpdateMinAverageOffset is called before Swiped, but the order _shouldn't_ matter.
        Assert.IsFalse(note.Swiped(secondTouch));
        note.MaybeUpdateMinAverageOffset(secondTouch);
        Assert.IsFalse(note.Swiped(secondTouch));
    }

    [Test]
    public void BackAndForthSwipeCounts()
    {
        SwipeNote noteCCW = SwipeNote.CreateSwipe(0, 0, 0, 30, 1, SwipeNote.SwipeDirection.Counterclockwise);
        SwipeNote noteCW =  SwipeNote.CreateSwipe(0, 0, 0, 30, 2, SwipeNote.SwipeDirection.Clockwise);

        bool[,] firstSegments = new bool[60, 4];
        // 3x2 segments from (7, 1) to (9, 2) inclusive
        foreach (int anglePos in Enumerable.Range(7, 3))
        foreach (int depthPos in Enumerable.Range(1, 2))
            firstSegments[anglePos, depthPos] = true;
        TouchState firstTouch = new(firstSegments);
        // In ScoringManager, MaybeUpdateMinAverageOffset is called before Swiped, but the order _shouldn't_ matter.
        Assert.IsFalse(noteCCW.Swiped(firstTouch));
        noteCCW.MaybeUpdateMinAverageOffset(firstTouch);
        Assert.IsFalse(noteCCW.Swiped(firstTouch));
        Assert.IsFalse(noteCW.Swiped(firstTouch));
        noteCW.MaybeUpdateMinAverageOffset(firstTouch);
        Assert.IsFalse(noteCW.Swiped(firstTouch));

        bool[,] secondSegments = new bool[60, 4];
        // shift the range 5 clockwise, so now:
        // 3x2 segments from (2, 1) to (4, 2) inclusive
        foreach (int anglePos in Enumerable.Range(2, 3))
        foreach (int depthPos in Enumerable.Range(1, 2))
            secondSegments[anglePos, depthPos] = true;
        TouchState secondTouch = new(secondSegments);
        // In ScoringManager, MaybeUpdateMinAverageOffset is called before Swiped, but the order _shouldn't_ matter.
        Assert.IsFalse(noteCCW.Swiped(secondTouch));
        noteCCW.MaybeUpdateMinAverageOffset(secondTouch);
        Assert.IsFalse(noteCCW.Swiped(secondTouch));
        Assert.IsTrue(noteCW.Swiped(secondTouch));
        noteCW.MaybeUpdateMinAverageOffset(secondTouch);
        Assert.IsTrue(noteCW.Swiped(secondTouch));

        bool[,] thirdSegments = new bool[60, 4];
        // shift the range back 5 counterclockwise, so now:
        // 3x2 segments from (7, 1) to (9, 2) inclusive
        // Same as first.
        foreach (int anglePos in Enumerable.Range(7, 3))
        foreach (int depthPos in Enumerable.Range(1, 2))
            thirdSegments[anglePos, depthPos] = true;
        TouchState thirdTouch = new(thirdSegments);
        // In ScoringManager, MaybeUpdateMinAverageOffset is called before Swiped, but the order _shouldn't_ matter.
        Assert.IsTrue(noteCCW.Swiped(thirdTouch));
        noteCCW.MaybeUpdateMinAverageOffset(thirdTouch);
        Assert.IsTrue(noteCCW.Swiped(thirdTouch));
    }

    [Test]
    public void SwipeAcrossAnglePosZeroCounts()
    {
        // Same as NormalSwipeCounts but the swipe goes across anglePos 0.
        SwipeNote note = SwipeNote.CreateSwipe(0, 0, 45, 30, 1, SwipeNote.SwipeDirection.Counterclockwise);

        bool[,] firstSegments = new bool[60, 4];
        // 3x2 segments from (56, 1) to (58, 2) inclusive
        foreach (int anglePos in Enumerable.Range(56, 3))
        foreach (int depthPos in Enumerable.Range(1, 2))
            firstSegments[anglePos, depthPos] = true;
        TouchState firstTouch = new(firstSegments);
        // In ScoringManager, MaybeUpdateMinAverageOffset is called before Swiped, but the order _shouldn't_ matter.
        Assert.IsFalse(note.Swiped(firstTouch));
        note.MaybeUpdateMinAverageOffset(firstTouch);
        Assert.IsFalse(note.Swiped(firstTouch));

        bool[,] secondSegments = new bool[60, 4];
        // shift the range 5 counterclockwise, so now:
        // 3x2 segments from (1, 1) to (3, 2) inclusive
        foreach (int anglePos in Enumerable.Range(1, 3))
        foreach (int depthPos in Enumerable.Range(1, 2))
            secondSegments[anglePos, depthPos] = true;
        TouchState secondTouch = new(secondSegments);
        // In ScoringManager, MaybeUpdateMinAverageOffset is called before Swiped, but the order _shouldn't_ matter.
        Assert.IsTrue(note.Swiped(secondTouch));
        note.MaybeUpdateMinAverageOffset(secondTouch);
        Assert.IsTrue(note.Swiped(secondTouch));
    }

    private readonly Type fullCircleSwipeNote =
        typeof(SwipeNote).GetNestedType("FullCircleSwipeNote", BindingFlags.NonPublic);
    [Test]
    public void CreatedFullCircleSwipeNoteHasCorrectType()
    {
        SwipeNote normalNote = SwipeNote.CreateSwipe(0, 0, 0, 59, 1, SwipeNote.SwipeDirection.Clockwise);
        Assert.AreNotEqual(fullCircleSwipeNote, normalNote.GetType());
        SwipeNote fullCircleNote = SwipeNote.CreateSwipe(0, 0, 0, 60, 2, SwipeNote.SwipeDirection.Clockwise);
        Assert.AreEqual(fullCircleSwipeNote, fullCircleNote.GetType());
    }

    public class FullCircleSwipeNoteTests
    {
        [Test]
        public void FullCircleSwipeCounts()
        {
            SwipeNote note = SwipeNote.CreateSwipe(0, 0, 0, 60, 1, SwipeNote.SwipeDirection.Counterclockwise);

            bool[,] firstSegments = new bool[60, 4];
            // 3x2 segments from (2, 1) to (4, 2) inclusive
            foreach (int anglePos in Enumerable.Range(2, 3))
            foreach (int depthPos in Enumerable.Range(1, 2))
                firstSegments[anglePos, depthPos] = true;
            TouchState firstTouch = new(firstSegments);
            // In ScoringManager, MaybeUpdateMinAverageOffset is called before Swiped, but the order _shouldn't_ matter.
            Assert.IsFalse(note.Swiped(firstTouch));
            note.MaybeUpdateMinAverageOffset(firstTouch);
            Assert.IsFalse(note.Swiped(firstTouch));

            bool[,] secondSegments = new bool[60, 4];
            // shift the range 5 counterclockwise, so now:
            // 3x2 segments from (7, 1) to (9, 2) inclusive
            foreach (int anglePos in Enumerable.Range(7, 3))
            foreach (int depthPos in Enumerable.Range(1, 2))
                secondSegments[anglePos, depthPos] = true;
            TouchState secondTouch = new(secondSegments);
            // In ScoringManager, MaybeUpdateMinAverageOffset is called before Swiped, but the order _shouldn't_ matter.
            Assert.IsTrue(note.Swiped(secondTouch));
            note.MaybeUpdateMinAverageOffset(secondTouch);
            Assert.IsTrue(note.Swiped(secondTouch));
        }

        [Test]
        public void FullCircleSwipeDoesntCountIfTooSmall()
        {
            SwipeNote note = SwipeNote.CreateSwipe(0, 0, 0, 60, 1, SwipeNote.SwipeDirection.Counterclockwise);

            bool[,] firstSegments = new bool[60, 4];
            // 3x2 segments from (2, 1) to (4, 2) inclusive
            foreach (int anglePos in Enumerable.Range(2, 3))
            foreach (int depthPos in Enumerable.Range(1, 2))
                firstSegments[anglePos, depthPos] = true;
            TouchState firstTouch = new(firstSegments);
            // In ScoringManager, MaybeUpdateMinAverageOffset is called before Swiped, but the order _shouldn't_ matter.
            Assert.IsFalse(note.Swiped(firstTouch));
            note.MaybeUpdateMinAverageOffset(firstTouch);
            Assert.IsFalse(note.Swiped(firstTouch));

            bool[,] secondSegments = new bool[60, 4];
            // shift the range counterclockwise by only 1, so now:
            // 3x2 segments from (3, 1) to (5, 2) inclusive
            foreach (int anglePos in Enumerable.Range(3, 3))
            foreach (int depthPos in Enumerable.Range(1, 2))
                secondSegments[anglePos, depthPos] = true;
            TouchState secondTouch = new(secondSegments);
            // In ScoringManager, MaybeUpdateMinAverageOffset is called before Swiped, but the order _shouldn't_ matter.
            Assert.IsFalse(note.Swiped(secondTouch));
            note.MaybeUpdateMinAverageOffset(secondTouch);
            Assert.IsFalse(note.Swiped(secondTouch));
        }

        [Test]
        public void FullCircleSwipeDoesntCountIfWrongDirection()
        {
            // This is the same as FullCircleSwipeCounts, but the swipe direction is CW instead of CCW.
            SwipeNote note = SwipeNote.CreateSwipe(0, 0, 0, 60, 1, SwipeNote.SwipeDirection.Clockwise);

            bool[,] firstSegments = new bool[60, 4];
            // 3x2 segments from (2, 1) to (4, 2) inclusive
            foreach (int anglePos in Enumerable.Range(2, 3))
            foreach (int depthPos in Enumerable.Range(1, 2))
                firstSegments[anglePos, depthPos] = true;
            TouchState firstTouch = new(firstSegments);
            // In ScoringManager, MaybeUpdateMinAverageOffset is called before Swiped, but the order _shouldn't_ matter.
            Assert.IsFalse(note.Swiped(firstTouch));
            note.MaybeUpdateMinAverageOffset(firstTouch);
            Assert.IsFalse(note.Swiped(firstTouch));

            bool[,] secondSegments = new bool[60, 4];
            // shift the range 5 counterclockwise, so now:
            // 3x2 segments from (7, 1) to (9, 2) inclusive
            foreach (int anglePos in Enumerable.Range(7, 3))
            foreach (int depthPos in Enumerable.Range(1, 2))
                secondSegments[anglePos, depthPos] = true;
            TouchState secondTouch = new(secondSegments);
            // In ScoringManager, MaybeUpdateMinAverageOffset is called before Swiped, but the order _shouldn't_ matter.
            Assert.IsFalse(note.Swiped(secondTouch));
            note.MaybeUpdateMinAverageOffset(secondTouch);
            Assert.IsFalse(note.Swiped(secondTouch));
        }

        [Test]
        public void FullCircleSwipeAcrossAnglePosZeroCounts()
        {
            // Same as FullCircleSwipeCounts but the swipe goes across anglePos 0.
            SwipeNote note = SwipeNote.CreateSwipe(0, 0, 0, 60, 1, SwipeNote.SwipeDirection.Counterclockwise);

            bool[,] firstSegments = new bool[60, 4];
            // 3x2 segments from (56, 1) to (58, 2) inclusive
            foreach (int anglePos in Enumerable.Range(56, 3))
            foreach (int depthPos in Enumerable.Range(1, 2))
                firstSegments[anglePos, depthPos] = true;
            TouchState firstTouch = new(firstSegments);
            // In ScoringManager, MaybeUpdateMinAverageOffset is called before Swiped, but the order _shouldn't_ matter.
            Assert.IsFalse(note.Swiped(firstTouch));
            note.MaybeUpdateMinAverageOffset(firstTouch);
            Assert.IsFalse(note.Swiped(firstTouch));

            bool[,] secondSegments = new bool[60, 4];
            // shift the range 5 counterclockwise, so now:
            // 3x2 segments from (1, 1) to (3, 2) inclusive
            foreach (int anglePos in Enumerable.Range(1, 3))
            foreach (int depthPos in Enumerable.Range(1, 2))
                secondSegments[anglePos, depthPos] = true;
            TouchState secondTouch = new(secondSegments);
            // In ScoringManager, MaybeUpdateMinAverageOffset is called before Swiped, but the order _shouldn't_ matter.
            Assert.IsTrue(note.Swiped(secondTouch));
            note.MaybeUpdateMinAverageOffset(secondTouch);
            Assert.IsTrue(note.Swiped(secondTouch));
        }

        [Test]
        public void WrongDirectionFullCircleSwipeAcrossAnglePosZeroDoesntCount()
        {
            // Same as FullCircleSwipeAcrossAnglePosZeroCounts but the swipe direction is CW instead of CCW.
            SwipeNote note = SwipeNote.CreateSwipe(0, 0, 0, 60, 1, SwipeNote.SwipeDirection.Clockwise);

            bool[,] firstSegments = new bool[60, 4];
            // 3x2 segments from (56, 1) to (58, 2) inclusive
            foreach (int anglePos in Enumerable.Range(56, 3))
            foreach (int depthPos in Enumerable.Range(1, 2))
                firstSegments[anglePos, depthPos] = true;
            TouchState firstTouch = new(firstSegments);
            // In ScoringManager, MaybeUpdateMinAverageOffset is called before Swiped, but the order _shouldn't_ matter.
            Assert.IsFalse(note.Swiped(firstTouch));
            note.MaybeUpdateMinAverageOffset(firstTouch);
            Assert.IsFalse(note.Swiped(firstTouch));

            bool[,] secondSegments = new bool[60, 4];
            // shift the range 5 counterclockwise, so now:
            // 3x2 segments from (1, 1) to (3, 2) inclusive
            foreach (int anglePos in Enumerable.Range(1, 3))
            foreach (int depthPos in Enumerable.Range(1, 2))
                secondSegments[anglePos, depthPos] = true;
            TouchState secondTouch = new(secondSegments);
            // In ScoringManager, MaybeUpdateMinAverageOffset is called before Swiped, but the order _shouldn't_ matter.
            Assert.IsFalse(note.Swiped(secondTouch));
            note.MaybeUpdateMinAverageOffset(secondTouch);
            Assert.IsFalse(note.Swiped(secondTouch));
        }

        [Test]
        public void TwoHandedFullCircleSwipeAcrossAnglePosZeroAndThirtyCounts()
        {
            // Basically like FullCircleSwipeAcrossAnglePosZeroCounts but with a second hand swiping in the same
            // direction across anglePos 30. This is the case that breaks if we only have two "virtual notes".
            SwipeNote note = SwipeNote.CreateSwipe(0, 0, 0, 60, 1, SwipeNote.SwipeDirection.Counterclockwise);

            bool[,] firstSegments = new bool[60, 4];
            // right hand: 3x2 segments from (56, 1) to (58, 2) inclusive
            foreach (int anglePos in Enumerable.Range(56, 3))
            foreach (int depthPos in Enumerable.Range(1, 2))
                firstSegments[anglePos, depthPos] = true;
            // left hand: 3x2 segments from (26, 1) to (28, 2) inclusive
            foreach (int anglePos in Enumerable.Range(26, 3))
            foreach (int depthPos in Enumerable.Range(1, 2))
                firstSegments[anglePos, depthPos] = true;
            TouchState firstTouch = new(firstSegments);
            // In ScoringManager, MaybeUpdateMinAverageOffset is called before Swiped, but the order _shouldn't_ matter.
            Assert.IsFalse(note.Swiped(firstTouch));
            note.MaybeUpdateMinAverageOffset(firstTouch);
            Assert.IsFalse(note.Swiped(firstTouch));

            bool[,] secondSegments = new bool[60, 4];
            // shift the range 5 counterclockwise, so now:
            // right hand: 3x2 segments from (1, 1) to (3, 2) inclusive
            foreach (int anglePos in Enumerable.Range(1, 3))
            foreach (int depthPos in Enumerable.Range(1, 2))
                secondSegments[anglePos, depthPos] = true;
            // left hand: 3x2 segments from (31, 1) to (33, 2) inclusive
            foreach (int anglePos in Enumerable.Range(31, 3))
            foreach (int depthPos in Enumerable.Range(1, 2))
                secondSegments[anglePos, depthPos] = true;
            TouchState secondTouch = new(secondSegments);
            // In ScoringManager, MaybeUpdateMinAverageOffset is called before Swiped, but the order _shouldn't_ matter.
            Assert.IsTrue(note.Swiped(secondTouch));
            note.MaybeUpdateMinAverageOffset(secondTouch);
            Assert.IsTrue(note.Swiped(secondTouch));
        }
    }
}
