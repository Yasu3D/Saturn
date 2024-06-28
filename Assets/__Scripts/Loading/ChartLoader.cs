using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using SaturnGame.Loading;
using SaturnGame.Settings;
using UnityEngine;

namespace SaturnGame.RhythmGame
{
public class ChartLoader
{
    public static Chart LoadChart(string filepath)
    {
        ChartLoader loader = new();

        if (!loader.LoadChartFromFilename(filepath))
            throw new Exception("Failed to load chart");

        return loader.chart;
    }

    private Chart chart;

    private readonly List<Gimmick> bpmGimmicks = new();
    private readonly List<Gimmick> timeSigGimmicks = new();

    private int readerIndex;

    /// <summary>
    /// Parses a <c>.mer</c> file and creates lists of objects from it.
    /// </summary>
    /// <param name="filepath"></param>
    /// <returns></returns>
    private bool LoadChartFromFilename(string filepath)
    {
        if (!File.Exists(filepath)) return false;

        FileStream fileStream = new(filepath, FileMode.Open, FileAccess.Read);
        List<string> merFile = MerLoader.LoadMer(fileStream);

        readerIndex = 0;
        bpmGimmicks.Clear();
        timeSigGimmicks.Clear();

        chart = new Chart();

        ParseMetadata(merFile);
        ParseChart(merFile);
        GenerateBarLines();
        CreateBgmData();
        CreateHiSpeedData();
        SetTime();
        ProcessHitWindows(chart);

        if (chart.ReverseGimmicks.Count != 0)
            GenerateReverseLists();

        if (SettingsManager.Instance.PlayerSettings.GameSettings.MirrorNotes != 0)
            MirrorChart();

        if (!CheckLoadErrors().passed)
        {
            Debug.LogError($"Chart load failed! | {CheckLoadErrors().error}");
            return false;
        }

        // TODO: Remove this
        Debug.Log("[Chart Load] Chart loaded successfully!");
        return true;
    }


    /// <summary>
    /// Loops through a .mer file's metadata tags until it<br />
    /// either finds a <c>#BODY</c> tag or runs out of lines to parse.
    /// </summary>
    /// <param name="merFile"></param>
    private void ParseMetadata([CanBeNull] List<string> merFile)
    {
        if (merFile == null) return;

        do
        {
            string merLine = merFile[readerIndex];

            string tempDifficultyString = MerLoader.GetMetadata(merLine, "#DIFFICULTY ");
            if (tempDifficultyString != null)
                chart.Difficulty = Convert.ToSingle(tempDifficultyString, CultureInfo.InvariantCulture);

            string tempClearThresholdString = MerLoader.GetMetadata(merLine, "#CLEAR_THRESHOLD");
            if (tempClearThresholdString != null)
                chart.ClearThreshold = Convert.ToSingle(tempClearThresholdString, CultureInfo.InvariantCulture);

            string tempAudioOffsetString = MerLoader.GetMetadata(merLine, "#OFFSET ");
            if (tempAudioOffsetString != null)
                chart.AudioOffset = Convert.ToSingle(tempAudioOffsetString, CultureInfo.InvariantCulture);

            string tempMovieOffsetString = MerLoader.GetMetadata(merLine, "#MOVIEOFFSET ");
            if (tempMovieOffsetString != null)
                chart.MovieOffset = Convert.ToSingle(tempMovieOffsetString, CultureInfo.InvariantCulture);

            if (merLine.Contains("#BODY"))
            {
                readerIndex++;
                break;
            }
        } while (++readerIndex < merFile.Count);
    }

