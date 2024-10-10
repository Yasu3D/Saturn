using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SaturnGame.RhythmGame;
using SaturnGame.RhythmGame.Loading;
using static TestUtils;

public class ChartLoaderTests
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
                    InvokePrivateStaticMethod<ChartLoader, bool>("SegmentsOverlap", fullCircleNote, otherNote),
                    "Didn't overlap with position {0} and size {1}", position, size);
                Assert.IsTrue(
                    InvokePrivateStaticMethod<ChartLoader, bool>("SegmentsOverlap", otherNote, fullCircleNote),
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
            Assert.IsTrue(InvokePrivateStaticMethod<ChartLoader, bool>("SegmentsOverlap", firstNote, secondNote));
            Assert.IsTrue(InvokePrivateStaticMethod<ChartLoader, bool>("SegmentsOverlap", secondNote, firstNote));
        }
        {
            PositionedChartElement firstNote = new SimplePositionedElement(56, 5);
            PositionedChartElement secondNote = new SimplePositionedElement(0, 5);
            Assert.IsTrue(InvokePrivateStaticMethod<ChartLoader, bool>("SegmentsOverlap", firstNote, secondNote));
            Assert.IsTrue(InvokePrivateStaticMethod<ChartLoader, bool>("SegmentsOverlap", secondNote, firstNote));
        }
    }

    [Test]
    public void EdgeCaseNotesDoNotOverlap()
    {
        {
            PositionedChartElement firstNote = new SimplePositionedElement(0, 5);
            PositionedChartElement secondNote = new SimplePositionedElement(5, 5);
            Assert.IsFalse(InvokePrivateStaticMethod<ChartLoader, bool>("SegmentsOverlap", firstNote, secondNote));
            Assert.IsFalse(InvokePrivateStaticMethod<ChartLoader, bool>("SegmentsOverlap", secondNote, firstNote));
        }
        {
            PositionedChartElement firstNote = new SimplePositionedElement(55, 5);
            PositionedChartElement secondNote = new SimplePositionedElement(0, 5);
            Assert.IsFalse(InvokePrivateStaticMethod<ChartLoader, bool>("SegmentsOverlap", firstNote, secondNote));
            Assert.IsFalse(InvokePrivateStaticMethod<ChartLoader, bool>("SegmentsOverlap", secondNote, firstNote));
        }
    }

    /// ProcessHitWindows tests

    private const float FrameMs = 1000f / 60f;
    [Test]
    public void TwoOverlappingNotesSplitEvenly()
    {
        TouchNote note1 = new(0, 0, 0, 5, 1) { TimeMs = 0 };
        TouchNote note2 = new(0, 64, 0, 5, 2) { TimeMs = 64 };
        Chart chart = new() { Notes = new List<Note> { note1, note2 } };
        InvokePrivateStaticMethod<ChartLoader>("ProcessHitWindows", chart);
        Assert.AreEqual(-100f, note1.EarliestHitTimeMs);
        Assert.AreEqual(32f, note1.LatestHitTimeMs);
        Assert.AreEqual(32f, note2.EarliestHitTimeMs);
        Assert.AreEqual(164f, note2.LatestHitTimeMs);
    }

    [Test]
    public void OverlappingWithChainDoesNotTruncateChain()
    {
        // ChainNotes have max ±4frame (66.67ms) window, but TouchNotes have ±6frame (100ms) window.
        // These can overlap, but the midpoints will be outside the max window of the chain note.
        // The touch note's window shouldn't truncate past the start of the chain note's window.
        TouchNote note1 = new(0, 0, 0, 5, 1) { TimeMs = 0 };
        ChainNote note2 = new(0, 140, 0, 5, 2) { TimeMs = 140 };
        Chart chart = new() { Notes = new List<Note> { note1, note2 } };
        InvokePrivateStaticMethod<ChartLoader>("ProcessHitWindows", chart);
        Assert.AreEqual(-100f, note1.EarliestHitTimeMs);
        Assert.AreEqual(140f - 4 * FrameMs, note1.LatestHitTimeMs);
        Assert.Greater(note1.LatestHitTimeMs, 70f /* midpoint */);
        Assert.AreEqual(140f - 4 * FrameMs, note2.EarliestHitTimeMs);
        Assert.AreEqual(140f + 4 * FrameMs, note2.LatestHitTimeMs);
    }

    [Test]
    public void OverlappingWithChainDoesNotTruncateChainInReverse()
    {
        // see OverlappingWithChainDoesNotTruncateChain
        ChainNote note1 = new(0, 0, 0, 5, 1) { TimeMs = 0 };
        TouchNote note2 = new(0, 140, 0, 5, 2) { TimeMs = 140 };
        Chart chart = new() { Notes = new List<Note> { note1, note2 } };
        InvokePrivateStaticMethod<ChartLoader>("ProcessHitWindows", chart);
        Assert.AreEqual(-4 * FrameMs, note1.EarliestHitTimeMs);
        Assert.AreEqual(4 * FrameMs, note1.LatestHitTimeMs);
        Assert.Less(note2.EarliestHitTimeMs, 70f /* midpoint */);
        Assert.AreEqual(4 * FrameMs, note2.EarliestHitTimeMs);
        Assert.AreEqual(240f, note2.LatestHitTimeMs);
    }

    [Test]
    public void MultipleOverlappingWindowsDontLeaveGaps()
    {
        // Consider the following diagram of notes in close succession:
        //
        // ------         <-- note3 (latest)
        //
        //       -------  <-- note2
        //
        // - - - - - - -  <-- hitwindow cutoff
        //
        // -------------  <-- note1 (earliest)
        //
        // Here, we must make sure that note3's hit window is only truncated to wherever note1's hitwindow ends.
        // Naively it might get truncated to the midpoint between note1 and note2, which is wrong.
        TouchNote note1 = new(0, 0, 0, 10, 1) { TimeMs = 0 };
        TouchNote note2 = new(0, 8, 5, 5, 2) { TimeMs = 8f };
        TouchNote note3 = new(0, 12, 0, 5, 3) { TimeMs = 12f };
        Chart chart = new() { Notes = new List<Note> { note1, note2, note3 } };
        InvokePrivateStaticMethod<ChartLoader>("ProcessHitWindows", chart);
        Assert.AreEqual(4f, note1.LatestHitTimeMs);
        Assert.AreEqual(4f, note2.EarliestHitTimeMs);
        Assert.AreEqual(4f, note3.EarliestHitTimeMs);
    }

    [Test]
    public void MultipleOverlappingWindowsDontLeaveGapsInReverse()
    {
        // See MultipleOverlappingWindowsDontLeaveGaps, but now note3 is earliest.
        TouchNote note3 = new(0, 0, 0, 5, 1) { TimeMs = 0f };
        TouchNote note2 = new(0, 4, 5, 5, 2) { TimeMs = 4f };
        TouchNote note1 = new(0, 12, 0, 10, 3) { TimeMs = 12f };
        Chart chart = new() { Notes = new List<Note> { note1, note2, note3 } };
        InvokePrivateStaticMethod<ChartLoader>("ProcessHitWindows", chart);
        Assert.AreEqual(8f, note1.EarliestHitTimeMs);
        Assert.AreEqual(8f, note2.LatestHitTimeMs);
        Assert.AreEqual(8f, note3.LatestHitTimeMs);
    }
}
