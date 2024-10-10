using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using SaturnGame.Settings;
using UnityEngine;

namespace SaturnGame.RhythmGame.Loading
{
public class ChartLoader
{
    public static Chart LoadChart(string filepath)
    {
        try
        {
            string[] data = File.ReadLines(filepath).ToArray();
            if (data.Length == 0) throw new("Chart File is empty!");

            // Naively detect .SAT format
            if (data[0].Contains("@SAT_VERSION"))
            {
                return SatLoader.LoadChart(filepath, data);
            }
            else
            {
                return MerLoader.LoadChart(data);
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }
    
    public static bool ContainsTag(string input, string tag, out string result)
    {
        if (input.Contains(tag))
        {
            result = input[(input.IndexOf(tag, StringComparison.Ordinal) + tag.Length)..].Trim();
            return true;
        }

        result = "";
        return false;
    }
}

internal static class SatLoader
{
    public static Chart LoadChart(string filepath, string[] data)
    {
        return new();
    }
}

internal static class MerLoader
{
    public static Chart LoadChart(string[] data)
    {
        int contentSeparator = Array.IndexOf(data, "#BODY");
        if (contentSeparator == -1) throw new("#BODY declaration not found!");

        string[] metadata = data[..contentSeparator];
        string[] content = data[(contentSeparator + 1)..];

        Chart chart = new();
        List<Gimmick> bpmGimmicks = new();
        List<Gimmick> timeSigGimmicks = new();
        
        parseMetadata();
        parseContent();
        ChartPostProcessing.GenerateBarLines(chart);
        ChartPostProcessing.GenerateBgmData(chart, bpmGimmicks, timeSigGimmicks);
        ChartPostProcessing.GenerateHiSpeedData(chart);
        ChartPostProcessing.SetTime(chart);
        ChartPostProcessing.ProcessHitWindows(chart);
        
        if (chart.ReverseGimmicks.Count != 0) ChartPostProcessing.GenerateReverseLists(chart);
        if (SettingsManager.Instance != null && SettingsManager.Instance.PlayerSettings.GameSettings.MirrorNotes != 0) ChartPostProcessing.MirrorChart(chart);
        
        return chart;

        void parseMetadata()
        {
            foreach (string line in metadata)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                string result;
                
                if (ChartLoader.ContainsTag(line, "#DIFFICULTY ", out result)) chart.Level = Convert.ToSingle(result, CultureInfo.InvariantCulture);
                if (ChartLoader.ContainsTag(line, "#LEVEL ", out result)) chart.Level = Convert.ToSingle(result, CultureInfo.InvariantCulture);
                if (ChartLoader.ContainsTag(line, "#CLEAR_THRESHOLD ", out result)) chart.ClearThreshold = Convert.ToSingle(result, CultureInfo.InvariantCulture);
                if (ChartLoader.ContainsTag(line, "#OFFSET ", out result)) chart.AudioOffset = Convert.ToSingle(result, CultureInfo.InvariantCulture);
                if (ChartLoader.ContainsTag(line, "#MOVIEOFFSET ", out result)) chart.MovieOffset = Convert.ToSingle(result, CultureInfo.InvariantCulture);
                
                //if (ChartLoader.ContainsTag(line, "#AUDIO ", out result)) chart.BgmFilepath = Path.Combine(Path.GetDirectoryName(chart.Filepath) ?? "", result);
                //if (ChartLoader.ContainsTag(line, "#AUTHOR ", out result)) chart.Author = result;
                //if (ChartLoader.ContainsTag(line, "#PREVIEW_TIME ", out result)) chart.PreviewStart = Convert.ToDecimal(result, CultureInfo.InvariantCulture);
                //if (ChartLoader.ContainsTag(line, "#PREVIEW_LENGTH ", out result)) chart.PreviewTime = Convert.ToDecimal(result, CultureInfo.InvariantCulture);
            }
        }

        void parseContent()
        {
            Note lastNote = null; // make lastNote start as null so the compiler doesn't scream
            Gimmick tempGimmick;
            // (incomplete HoldNote, next segment noteId)
            List<(HoldNote, int)> incompleteHoldNotes = new();
            // noteId -> (segment, next segment noteId)
            Dictionary<int, (HoldSegment, int?)> allHoldSegments = new();

            foreach (string line in content)
            {
                if (string.IsNullOrEmpty(line)) continue;

                string[] split = line.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                int measure = Convert.ToInt32(split[0], CultureInfo.InvariantCulture);
                int tick = Convert.ToInt32(split[1], CultureInfo.InvariantCulture);
                int objectID = Convert.ToInt32(split[2], CultureInfo.InvariantCulture);

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
                        int noteTypeID = Convert.ToInt32(split[3], CultureInfo.InvariantCulture);

                        // end of chart note
                        if (noteTypeID is 14)
                        {
                            chart.EndOfChart = new(measure, tick);
                            continue;
                        }

                        int position = Convert.ToInt32(split[5], CultureInfo.InvariantCulture);
                        int size = Convert.ToInt32(split[6], CultureInfo.InvariantCulture);
                        int noteId = Convert.ToInt32(split[4], CultureInfo.InvariantCulture);

                        Note tempNote;

                        switch (noteTypeID)
                        {
                            // hold notes
                            case 9 or 25:
                            {
                                HoldSegment holdStart = new(measure, tick, position, size, true);
                                int nextNoteID = Convert.ToInt32(split[8], CultureInfo.InvariantCulture);

                                HoldNote hold = new(holdStart, noteId);
                                hold.SetBonusTypeFromNoteID(noteTypeID);
                                incompleteHoldNotes.Add((hold, nextNoteID));

                                tempNote = hold;
                                chart.HoldNotes.Add(hold);
                                break;
                            }
                            case 10 or 11:
                            {
                                bool renderFlag = Convert.ToInt32(split[7], CultureInfo.InvariantCulture) == 1;
                                int? nextNoteId = noteTypeID == 10
                                    ? Convert.ToInt32(split[8], CultureInfo.InvariantCulture)
                                    : null;
                                allHoldSegments.Add(noteId, (new(measure, tick, position, size, renderFlag), nextNoteId));
                                continue; // don't proceed to sync check for hold segments
                            }
                            // mask notes
                            case 12 or 13:
                            {
                                int dir = Convert.ToInt32(split[8], CultureInfo.InvariantCulture);
                                bool add = noteTypeID == 12;
                                chart.Masks.Add(new(measure, tick, position, size, (Mask.MaskDirection)dir, add));
                                continue;
                            }
                            default:
                            {
                                Note note = Note.CreateFromNoteTypeID(measure, tick, noteTypeID, position, size, noteId);
                                chart.Notes.Add(note);
                                tempNote = note;
                                break;
                            }
                        }

                        if (tempNote is null)
                            Debug.LogError($"null noteid {noteTypeID}");

                        // all other notes
                        ChartPostProcessing.CheckSync(chart, tempNote, lastNote);
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
                            if (split.Length == 4)
                            {
                                value1 = Convert.ToInt32(split[3], CultureInfo.InvariantCulture);
                                value2 = 4; // yeah.......... this happens in the wild
                            }
                            else if (split.Length > 4)
                            {
                                value1 = Convert.ToInt32(split[3], CultureInfo.InvariantCulture);
                                value2 = Convert.ToInt32(split[4], CultureInfo.InvariantCulture);
                            }
                        }

                        if (objectID is 2 or 5 && split.Length > 3)
                            value1 = Convert.ToSingle(split[3], CultureInfo.InvariantCulture);

                        tempGimmick = new(measure, tick, objectID, value1, value2);

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
    }
}

internal static class ChartPostProcessing
{
    /// <summary>
    /// Check if the last parsed note is on the same timestamp as the current note. <br />
    /// This should efficiently and cleanly detect any simultaneous notes.
    /// </summary>
    internal static void CheckSync(Chart chart, Note note0, Note note1)
    {
        if (note1 == null) return;
        if ((note1 is ChainNote && note1.BonusType != Note.NoteBonusType.RNote) || (note0 is ChainNote && note0.BonusType != Note.NoteBonusType.RNote)) return;
        if ((note1 is HoldNote || note0 is HoldNote) && note1.Position == note0.Position && note1.Size == note0.Size) return;

        if (note0.Measure == note1.Measure && note0.Tick == note1.Tick)
        {
            note1.IsSync = true;
            note0.IsSync = true;

            generateSync();
        }

        return;

        void generateSync()
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
    }
    
    /// <summary>
    /// Generates a Bar Line every Measure.
    /// </summary>
    internal static void GenerateBarLines(Chart chart)
    {
        for (int i = 0; i <= chart.EndOfChart.Measure; i++)
            chart.BarLines.Add(new(i, 0));
    }
    
    /// <summary>
    /// A rather bulky function that ""cleanly"" merges BeatsPerMinuteGimmicks <br />
    /// and TimeSignatureGimmicks into one list. My only excuse for the bulk <br />
    /// is that most charts don't have many BPM/TimeSig changes.
    /// </summary>
    internal static void GenerateBgmData(Chart chart, List<Gimmick> bpmGimmicks, List<Gimmick> timeSigGimmicks)
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
    internal static void GenerateHiSpeedData(Chart chart)
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
    internal static void SetTime(Chart chart)
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
    
    /// <summary>
    /// Mirrors an entire chart.
    /// </summary>
    internal static void MirrorChart(Chart chart)
    {
        foreach (Note note in chart.Notes) MirrorObject(note);
        
        foreach (Mask mask in chart.Masks) MirrorObject(mask);
        
        foreach (SyncIndicator sync in chart.Syncs) MirrorObject(sync);

        foreach (Note note in chart.ReverseNotes) MirrorObject(note);
        
        foreach (HoldNote hold in chart.HoldNotes)
        {
            foreach (HoldSegment note in hold.Notes) MirrorObject(note);
        }

        foreach (HoldNote hold in chart.ReverseHoldNotes)
        {
            foreach (HoldSegment note in hold.Notes) MirrorObject(note);
        }
    }
    
    /// <summary>
    /// Mirrors a note along an axis.
    /// </summary>
    /// <remarks>
    /// Axis 30 = horizontal mirror <br/>
    /// Axis 0 = vertical mirror
    /// </remarks>
    private static void MirrorObject(PositionedChartElement note, int axis = 30)
    {
        int newPos = SaturnMath.Modulo(axis - note.Size - note.Position, 60);

        if (note is SwipeNote swipeNote)
        {
            swipeNote.Direction = swipeNote.Direction switch
            {
                SwipeNote.SwipeDirection.Clockwise => SwipeNote.SwipeDirection.Counterclockwise,
                SwipeNote.SwipeDirection.Counterclockwise => SwipeNote.SwipeDirection.Clockwise,
                _ => swipeNote.Direction,
            };
        }

        note.Position = newPos;
    }
    
    /// <summary>
    /// Generates a List of Notes for reverse gimmicks.
    /// </summary>
    /// <param name="chart"></param>
    internal static void GenerateReverseLists(Chart chart)
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
                ReverseNote(chart, note, effectStartTime, effectEndTime, noteEndTime);

            foreach (HoldNote hold in holdsToReverse)
                ReverseHold(chart, hold, effectStartTime, effectEndTime, noteEndTime);

            // List.Reverse() from Linq
            chart.ReverseHoldNotes.Reverse();
        }
    }
    