    /// <summary>
    /// Loops through a .mer file's body and adds chartObjects to appropriate lists.
    /// </summary>
    /// <param name="merFile"></param>
    private void ParseChart(List<string> merFile)
    {
        Note lastNote = null; // make lastNote start as null so the compiler doesn't scream
        Gimmick tempGimmick;
        // (incomplete HoldNote, next segment noteId)
        List<(HoldNote, int)> incompleteHoldNotes = new();
        // noteId -> (segment, next segment noteId)
        Dictionary<int, (HoldSegment, int?)> allHoldSegments = new();

        for (int i = readerIndex; i < merFile.Count; i++)
        {
            if (string.IsNullOrEmpty(merFile[i])) continue;

            string[] splitLine = merFile[i].Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            int measure = Convert.ToInt32(splitLine[0], CultureInfo.InvariantCulture);
            int tick = Convert.ToInt32(splitLine[1], CultureInfo.InvariantCulture);
            int objectID = Convert.ToInt32(splitLine[2], CultureInfo.InvariantCulture);

            switch (objectID)
            {
                // Invalid ID
                case 0:
                {
                    continue;
                }
                // Note
                case 1:
                {
                    int noteTypeID = Convert.ToInt32(splitLine[3], CultureInfo.InvariantCulture);

                    // end of chart note
                    if (noteTypeID is 14)
                    {
                        chart.EndOfChart = new(measure, tick);
                        continue;
                    }

                    int position = Convert.ToInt32(splitLine[5], CultureInfo.InvariantCulture);
                    int size = Convert.ToInt32(splitLine[6], CultureInfo.InvariantCulture);

                    Note tempNote;

                    switch (noteTypeID)
                    {
                        // hold notes
                        case 9 or 25:
                        {
                            HoldSegment holdStart = new(measure, tick, position, size, true);
                            int nextNoteID = Convert.ToInt32(splitLine[8], CultureInfo.InvariantCulture);

                            HoldNote hold = new(holdStart);
                            hold.SetBonusTypeFromNoteID(noteTypeID);
                            incompleteHoldNotes.Add((hold, nextNoteID));

                            tempNote = hold;
                            chart.HoldNotes.Add(hold);
                            break;
                        }
                        case 10 or 11:
                        {
                            int noteId = Convert.ToInt32(splitLine[4], CultureInfo.InvariantCulture);
                            bool renderFlag = Convert.ToInt32(splitLine[7], CultureInfo.InvariantCulture) == 1;
                            int? nextNoteId = noteTypeID == 10
                                ? Convert.ToInt32(splitLine[8], CultureInfo.InvariantCulture)
                                : null;
                            allHoldSegments.Add(noteId,
                                (new HoldSegment(measure, tick, position, size, renderFlag), nextNoteId));
                            continue; // don't proceed to sync check for hold segments
                        }
                        // mask notes
                        case 12 or 13:
                        {
                            int dir = Convert.ToInt32(splitLine[8], CultureInfo.InvariantCulture);
                            bool add = noteTypeID == 12;
                            chart.Masks.Add(new Mask(measure, tick, position, size, (Mask.MaskDirection)dir, add));
                            continue;
                        }
                        default:
                        {
                            Note note = Note.CreateFromNoteID(measure, tick, noteTypeID, position, size);
                            chart.Notes.Add(note);
                            tempNote = note;
                            break;
                        }
                    }

                    if (tempNote is null)
                        Debug.LogError($"null noteid {noteTypeID}");

                    // all other notes
                    CheckSync(tempNote, lastNote);
                    lastNote = tempNote;
                    break;
                }
                // Gimmick
                default:
                {
                    // create a gimmick
                    object value1 = null;
                    object value2 = null;

                    // avoid IndexOutOfRangeExceptions :]
                    if (objectID is 3)
                    {
                        if (splitLine.Length == 4)
                        {
                            value1 = Convert.ToInt32(splitLine[3], CultureInfo.InvariantCulture);
                            value2 = 4; // yeah.......... this happens in the wild
                        }
                        else if (splitLine.Length > 4)
                        {
                            value1 = Convert.ToInt32(splitLine[3], CultureInfo.InvariantCulture);
                            value2 = Convert.ToInt32(splitLine[4], CultureInfo.InvariantCulture);
                        }
                    }

                    if (objectID is 2 or 5 && splitLine.Length > 3)
                        value1 = Convert.ToSingle(splitLine[3], CultureInfo.InvariantCulture);

                    tempGimmick = new Gimmick(measure, tick, objectID, value1, value2);

                    // sort gimmicks by type
                    switch (tempGimmick.Type)
                    {
                        case Gimmick.GimmickType.BeatsPerMinute:
                        {
                            bpmGimmicks.Add(tempGimmick);
                            break;
                        }
                        case Gimmick.GimmickType.TimeSignature:
                        {
                            timeSigGimmicks.Add(tempGimmick);
                            break;
                        }
                        case Gimmick.GimmickType.HiSpeed:
                        {
                            chart.HiSpeedGimmicks.Add(tempGimmick);
                            break;
                        }
                        case Gimmick.GimmickType.StopStart:
                        {
                            // Convert Stops to HiSpeed changes internally since they're functionally identical(?)
                            tempGimmick.Type = Gimmick.GimmickType.StopStart;
                            tempGimmick.HiSpeed = 0;
                            chart.HiSpeedGimmicks.Add(tempGimmick);
                            break;
                        }
                        case Gimmick.GimmickType.StopEnd:
                        {
                            // Same as above.
                            tempGimmick.Type = Gimmick.GimmickType.StopEnd;
                            tempGimmick.HiSpeed = chart.HiSpeedGimmicks.LastOrDefault(x =>
                                    x.ChartTick < tempGimmick.ChartTick && x.Type is Gimmick.GimmickType.HiSpeed)
                                ?.HiSpeed ?? 1;
                            chart.HiSpeedGimmicks.Add(tempGimmick);
                            break;
                        }
                        case Gimmick.GimmickType.ReverseEffectStart:
                        case Gimmick.GimmickType.ReverseEffectEnd:
                        case Gimmick.GimmickType.ReverseNoteEnd:
                        {
                            chart.ReverseGimmicks.Add(tempGimmick);
                            break;
                        }
                    }

                    break;
                }
            }
        }

        // Assemble holds
        foreach ((HoldNote holdNote, int? firstNoteId) in incompleteHoldNotes)
        {
            List<HoldSegment> holdSegments = holdNote.Notes.ToList();
            int? currentNoteId = firstNoteId;

            while (currentNoteId is int noteId)
            {
                (HoldSegment segment, int? nextNoteId) = allHoldSegments[noteId];
                holdSegments.Add(segment);
                currentNoteId = nextNoteId;
            }

            holdNote.Notes = holdSegments.ToArray();
        }
    }

