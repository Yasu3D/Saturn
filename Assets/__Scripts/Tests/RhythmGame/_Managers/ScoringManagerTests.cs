using System.Linq;
using NUnit.Framework;
using SaturnGame.RhythmGame;
using static TestUtils;

public class ScoringManagerTests
{
    /// SegmentsOverlap tests.

    private class SimplePositionedElement : PositionedChartElement
    {
        public SimplePositionedElement(int position, int size) : base(0, 0, position, size) { }
    }

    [Test]
    public void FullCircleNotesOverlap()
    {
        // 60 * 60 * 60 = 216000 iterations - a lot but it should be fine
        foreach (int fullCirclePosition in Enumerable.Range(0, 60))
        {
            // Test all possible positions for the full circle note
            PositionedChartElement fullCircleNote = new SimplePositionedElement(fullCirclePosition, 60);

            // Test all possible note size/positions for the other note
            foreach (int position in Enumerable.Range(0, 60))
            foreach (int size in Enumerable.Range(1, 60))
            {
                Assert.IsTrue(position is >= 0 and <= 59);
                Assert.IsTrue(size is >= 1 and <= 60);
                PositionedChartElement otherNote = new SimplePositionedElement(position, size);
                Assert.IsTrue(
                    InvokePrivateStaticMethod<ScoringManager, bool>("SegmentsOverlap", fullCircleNote, otherNote),
                    "Didn't overlap with position {0} and size {1}", position, size);
                Assert.IsTrue(
                    InvokePrivateStaticMethod<ScoringManager, bool>("SegmentsOverlap", otherNote, fullCircleNote),
                    "Didn't overlap with position {0} and size {1} (swapped arguments)", position, size);
            }
        }
    }

    [Test]
    public void EdgeCaseNotesOverlap()
    {
        {
            PositionedChartElement firstNote = new SimplePositionedElement(0, 5);
            PositionedChartElement secondNote = new SimplePositionedElement(4, 5);
            Assert.IsTrue(InvokePrivateStaticMethod<ScoringManager, bool>("SegmentsOverlap", firstNote, secondNote));
            Assert.IsTrue(InvokePrivateStaticMethod<ScoringManager, bool>("SegmentsOverlap", secondNote, firstNote));
        }
        {
            PositionedChartElement firstNote = new SimplePositionedElement(56, 5);
            PositionedChartElement secondNote = new SimplePositionedElement(0, 5);
            Assert.IsTrue(InvokePrivateStaticMethod<ScoringManager, bool>("SegmentsOverlap", firstNote, secondNote));
            Assert.IsTrue(InvokePrivateStaticMethod<ScoringManager, bool>("SegmentsOverlap", secondNote, firstNote));
        }
    }

    [Test]
    public void EdgeCaseNotesDoNotOverlap()
    {
        {
            PositionedChartElement firstNote = new SimplePositionedElement(0, 5);
            PositionedChartElement secondNote = new SimplePositionedElement(5, 5);
            Assert.IsFalse(InvokePrivateStaticMethod<ScoringManager, bool>("SegmentsOverlap", firstNote, secondNote));
            Assert.IsFalse(InvokePrivateStaticMethod<ScoringManager, bool>("SegmentsOverlap", secondNote, firstNote));
        }
        {
            PositionedChartElement firstNote = new SimplePositionedElement(55, 5);
            PositionedChartElement secondNote = new SimplePositionedElement(0, 5);
            Assert.IsFalse(InvokePrivateStaticMethod<ScoringManager, bool>("SegmentsOverlap", firstNote, secondNote));
            Assert.IsFalse(InvokePrivateStaticMethod<ScoringManager, bool>("SegmentsOverlap", secondNote, firstNote));
        }
    }
}
