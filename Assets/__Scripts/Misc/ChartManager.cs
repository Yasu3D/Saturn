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
    public class ChartManager : PersistentSingleton<ChartManager>
    {
        public AudioClip BGMClip;
        public Chart Chart;

        private readonly List<Gimmick> bpmGimmicks = new();
        private readonly List<Gimmick> timeSigGimmicks = new();

        // Any modifications to Loading or LoadedChart should hold the lock.
        private readonly object loadingLock = new();
        public bool Loading { get; private set; }
        public string LoadedChart { get; private set; }
        private int readerIndex;

        public ScoreData LastScoreData;

        /// <summary>
        /// Parses a <c>.mer</c> file and creates lists of objects from it.
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public bool LoadChart(string filepath)
        {
            lock(loadingLock)
            {
                if (Loading)
                {
                    Debug.LogWarning("A chart is already loading. Stopping to avoid multiple loads running at the same time.");
                    return false;
                }

                Loading = true;
            }

            try
            {
                if (!File.Exists(filepath)) return false;

                FileStream fileStream = new(filepath, FileMode.Open, FileAccess.Read);
                List<string> merFile = MerLoader.LoadMer(fileStream);

                readerIndex = 0;
                bpmGimmicks.Clear();
                timeSigGimmicks.Clear();

                Chart = new();

                ParseMetadata(merFile);
                ParseChart(merFile);
                GenerateBarLines();
                CreateBgmData();
                CreateHiSpeedData();
                SetTime();

                if (Chart.ReverseGimmicks.Count != 0)
                    GenerateReverseLists();

                if (SettingsManager.Instance.PlayerSettings.GameSettings.MirrorNotes != 0)
                    MirrorChart();

                if (!CheckLoadErrors().passed)
                {
                    Debug.LogError($"Chart load failed! | {CheckLoadErrors().error}");
                    return false;
                }

                LoadedChart = filepath;

                // TODO: Remove this
                Debug.Log("[Chart Load] Chart loaded successfully!");
                return true;
            }
            finally
            {
                lock (loadingLock)
                {
                    if (!Loading)
                        Debug.LogError("Incorrect loading state.");

                    Loading = false;
                }
            }
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
                if (tempDifficultyString != null) Chart.Difficulty = Convert.ToSingle(tempDifficultyString, CultureInfo.InvariantCulture);

                string tempClearThresholdString = MerLoader.GetMetadata(merLine, "#CLEAR_THRESHOLD");
                if (tempClearThresholdString != null) Chart.ClearThreshold = Convert.ToSingle(tempClearThresholdString, CultureInfo.InvariantCulture);

                string tempAudioOffsetString = MerLoader.GetMetadata(merLine, "#OFFSET ");
                if (tempAudioOffsetString != null) Chart.AudioOffset = Convert.ToSingle(tempAudioOffsetString, CultureInfo.InvariantCulture);

                string tempMovieOffsetString = MerLoader.GetMetadata(merLine, "#MOVIEOFFSET ");
                if (tempMovieOffsetString != null) Chart.MovieOffset = Convert.ToSingle(tempMovieOffsetString, CultureInfo.InvariantCulture);

                if (merLine.Contains("#BODY"))
                {
                    readerIndex++;
                    break;
                }
            }
            while (++readerIndex < merFile.Count);
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

                string[] splitLine = merFile[i].Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries);

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
                            Chart.EndOfChart = new(measure, tick);
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
                                Chart.HoldNotes.Add(hold);
                                break;
                            }
                            case 10 or 11:
                            {
                                int noteId = Convert.ToInt32(splitLine[4], CultureInfo.InvariantCulture);
                                bool renderFlag = Convert.ToInt32(splitLine[7], CultureInfo.InvariantCulture) == 1;
                                int? nextNoteId = noteTypeID == 10 ? Convert.ToInt32(splitLine[8], CultureInfo.InvariantCulture) : null;
                                allHoldSegments.Add(noteId, (new HoldSegment(measure, tick, position, size, renderFlag), nextNoteId));
                                continue; // don't proceed to sync check for hold segments
                            }
                            // mask notes
                            case 12 or 13:
                            {
                                int dir = Convert.ToInt32(splitLine[8], CultureInfo.InvariantCulture);
                                bool add = noteTypeID == 12;
                                Chart.Masks.Add(new Mask(measure, tick, position, size, (Mask.MaskDirection) dir, add));
                                continue;
                            }
                            default:
                            {
                                Note note = Note.CreateFromNoteID(measure, tick, noteTypeID, position, size);
                                Chart.Notes.Add(note);
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
                                Chart.HiSpeedGimmicks.Add(tempGimmick);
                                break;
                            }
                            case Gimmick.GimmickType.StopStart:
                            {
                                // Convert Stops to HiSpeed changes internally since they're functionally identical(?)
                                tempGimmick.Type = Gimmick.GimmickType.StopStart;
                                tempGimmick.HiSpeed = 0;
                                Chart.HiSpeedGimmicks.Add(tempGimmick);
                                break;
                            }
                            case Gimmick.GimmickType.StopEnd:
                            {
                                // Same as above.
                                tempGimmick.Type = Gimmick.GimmickType.StopEnd;
                                tempGimmick.HiSpeed = Chart.HiSpeedGimmicks.LastOrDefault(x => x.ChartTick < tempGimmick.ChartTick && x.Type is Gimmick.GimmickType.HiSpeed)?.HiSpeed ?? 1;
                                Chart.HiSpeedGimmicks.Add(tempGimmick);
                                break;
                            }
                            case Gimmick.GimmickType.ReverseEffectStart:
                            case Gimmick.GimmickType.ReverseEffectEnd:
                            case Gimmick.GimmickType.ReverseNoteEnd:
                            {
                                Chart.ReverseGimmicks.Add(tempGimmick);
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
            if (Chart.EndOfChart is null)
                return (false, "Chart is missing End of Chart note!");

            if (Chart.Notes.Last().TimeMs > Chart.EndOfChart.TimeMs)
                return (false, "Notes behind end of Chart note!");

            // this is fine actually lol
            //if (bgmClip != null && chart.notes.Last().TimeMs > bgmClip.length * 1000) // conv. to ms
            //    return (false, "Chart is longer than audio!");

            if (Chart.BGMDataGimmicks.Count == 0)
                return (false, "Chart is missing BPM and TimeSignature data!");

            if (SaturnMath.Modulo(Chart.ReverseGimmicks.Count, 3) != 0) // Reverses always come in groups of 3 gimmicks. If the count is not divisible by 3 something's wrong.
                return (false, "Invalid reverse gimmicks! Every reverse must have these segments: [Effect Start] [Effect End] [Note End]");

            // Loop through all reverses to find any overlapping/out of order/broken ones.
            // The order must always be Effect Start > Effect End > Note End.
            Gimmick.GimmickType lastReverse = Gimmick.GimmickType.ReverseNoteEnd;
            foreach (Gimmick gimmick in Chart.ReverseGimmicks)
            {
                Gimmick.GimmickType currentReverse = gimmick.Type;

                if ((currentReverse is Gimmick.GimmickType.ReverseEffectStart && lastReverse is not Gimmick.GimmickType.ReverseNoteEnd) ||
                    (currentReverse is Gimmick.GimmickType.ReverseEffectEnd && lastReverse is not Gimmick.GimmickType.ReverseEffectStart) ||
                    (currentReverse is Gimmick.GimmickType.ReverseNoteEnd && lastReverse is not Gimmick.GimmickType.ReverseEffectEnd))
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
            for (int i = 0; i < Chart.ReverseGimmicks.Count - 2; i++)
            {
                if (Chart.ReverseGimmicks[i].Type is not Gimmick.GimmickType.ReverseEffectStart)
                    continue;

                // If [i] is EffectStart, then [i + 1] must be EffectEnd and [i + 2] must be NoteEnd
                float effectStartTime = Chart.ReverseGimmicks[i].ScaledVisualTime;
                float effectEndTime = Chart.ReverseGimmicks[i + 1].ScaledVisualTime;
                float noteEndTime = Chart.ReverseGimmicks[i + 2].ScaledVisualTime;

                List<Note> notesToReverse = Chart.Notes.Where(x => x.ScaledVisualTime >= effectEndTime && x.ScaledVisualTime < noteEndTime).ToList();
                List<HoldNote> holdsToReverse = Chart.HoldNotes.Where(x => x.Start.ScaledVisualTime >= effectEndTime && x.End.ScaledVisualTime < noteEndTime).ToList();

                // TODO: reverse syncs?

                foreach (Note note in notesToReverse)
                    ReverseNote(note, effectStartTime, effectEndTime, noteEndTime);

                foreach (HoldNote hold in holdsToReverse)
                    ReverseHold(hold, effectStartTime, effectEndTime, noteEndTime);

                // List.Reverse() from Linq
                Chart.ReverseHoldNotes.Reverse();
            }
        }

        /// <summary>
        /// Creates a copy of a Note, remaps it's position in time, <br />
        /// then adds a copy of it to <c>reverseNotes</c>
        /// </summary>
        private void ReverseNote([NotNull] Note note, float startTime, float midTime, float endTime)
        {
            Note copy = (Note) note.Clone();
            copy.ReverseTime(startTime, midTime, endTime);

            Chart.ReverseNotes.Insert(0, copy);
        }

        /// <summary>
        /// Reverses a Hold Note by creating a deep copy of it, <br />
        /// then remapping each segment note's position in time.
        /// </summary>
        private void ReverseHold([NotNull] HoldNote hold, float startTime, float midTime, float endTime)
        {
            HoldNote copy = HoldNote.DeepCopy(hold);
            copy.ReverseTime(startTime, midTime, endTime);

            Chart.ReverseHoldNotes.Add(copy);
        }

        /// <summary>
        /// Generates a Bar Line every Measure.
        /// </summary>
        private void GenerateBarLines()
        {
            for (int i = 0; i <= Chart.EndOfChart.Measure; i++)
                Chart.BarLines.Add(new BarLine(i, 0));
        }

        /// <summary>
        /// Check if the last parsed note is on the same timestamp as the current note. <br />
        /// This should efficiently and cleanly detect any simultaneous notes.
        /// </summary>
        private void CheckSync(Note current, [CanBeNull] Note last)
        {
            if (last == null) return;
            if (last is ChainNote || current is ChainNote) return;

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
            int size0 = SaturnMath.Modulo(note1.Position - position0, 60) + 1;  // pos + 1 // shift - 1

            int position1 = SaturnMath.Modulo(note1.Position + note1.Size - 1, 60); // pos + 1 // size  - 2
            int size1 = SaturnMath.Modulo(note0.Position - position1, 60) + 1;  // pos + 1 // shift - 1

            int finalPosition = size0 > size1 ? position1 : position0;
            int finalSize = Mathf.Min(size0, size1);

            if (finalSize > 30) return;

            SyncIndicator sync = new(measure, tick, finalPosition, finalSize);
            Chart.Syncs.Add(sync);
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
            foreach (Note note in Chart.Notes)
                MirrorObject(note);

            foreach (Mask mask in Chart.Masks)
                MirrorObject(mask);

            foreach (SyncIndicator sync in Chart.Syncs)
                MirrorObject(sync);

            foreach (HoldNote hold in Chart.HoldNotes)
            {
                foreach (HoldSegment note in hold.Notes)
                    MirrorObject(note);
            }

            foreach (Note note in Chart.ReverseNotes)
                MirrorObject(note);

            foreach (HoldNote hold in Chart.ReverseHoldNotes)
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
            IOrderedEnumerable<Gimmick> bpmAndTimeSigGimmicks = bpmGimmicks.Concat(timeSigGimmicks).OrderBy(x => x.ChartTick);

            Chart.BGMDataGimmicks = new();

            foreach (Gimmick gimmick in bpmAndTimeSigGimmicks)
            {
                Gimmick gimmickToUpdate;
                switch (Chart.BGMDataGimmicks.LastOrDefault())
                {
                    case Gimmick lastGimmick when lastGimmick.ChartTick == gimmick.ChartTick:
                    {
                        gimmickToUpdate = lastGimmick;
                        break;
                    }
                    default:
                    {
                        Gimmick newGimmick = new(gimmick.Measure, gimmick.Tick, lastBpm, lastTimeSig);
                        Chart.BGMDataGimmicks.Add(newGimmick);
                        gimmickToUpdate = newGimmick;
                        break;
                    }
                }

                switch (gimmick)
                {
                    case {Type: Gimmick.GimmickType.BeatsPerMinute, BeatsPerMinute: var bpm}:
                    {
                        gimmickToUpdate.BeatsPerMinute = bpm;
                        lastBpm = gimmick.BeatsPerMinute;
                        break;
                    }
                    case {Type: Gimmick.GimmickType.TimeSignature, TimeSig: var timeSig}:
                    {
                        gimmickToUpdate.TimeSig = timeSig;
                        lastTimeSig = timeSig;
                        break;
                    }
                }
            }

            Chart.BGMDataGimmicks[0].TimeMs = 0;
            for (int i = 1; i < Chart.BGMDataGimmicks.Count; i++)
            {
                float lastTime = Chart.BGMDataGimmicks[i - 1].TimeMs;
                float currentMeasure = Chart.BGMDataGimmicks[i].ChartTick * SaturnMath.TickToMeasure;
                float lastMeasure = Chart.BGMDataGimmicks[i - 1].ChartTick * SaturnMath.TickToMeasure;
                float timeSig = Chart.BGMDataGimmicks[i - 1].TimeSig.Ratio;
                float bpm = Chart.BGMDataGimmicks[i - 1].BeatsPerMinute;

                float time = lastTime + (currentMeasure - lastMeasure) * (4 * timeSig * (60000f / bpm));
                Chart.BGMDataGimmicks[i].TimeMs = time;
            }
        }

        /// <summary>
        /// Calculates scaled timestamps for HiSpeed changes.
        /// </summary>
        private void CreateHiSpeedData()
        {
            foreach (Gimmick gimmick in Chart.HiSpeedGimmicks)
                gimmick.CalculateTime(Chart.BGMDataGimmicks);

            float lastScaledTime = 0;
            float lastTime = 0;
            float lastHiSpeed = 1;

            foreach (Gimmick gimmick in Chart.HiSpeedGimmicks)
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
            foreach (Note note in Chart.Notes)
            {
                note.CalculateTime(Chart.BGMDataGimmicks);
                note.CalculateScaledTime(Chart.HiSpeedGimmicks);
            }

            foreach (SyncIndicator sync in Chart.Syncs)
            {
                sync.CalculateTime(Chart.BGMDataGimmicks);
                sync.CalculateScaledTime(Chart.HiSpeedGimmicks);
            }

            foreach (BarLine obj in Chart.BarLines)
            {
                obj.CalculateTime(Chart.BGMDataGimmicks);
                obj.CalculateScaledTime(Chart.HiSpeedGimmicks);
            }

            foreach (Mask note in Chart.Masks)
            {
                note.CalculateTime(Chart.BGMDataGimmicks);
                note.CalculateScaledTime(Chart.HiSpeedGimmicks);
            }

            foreach (HoldNote hold in Chart.HoldNotes)
            {
                hold.CalculateTime(Chart.BGMDataGimmicks);
                hold.CalculateScaledTime(Chart.HiSpeedGimmicks);
            }

            foreach (Gimmick gimmick in Chart.ReverseGimmicks)
            {
                gimmick.CalculateTime(Chart.BGMDataGimmicks);
                gimmick.CalculateScaledTime(Chart.HiSpeedGimmicks);
            }

            Chart.EndOfChart.CalculateTime(Chart.BGMDataGimmicks);
            Chart.EndOfChart.CalculateScaledTime(Chart.HiSpeedGimmicks);
        }
    }
}