    /// <summary>
    /// Check for common errors that may happen during a chart load.
    /// </summary>
    private (bool passed, string error) CheckLoadErrors()
    {
        // I made the checks separate to spare the next person reading this an aneurysm.
        // It's also organized from most likely to least likely, so it doesn't matter much.
        if (chart.EndOfChart is null)
            return (false, "Chart is missing End of Chart note!");

        if (chart.Notes.Last().TimeMs > chart.EndOfChart.TimeMs)
            return (false, "Notes behind end of Chart note!");

        // this is fine actually lol
        //if (bgmClip != null && chart.notes.Last().TimeMs > bgmClip.length * 1000) // conv. to ms
        //    return (false, "Chart is longer than audio!");

        if (chart.BGMDataGimmicks.Count == 0)
            return (false, "Chart is missing BPM and TimeSignature data!");

        // Reverses always come in groups of 3 gimmicks. If the count is not divisible by 3 something's wrong.
        if (SaturnMath.Modulo(chart.ReverseGimmicks.Count, 3) != 0)
        {
            return (false,
                "Invalid reverse gimmicks! Every reverse must have these segments: [Effect Start] [Effect End] [Note End]");
        }

        // Loop through all reverses to find any overlapping/out of order/broken ones.
        // The order must always be Effect Start > Effect End > Note End.
        Gimmick.GimmickType lastReverse = Gimmick.GimmickType.ReverseNoteEnd;
        foreach (Gimmick gimmick in chart.ReverseGimmicks)
        {
            Gimmick.GimmickType currentReverse = gimmick.Type;

            if ((currentReverse is Gimmick.GimmickType.ReverseEffectStart &&
                 lastReverse is not Gimmick.GimmickType.ReverseNoteEnd) ||
                (currentReverse is Gimmick.GimmickType.ReverseEffectEnd &&
                 lastReverse is not Gimmick.GimmickType.ReverseEffectStart) ||
                (currentReverse is Gimmick.GimmickType.ReverseNoteEnd &&
                 lastReverse is not Gimmick.GimmickType.ReverseEffectEnd))
                return (false, "Invalid reverse gimmicks! Reverses are either overlapping or broken.");

            lastReverse = currentReverse;
        }

        return (true, "");
    }