    /// <summary>
    /// Creates a copy of a Note, remaps its position in time, <br />
    /// then adds a copy of it to <c>reverseNotes</c>
    /// </summary>
    private static void ReverseNote(Chart chart, Note note, float startTime, float midTime, float endTime)
    {
        Note copy = (Note)note.Clone();
        copy.ReverseTime(startTime, midTime, endTime);

        chart.ReverseNotes.Insert(0, copy);
    }

    /// <summary>
    /// Reverses a Hold Note by creating a deep copy of it, <br />
    /// then remapping each segment note's position in time.
    /// </summary>
    private static void ReverseHold(Chart chart, HoldNote hold, float startTime, float midTime, float endTime)
    {
        HoldNote copy = HoldNote.DeepCopy(hold);
        copy.ReverseTime(startTime, midTime, endTime);

        chart.ReverseHoldNotes.Add(copy);
    }
    
    internal static void ProcessHitWindows(Chart chart)
    {
        List<Note> allNotesFromChart = chart.Notes.Concat(chart.HoldNotes).OrderBy(note => note.TimeMs).ToList();
        // TODO: swipe notes within a hold --- Window stays unchanged! - Yasu
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
    
    internal static bool SegmentsOverlap(PositionedChartElement note1, PositionedChartElement note2)
    {
        if (note1.Left == note1.Right || note2.Left == note2.Right)
        {
            // Full circle notes always overlap
            return true;
        }

        // Bonus reading: https://fgiesen.wordpress.com/2015/09/24/intervals-in-modular-arithmetic/
        // Each note is a half-open interval in mod60.

        // See half_open_intervals_overlap in the link.
        // We know that the interval size is positive, so we don't need to worry about that case.
        return pointWithinNote(note1, note2.Left) || pointWithinNote(note2, note1.Left);

        // See point_in_half_open_interval in the link.
        // This returns true iff point is in the half-open interval [note.Left, note.Right) (mod 60)
        // TODO: move to SaturnMath
        bool pointWithinNote(PositionedChartElement note, int point)
        {
            return SaturnMath.Modulo(point - note.Left, 60) < SaturnMath.Modulo(note.Right - note.Left, 60);
        }
    }
}
}