    /// <summary>
    /// Adds reversed notes to a list for Reverse Gimmick animations.
    /// </summary>
    private void GenerateReverseLists()
    {
        // Loop over all Reverse Gimmicks except the last two to avoid an ArrayIndexOutOfBoundsException
        for (int i = 0; i < chart.ReverseGimmicks.Count - 2; i++)
        {
            if (chart.ReverseGimmicks[i].Type is not Gimmick.GimmickType.ReverseEffectStart)
                continue;

            // If [i] is EffectStart, then [i + 1] must be EffectEnd and [i + 2] must be NoteEnd
            float effectStartTime = chart.ReverseGimmicks[i].ScaledVisualTime;
            float effectEndTime = chart.ReverseGimmicks[i + 1].ScaledVisualTime;
            float noteEndTime = chart.ReverseGimmicks[i + 2].ScaledVisualTime;

            List<Note> notesToReverse = chart.Notes
                .Where(x => x.ScaledVisualTime >= effectEndTime && x.ScaledVisualTime < noteEndTime).ToList();
            List<HoldNote> holdsToReverse = chart.HoldNotes.Where(x =>
                x.Start.ScaledVisualTime >= effectEndTime && x.End.ScaledVisualTime < noteEndTime).ToList();

            // TODO: reverse syncs?

            foreach (Note note in notesToReverse)
                ReverseNote(note, effectStartTime, effectEndTime, noteEndTime);

            foreach (HoldNote hold in holdsToReverse)
                ReverseHold(hold, effectStartTime, effectEndTime, noteEndTime);

            // List.Reverse() from Linq
            chart.ReverseHoldNotes.Reverse();
        }
    }

    /// <summary>
    /// Creates a copy of a Note, remaps it's position in time, <br />
    /// then adds a copy of it to <c>reverseNotes</c>
    /// </summary>
    private void ReverseNote([NotNull] Note note, float startTime, float midTime, float endTime)
    {
        Note copy = (Note)note.Clone();
        copy.ReverseTime(startTime, midTime, endTime);

        chart.ReverseNotes.Insert(0, copy);
    }

    /// <summary>
    /// Reverses a Hold Note by creating a deep copy of it, <br />
    /// then remapping each segment note's position in time.
    /// </summary>
    private void ReverseHold([NotNull] HoldNote hold, float startTime, float midTime, float endTime)
    {
        HoldNote copy = HoldNote.DeepCopy(hold);
        copy.ReverseTime(startTime, midTime, endTime);

        chart.ReverseHoldNotes.Add(copy);
    }

    /// <summary>
    /// Generates a Bar Line every Measure.
    /// </summary>
    private void GenerateBarLines()
    {
        for (int i = 0; i <= chart.EndOfChart.Measure; i++)
            chart.BarLines.Add(new BarLine(i, 0));
    }

    /// <summary>
    /// Check if the last parsed note is on the same timestamp as the current note. <br />
    /// This should efficiently and cleanly detect any simultaneous notes.
    /// </summary>
    private void CheckSync(Note current, [CanBeNull] Note last)
    {
        if (last == null) return;
        if ((last is ChainNote && last.BonusType != Note.NoteBonusType.RNote) || (current is ChainNote && current.BonusType != Note.NoteBonusType.RNote)) return;
        if ((last is HoldNote || current is HoldNote) && last.Position == current.Position && last.Size == current.Size) return;

        if (current.Measure == last.Measure && current.Tick == last.Tick)
        {
            last.IsSync = true;
            current.IsSync = true;

            GenerateSync(current, last);
        }
    }

    /// <summary>
    /// Finds shortest distance between two notes and <br />
    /// generates a sync object connecting them
    /// </summary>
    private void GenerateSync([NotNull] Note note0, [NotNull] Note note1)
    {
        int measure = note0.Measure;
        int tick = note0.Tick;

        // Instead of finding the shortest path between 4 points,
        // think of two "imaginary" notes between note0 and note1 that fill the gaps.
        // Check which of these two imaginary notes is smaller, then return that note.

        // The somewhat "random" math here is explainable. Trust me.
        // For the sizes, it subtracts 1 in the modulo, then adds back 1 after to "shift" the range from 0-60 to 1-59.
        // The notes are also shrunk by 1 on each side, so [pos + 1] and [size - 2].
        // These two are then simplified because they cancel each other out.
        // The comments after each line show what it was before the simplification.
        // Thanks for coming to my ted talk. Happy contributing.

        // cg505's note: This may not work when there are more than 2 simultaneous notes... but let's not
        // get a headache over that at this point.
        // yasu's note: it appears mercury's system is just as "dumb". Good enough :3

        int position0 = SaturnMath.Modulo(note0.Position + note0.Size - 1, 60); // pos + 1 // size  - 2
        int size0 = SaturnMath.Modulo(note1.Position - position0, 60) + 1; // pos + 1 // shift - 1

        int position1 = SaturnMath.Modulo(note1.Position + note1.Size - 1, 60); // pos + 1 // size  - 2
        int size1 = SaturnMath.Modulo(note0.Position - position1, 60) + 1; // pos + 1 // shift - 1

        int finalPosition = size0 > size1 ? position1 : position0;
        int finalSize = Mathf.Min(size0, size1);

        if (finalSize > 30) return;

        SyncIndicator sync = new(measure, tick, finalPosition, finalSize);
        chart.Syncs.Add(sync);
    }

    /// <summary>
    /// Mirrors a note along an axis.
    /// </summary>
    /// <remarks>
    /// Axis 30 = horizontal mirror <br/>
    /// Axis 0 = vertical mirror
    /// </remarks>
    private static void MirrorObject([NotNull] PositionedChartElement note, int axis = 30)
    {
        int newPos = SaturnMath.Modulo(axis - note.Size - note.Position, 60);

        if (note is SwipeNote swipeNote)
        {
            switch (swipeNote.Direction)
            {
                case SwipeNote.SwipeDirection.Clockwise:
                {
                    swipeNote.Direction = SwipeNote.SwipeDirection.Counterclockwise;
                    break;
                }
                case SwipeNote.SwipeDirection.Counterclockwise:
                {
                    swipeNote.Direction = SwipeNote.SwipeDirection.Clockwise;
                    break;
                }
            }
        }

        note.Position = newPos;
    }

    /// <summary>
    /// Mirrors an entire chart.
    /// </summary>
    private void MirrorChart()
    {
        foreach (Note note in chart.Notes)
            MirrorObject(note);

        foreach (Mask mask in chart.Masks)
            MirrorObject(mask);

        foreach (SyncIndicator sync in chart.Syncs)
            MirrorObject(sync);

        foreach (HoldNote hold in chart.HoldNotes)
        {
            foreach (HoldSegment note in hold.Notes)
                MirrorObject(note);
        }

        foreach (Note note in chart.ReverseNotes)
            MirrorObject(note);

        foreach (HoldNote hold in chart.ReverseHoldNotes)
        {
            foreach (HoldSegment note in hold.Notes)
                MirrorObject(note);
        }
    }

    /// <summary>
    /// A rather bulky function that ""cleanly"" merges BeatsPerMinuteGimmicks <br />
    /// and TimeSignatureGimmicks into one list. My only excuse for the bulk <br />
    /// is that most charts don't have many BPM/TimeSig changes.
    /// </summary>
    private void CreateBgmData()
    {
        if (bpmGimmicks.Count == 0 || timeSigGimmicks.Count == 0) return;

        float lastBpm = bpmGimmicks[0].BeatsPerMinute;
        TimeSignature lastTimeSig = timeSigGimmicks[0].TimeSig;

        // merge both lists and sort by timestamp
        IOrderedEnumerable<Gimmick> bpmAndTimeSigGimmicks =
            bpmGimmicks.Concat(timeSigGimmicks).OrderBy(x => x.ChartTick);

        chart.BGMDataGimmicks = new();

        foreach (Gimmick gimmick in bpmAndTimeSigGimmicks)
        {
            Gimmick gimmickToUpdate;
            switch (chart.BGMDataGimmicks.LastOrDefault())
            {
                case Gimmick lastGimmick when lastGimmick.ChartTick == gimmick.ChartTick:
                {
                    gimmickToUpdate = lastGimmick;
                    break;
                }
                default:
                {
                    Gimmick newGimmick = new(gimmick.Measure, gimmick.Tick, lastBpm, lastTimeSig);
                    chart.BGMDataGimmicks.Add(newGimmick);
                    gimmickToUpdate = newGimmick;
                    break;
                }
            }

            switch (gimmick)
            {
                case { Type: Gimmick.GimmickType.BeatsPerMinute, BeatsPerMinute: var bpm }:
                {
                    gimmickToUpdate.BeatsPerMinute = bpm;
                    lastBpm = gimmick.BeatsPerMinute;
                    break;
                }
                case { Type: Gimmick.GimmickType.TimeSignature, TimeSig: var timeSig }:
                {
                    gimmickToUpdate.TimeSig = timeSig;
                    lastTimeSig = timeSig;
                    break;
                }
            }
        }

        chart.BGMDataGimmicks[0].TimeMs = 0;
        for (int i = 1; i < chart.BGMDataGimmicks.Count; i++)
        {
            float lastTime = chart.BGMDataGimmicks[i - 1].TimeMs;
            float currentMeasure = chart.BGMDataGimmicks[i].ChartTick * SaturnMath.TickToMeasure;
            float lastMeasure = chart.BGMDataGimmicks[i - 1].ChartTick * SaturnMath.TickToMeasure;
            float timeSig = chart.BGMDataGimmicks[i - 1].TimeSig.Ratio;
            float bpm = chart.BGMDataGimmicks[i - 1].BeatsPerMinute;

            float time = lastTime + (currentMeasure - lastMeasure) * (4 * timeSig * (60000f / bpm));
            chart.BGMDataGimmicks[i].TimeMs = time;
        }
    }

    /// <summary>
    /// Calculates scaled timestamps for HiSpeed changes.
    /// </summary>
    private void CreateHiSpeedData()
    {
        foreach (Gimmick gimmick in chart.HiSpeedGimmicks)
            gimmick.CalculateTime(chart.BGMDataGimmicks);

        float lastScaledTime = 0;
        float lastTime = 0;
        float lastHiSpeed = 1;

        foreach (Gimmick gimmick in chart.HiSpeedGimmicks)
        {
            float currentTime = gimmick.TimeMs;
            float scaledTime = lastScaledTime + Mathf.Abs(currentTime - lastTime) * lastHiSpeed;
            gimmick.ScaledVisualTime = scaledTime;

            lastScaledTime = scaledTime;
            lastTime = gimmick.TimeMs;
            lastHiSpeed = gimmick.HiSpeed;
        }
    }

    /// <summary>
    /// Loops through every ChartObject in every list <br />
    /// and calculates the object's time in milliseconds <br />
    /// according to all BPM and TimeSignature changes.
    /// </summary>
    private void SetTime()
    {
        foreach (Note note in chart.Notes)
        {
            note.CalculateTime(chart.BGMDataGimmicks);
            note.CalculateScaledTime(chart.HiSpeedGimmicks);
        }

        foreach (SyncIndicator sync in chart.Syncs)
        {
            sync.CalculateTime(chart.BGMDataGimmicks);
            sync.CalculateScaledTime(chart.HiSpeedGimmicks);
        }

        foreach (BarLine obj in chart.BarLines)
        {
            obj.CalculateTime(chart.BGMDataGimmicks);
            obj.CalculateScaledTime(chart.HiSpeedGimmicks);
        }

        foreach (Mask note in chart.Masks)
        {
            note.CalculateTime(chart.BGMDataGimmicks);
            note.CalculateScaledTime(chart.HiSpeedGimmicks);
        }

        foreach (HoldNote hold in chart.HoldNotes)
        {
            hold.CalculateTime(chart.BGMDataGimmicks);
            hold.CalculateScaledTime(chart.HiSpeedGimmicks);
        }

        foreach (Gimmick gimmick in chart.ReverseGimmicks)
        {
            gimmick.CalculateTime(chart.BGMDataGimmicks);
            gimmick.CalculateScaledTime(chart.HiSpeedGimmicks);
        }

        chart.EndOfChart.CalculateTime(chart.BGMDataGimmicks);
        chart.EndOfChart.CalculateScaledTime(chart.HiSpeedGimmicks);
    }

    private static bool SegmentsOverlap([NotNull] PositionedChartElement note1, [NotNull] PositionedChartElement note2)
    {
        if (note1.Left == note1.Right || note2.Left == note2.Right)
        {
            // Full circle notes always overlap
            return true;
        }

        // Bonus reading: https://fgiesen.wordpress.com/2015/09/24/intervals-in-modular-arithmetic/
        // Each note is a half-open interval in mod60.

        // See point_in_half_open_interval in the link.
        // This returns true iff point is in the half-open interval [note.Left, note.Right) (mod 60)
        // TODO: move to SaturnMath
        bool pointWithinNote([NotNull] PositionedChartElement note, int point)
        {
            return SaturnMath.Modulo(point - note.Left, 60) < SaturnMath.Modulo(note.Right - note.Left, 60);
        }

        // See half_open_intervals_overlap in the link.
        // We know that the interval size is positive, so we don't need to worry about that case.
        return pointWithinNote(note1, note2.Left) || pointWithinNote(note2, note1.Left);
    }

    private static void ProcessHitWindows([NotNull] Chart chart)
    {
        List<Note> allNotesFromChart = chart.Notes.Concat(chart.HoldNotes).OrderBy(note => note.TimeMs).ToList();
        // TODO: swipe notes within a hold... that is gonna be hell lmao
        // TODO: holds with a swipe on the hold start take on the timing window of the swipe??
        chart.ProcessedNotesForGameplay.Clear();

        foreach (Note note in allNotesFromChart)
        {
            note.EarliestHitTimeMs = note.TimeMs + note.HitWindows[^1].LeftMs;
            note.LatestHitTimeMs = note.TimeMs + note.HitWindows[^1].RightMs;

            if (chart.HoldNotes.Any(holdNote =>
                    holdNote.End.ChartTick == note.ChartTick && SegmentsOverlap(holdNote.End, note)))
            {
                // Notes that overlap with a hold end should lose their early window (except Marvelous).
                note.EarliestHitTimeMs =
                    Math.Max(note.EarliestHitTimeMs.Value, note.TimeMs + note.HitWindows[0].LeftMs);
            }

            // Look backwards through the chart to see if any notes overlap with this.
            // Potential optimization: once a note is out of range (for all possible note type windows!),
            // we know it can never come back in range. We can keep track of the minimum index into the notes
            // array that we have to care about, like minNoteIndex below.
            List<Note> overlappingNotes = chart.ProcessedNotesForGameplay
                // Don't try to do anything with notes that are at exactly the same time. Furthermore, we know we
                // can't find any other notes from an earlier ChartTick, so just break out of the loop.
                .TakeWhile(otherNote => otherNote.ChartTick != note.ChartTick)
                .Where(otherNote => otherNote.LatestHitTimeMs!.Value > note.EarliestHitTimeMs.Value &&
                                    SegmentsOverlap(note, otherNote))
                .ToList();

            if (overlappingNotes.Any())
            {
                // We have overlapping timing windows. Split the difference between the two closest notes.
                Note latestNote = overlappingNotes.OrderByDescending(overlappingNote => overlappingNote.TimeMs).First();
                // TODO: If the windows of the two notes are different sizes (e.g. touch vs swipe notes), bias the split point.
                float cutoff = (latestNote.TimeMs + note.TimeMs) / 2;

                // Use Max here to avoid expanding the window here if it's already truncated.
                cutoff = Math.Max(cutoff, note.EarliestHitTimeMs.Value);

                // It is possible that latestNote has an overlapping timing window, but it is already truncated so that
                // the midpoint of the two notes is _not_ contained within its timing window. In this case we shouldn't
                // truncate the window past the latest LatestHitTimeMs of an overlapping note. Such a truncation could
                // leave some period of time that is outside any note's window.
                float latestOverlapTimeMs = overlappingNotes.Max(overlappingNote => overlappingNote.LatestHitTimeMs!.Value);
                // Note: since we know that latestNote.LatestHitTimeMs > note.EarliestHitTimeMs, this still preserves
                // cutoff >= latestNote.LatestHitTime > note.EarliestHitTimeMs.
                cutoff = Math.Min(cutoff, latestOverlapTimeMs);

                note.EarliestHitTimeMs = cutoff;
                foreach (Note otherNote in overlappingNotes)
                    // Use Math.Min here to avoid expanding any windows - keep in mind that just because the windows
                    // overlap, we don't know that the calculated cutoff will actually be within the window.
                    otherNote.LatestHitTimeMs = Math.Min(otherNote.LatestHitTimeMs!.Value, cutoff);
            }

            chart.ProcessedNotesForGameplay.Add(note);
        }
    }
}
}