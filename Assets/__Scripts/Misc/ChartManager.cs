using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using UnityEngine;
using SaturnGame.Settings;
using SaturnGame.Loading;

namespace SaturnGame.RhythmGame
{
    public class ChartManager : PersistentSingleton<ChartManager>
    {
        public AudioClip bgmClip;
        public Chart chart;

        private List<Gimmick> bpmGimmicks = new();
        private List<Gimmick> timeSigGimmicks = new();

        // Any modifications to Loading or LoadedChart should hold the lock.
        public readonly object LoadingLock = new();
        public bool Loading { get; private set; } = false;
        public string LoadedChart { get; private set; } = null;
        private int readerIndex = 0;

        /// <summary>
        /// Parses a <c>.mer</c> file and creates lists of objects from it.
        /// </summary>
        /// <param name="merStream"></param>
        /// <returns></returns>
        public async Task<bool> LoadChart(string filepath)
        {
            lock(LoadingLock)
            {
                if (Loading)
                {
                    Debug.LogWarning("A chart is already loading. Stopping to avoid multiple loads running at the same time.");
                    return false;
                }
                else
                {
                    Loading = true;
                }
            }

            try
            {
                if (!File.Exists(filepath)) return false;

                FileStream fileStream = new(filepath, FileMode.Open, FileAccess.Read);
                List<string> merFile = MerLoader.LoadMer(fileStream);

                readerIndex = 0;
                bpmGimmicks.Clear();
                timeSigGimmicks.Clear();

                chart = new();

                await Task.Run(() => ParseMetadata(merFile));
                await Task.Run(() => ParseChart(merFile));
                await Task.Run(() => GenerateBarLines());
                await Task.Run(() => CreateBgmData());
                await Task.Run(() => CreateHiSpeedData());
                await Task.Run(() => SetTime());

                if (chart.reverseGimmicks.Count != 0)
                    await Task.Run(() => GenerateReverseLists());

                if (SettingsManager.Instance.PlayerSettings.GameSettings.MirrorNotes != 0)
                    await Task.Run(() => MirrorChart());

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
                lock (LoadingLock)
                {
                    if (!Loading)
                    {
                        Debug.LogError("Incorrect loading state.");
                    }
                    Loading = false;
                }
            }
        }


        /// <summary>
        /// Loops through a .mer file's metadata tags until it<br />
        /// either finds a <c>#BODY</c> tag or runs out of lines to parse.
        /// </summary>
        /// <param name="merFile"></param>
        private void ParseMetadata(List<string> merFile)
        {
            if (merFile == null) return;

            do
            {
                string merLine = merFile[readerIndex];

                string tempDifficultyString = MerLoader.GetMetadata(merLine, "#DIFFICULTY ");
                if (tempDifficultyString != null) chart.difficulty = Convert.ToSingle(tempDifficultyString);

                string tempClearThresholdString = MerLoader.GetMetadata(merLine, "#CLEAR_THRESHOLD");
                if (tempClearThresholdString != null) chart.clearThreshold = Convert.ToSingle(tempClearThresholdString);

                string tempAudioOffsetString = MerLoader.GetMetadata(merLine, "#OFFSET ");
                if (tempAudioOffsetString != null) chart.audioOffset = Convert.ToSingle(tempAudioOffsetString);

                string tempMovieOffsetString = MerLoader.GetMetadata(merLine, "#MOVIEOFFSET ");
                if (tempMovieOffsetString != null) chart.movieOffset = Convert.ToSingle(tempMovieOffsetString);

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
            Note tempNote;
            Note lastNote = null; // make lastNote start as null so the compiler doesn't scream
            Gimmick tempGimmick;

            for (int i = readerIndex; i < merFile.Count; i++)
            {
                if (string.IsNullOrEmpty(merFile[i])) continue;

                string[] splitLine = merFile[i].Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries);

                int measure = Convert.ToInt32(splitLine[0]);
                int tick = Convert.ToInt32(splitLine[1]);
                int objectID = Convert.ToInt32(splitLine[2]);

                // Invalid ID
                if (objectID == 0) continue;

                // Note
                if (objectID == 1)
                {
                    int noteTypeID = Convert.ToInt32(splitLine[3]);

                    // end of chart note
                    if (noteTypeID is 14)
                    {
                        chart.endOfChart = new(measure, tick);
                        continue;
                    }

                    // skip hold segment and hold end. they're handled separately.
                    if (noteTypeID is 10 or 11) continue;

                    int position = Convert.ToInt32(splitLine[5]);
                    int size = Convert.ToInt32(splitLine[6]);

                    // hold notes
                    if (noteTypeID is 9 or 25)
                    {
                        HoldSegment holdStart = new HoldSegment(measure, tick, position, size, true);
                        // .... it ain't pretty but it does the job. I hope.
                        // start another loop that begins at the hold start
                        // and looks for a referenced note.
                        int referencedNoteIndex = Convert.ToInt32(splitLine[8]);
                        List<HoldSegment> holdSegments = new() { holdStart };

                        for (int j = i; j < merFile.Count; j++)
                        {
                            string[] tempSplitLine = merFile[j].Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries);

                            int tempNoteIndex = Convert.ToInt32(tempSplitLine[4]);

                            // found the next referenced note
                            if (tempNoteIndex == referencedNoteIndex)
                            {
                                int tempMeasure = Convert.ToInt32(tempSplitLine[0]);
                                int tempTick = Convert.ToInt32(tempSplitLine[1]);
                                int tempNoteTypeID = Convert.ToInt32(tempSplitLine[3]);
                                int tempPosition = Convert.ToInt32(tempSplitLine[5]);
                                int tempSize = Convert.ToInt32(tempSplitLine[6]);
                                bool tempRenderFlag = Convert.ToInt32(tempSplitLine[7]) == 1;

                                HoldSegment tempSegmentNote = new(tempMeasure, tempTick, tempPosition, tempSize, tempRenderFlag);
                                holdSegments.Add(tempSegmentNote);

                                if (tempNoteTypeID == 10)
                                {
                                    referencedNoteIndex = Convert.ToInt32(tempSplitLine[8]);
                                }

                                // noteType 11 = hold end
                                if (tempNoteTypeID == 11)
                                {
                                    break;
                                }
                            }
                        }

                        HoldNote hold = new(holdSegments.ToArray());
                        hold.SetBonusTypeFromNoteID(noteTypeID);
                        tempNote = hold;
                        chart.holdNotes.Add(hold);
                    }
                    // mask notes
                    else if (noteTypeID is 12 or 13)
                    {
                        int dir = Convert.ToInt32(splitLine[8]);
                        bool add = noteTypeID == 12;
                        chart.masks.Add(new Mask(measure, tick, position, size, (Mask.MaskDirection) dir, add));
                        continue;
                    }
                    else
                    {
                        Note note = Note.CreateFromNoteID(measure, tick, noteTypeID, position, size);
                        chart.notes.Add(note);
                        tempNote = note;
                    }

                    if (tempNote is null)
                    {
                        Debug.LogError($"null noteid {noteTypeID}");
                    }

                    // all other notes
                    CheckSync(tempNote, lastNote);
                    lastNote = tempNote;
                }

                // Gimmick
                else
                {
                    // create a gimmick
                    object value1 = null;
                    object value2 = null;

                    // avoid IndexOutOfRangeExceptions :]
                    if (objectID is 3 && splitLine.Length > 4)
                    {
                        value1 = Convert.ToInt32(splitLine[3]);
                        value2 = Convert.ToInt32(splitLine[4]);
                    }

                    if (objectID is 2 or 5 && splitLine.Length > 3)
                        value1 = Convert.ToSingle(splitLine[3]);

                    tempGimmick = new Gimmick(measure, tick, objectID, value1, value2);

                    // sort gimmicks by type
                    switch (tempGimmick.Type)
                    {
                        case Gimmick.GimmickType.BeatsPerMinute:
                            bpmGimmicks.Add(tempGimmick);
                            break;
                        case Gimmick.GimmickType.TimeSignature:
                            timeSigGimmicks.Add(tempGimmick);
                            break;
                        case Gimmick.GimmickType.HiSpeed:
                            chart.hiSpeedGimmicks.Add(tempGimmick);
                            break;
                        case Gimmick.GimmickType.StopStart:
                            // Convert Stops to HiSpeed changes internally since they're functionally identical(?)
                            tempGimmick.Type = Gimmick.GimmickType.StopStart;
                            tempGimmick.HiSpeed = 0;
                            chart.hiSpeedGimmicks.Add(tempGimmick);
                            break;
                        case Gimmick.GimmickType.StopEnd:
                            // Same as above.
                            tempGimmick.Type = Gimmick.GimmickType.StopEnd;
                            tempGimmick.HiSpeed = chart.hiSpeedGimmicks.LastOrDefault(x => x.TimeMs < tempGimmick.TimeMs && x.Type is Gimmick.GimmickType.HiSpeed)?.HiSpeed ?? 1;
                            chart.hiSpeedGimmicks.Add(tempGimmick);
                            break;
                        case Gimmick.GimmickType.ReverseEffectStart:
                        case Gimmick.GimmickType.ReverseEffectEnd:
                        case Gimmick.GimmickType.ReverseNoteEnd:
                            chart.reverseGimmicks.Add(tempGimmick);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Check for common errors that may happen during a chart load.
        /// </summary>
        private (bool passed, string error) CheckLoadErrors()
        {
            // I made the checks separate to spare the next person reading this an aneyurism.
            // It's also organized from most likely to least likely, so it doesn't matter much.
            if (chart.endOfChart is null)
                return (false, "Chart is missing End of Chart note!");

            if (chart.notes.Last().TimeMs > chart.endOfChart.TimeMs)
                return (false, "Notes behind end of Chart note!");

            if (bgmClip != null && chart.notes.Last().TimeMs > bgmClip.length * 1000) // conv. to ms
                return (false, "Chart is longer than audio!");

            if (chart.bgmDataGimmicks.Count == 0)
                return (false, "Chart is missing BPM and TimeSignature data!");

            if (chart.reverseGimmicks.Count % 3 != 0) // Reverses always come in groups of 3 gimmicks. If the count is not divisible by 3 something's wrong.
                return (false, "Invalid reverse gimmicks! Every reverse must have these segments: [Effect Start] [Effect End] [Note End]");

            // Loop through all reverses to find any overlapping/out of order/broken ones.
            // The order must always be Effect Start > Effect End > Note End.
            Gimmick.GimmickType lastReverse = Gimmick.GimmickType.ReverseNoteEnd;
            for (int i = 0; i < chart.reverseGimmicks.Count; i++)
            {
                var currentReverse = chart.reverseGimmicks[i].Type;

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
            for (int i = 0; i < chart.reverseGimmicks.Count - 2; i++)
            {
                if (chart.reverseGimmicks[i].Type is not Gimmick.GimmickType.ReverseEffectStart)
                    continue;

                // If [i] is EffectStart, then [i + 1] must be EffectEnd and [i + 2] must be NoteEnd
                float effectStartTime = chart.reverseGimmicks[i].ScaledVisualTime;
                float effectEndTime = chart.reverseGimmicks[i + 1].ScaledVisualTime;
                float noteEndTime = chart.reverseGimmicks[i + 2].ScaledVisualTime;

                List<Note> notesToReverse = chart.notes.Where(x => x.ScaledVisualTime >= effectEndTime && x.ScaledVisualTime < noteEndTime).ToList();
                List<HoldNote> holdsToReverse = chart.holdNotes.Where(x => x.Start.ScaledVisualTime >= effectEndTime && x.End.ScaledVisualTime < noteEndTime).ToList();

                // TODO: reverse syncs?

                foreach (Note note in notesToReverse)
                    ReverseNote(note, effectStartTime, effectEndTime, noteEndTime);

                foreach (HoldNote hold in holdsToReverse)
                    ReverseHold(hold, effectStartTime, effectEndTime, noteEndTime);

                // List.Reverse() from Linq
                chart.reverseHoldNotes.Reverse();
            }
        }

        /// <summary>
        /// Creates a copy of a Note, remaps it's position in time, <br />
        /// then adds a copy of it to <c>reverseNotes</c>
        /// </summary>
        private void ReverseNote(Note note, float startTime, float midTime, float endTime)
        {
            Note copy = (Note) note.Clone();
            copy.ReverseTime(startTime, midTime, endTime);

            chart.reverseNotes.Insert(0, copy);
        }

        /// <summary>
        /// Reverses a Hold Note by creating a deep copy of it, <br />
        /// then remapping each segment note's position in time.
        /// </summary>
        private void ReverseHold(HoldNote hold, float startTime, float midTime, float endTime)
        {
            HoldNote copy = HoldNote.DeepCopy(hold);
            copy.ReverseTime(startTime, midTime, endTime);



            chart.reverseHoldNotes.Add(copy);
        }

        /// <summary>
        /// Generates a Bar Line every Measure.
        /// </summary>
        private void GenerateBarLines()
        {
            for (int i = 0; i <= chart.endOfChart.Measure; i++)
                chart.barLines.Add(new BarLine(i, 0));
        }

        /// <summary>
        /// Check if the last parsed note is on the same timestamp as the current note. <br />
        /// This should efficiently and cleanly detect any simultaneous notes.
        /// </summary>
        private void CheckSync(Note current, Note last)
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
        private void GenerateSync(Note note0, Note note1)
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
            chart.syncs.Add(sync);
        }

        /// <summary>
        /// Mirrors a note along an axis.
        /// </summary>
        /// <remarks>
        /// Axis 30 = horizontal mirror <br/>
        /// Axis 0 = vertical mirror
        /// </remarks>
        private void MirrorObject(PositionedChartElement note, int axis = 30)
        {
            int newPos = SaturnMath.Modulo(axis - note.Size - note.Position, 60);

            if (note is SwipeNote swipeNote)
            {
                switch (swipeNote.Direction)
                {
                    case SwipeNote.SwipeDirection.Clockwise:
                        swipeNote.Direction = SwipeNote.SwipeDirection.Counterclockwise;
                        break;
                    case SwipeNote.SwipeDirection.Counterclockwise:
                        swipeNote.Direction = SwipeNote.SwipeDirection.Clockwise;
                        break;
                }
            }

            note.Position = newPos;
        }

        /// <summary>
        /// Mirrors an entire chart.
        /// </summary>
        private void MirrorChart()
        {
            foreach (Note note in chart.notes)
                MirrorObject(note);

            foreach (Mask mask in chart.masks)
                MirrorObject(mask);

            foreach (SyncIndicator sync in chart.syncs)
                MirrorObject(sync);

            foreach (HoldNote hold in chart.holdNotes)
            {
                foreach (HoldSegment note in hold.Notes)
                    MirrorObject(note);
            }

            foreach (Note note in chart.reverseNotes)
                MirrorObject(note);

            foreach (HoldNote hold in chart.reverseHoldNotes)
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
            chart.bgmDataGimmicks = bpmGimmicks.Concat(timeSigGimmicks).OrderBy(x => x.ChartTick).ToList();

            chart.bgmDataGimmicks[0].BeatsPerMinute = lastBpm;
            chart.bgmDataGimmicks[0].TimeSig = lastTimeSig;

            int lastTick = 0;

            List<Gimmick> obsoleteGimmicks = new();

            for (int i = 1; i < chart.bgmDataGimmicks.Count; i++)
            {
                int currentTick = chart.bgmDataGimmicks[i].ChartTick;

                // Handles two gimmicks at the same time, in case a chart changes
                // BeatsPerMinute and TimeSignature simultaneously.
                if (currentTick == lastTick)
                {
                    // if this is a bpm change, then last change must've been a time sig change.
                    if (chart.bgmDataGimmicks[i].Type is Gimmick.GimmickType.BeatsPerMinute)
                    {
                        chart.bgmDataGimmicks[i - 1].BeatsPerMinute = chart.bgmDataGimmicks[i].BeatsPerMinute;
                        lastBpm = chart.bgmDataGimmicks[i].BeatsPerMinute;
                    }
                    if (chart.bgmDataGimmicks[i].Type is Gimmick.GimmickType.TimeSignature)
                    {
                        chart.bgmDataGimmicks[i - 1].TimeSig = chart.bgmDataGimmicks[i].TimeSig;
                        lastTimeSig = chart.bgmDataGimmicks[i].TimeSig;
                    }

                    // send gimmick to list for removal later
                    obsoleteGimmicks.Add(chart.bgmDataGimmicks[i]);
                    continue;
                }

                if (chart.bgmDataGimmicks[i].Type is Gimmick.GimmickType.BeatsPerMinute)
                {
                    chart.bgmDataGimmicks[i].TimeSig = lastTimeSig;
                    lastBpm = chart.bgmDataGimmicks[i].BeatsPerMinute;
                }

                if (chart.bgmDataGimmicks[i].Type is Gimmick.GimmickType.TimeSignature)
                {
                    chart.bgmDataGimmicks[i].BeatsPerMinute = lastBpm;
                    lastTimeSig = chart.bgmDataGimmicks[i].TimeSig;
                }

                lastTick = currentTick;
            }

            // clear obsolete gimmicks
            foreach (Gimmick gimmick in obsoleteGimmicks)
                chart.bgmDataGimmicks.Remove(gimmick);

            obsoleteGimmicks.Clear();

            chart.bgmDataGimmicks[0].TimeMs = 0;
            for (int i = 1; i < chart.bgmDataGimmicks.Count; i++)
            {
                float lastTime = chart.bgmDataGimmicks[i - 1].TimeMs;
                float currentMeasure = (chart.bgmDataGimmicks[i].ChartTick) * SaturnMath.tickToMeasure;
                float lastMeasure = (chart.bgmDataGimmicks[i - 1].ChartTick) * SaturnMath.tickToMeasure;
                float timeSig = chart.bgmDataGimmicks[i - 1].TimeSig.Ratio;
                float bpm = chart.bgmDataGimmicks[i - 1].BeatsPerMinute;

                float time = lastTime + ((currentMeasure - lastMeasure) * (4 * timeSig * (60000f / bpm)));
                chart.bgmDataGimmicks[i].TimeMs = time;
            }
        }

        /// <summary>
        /// Calculates scaled timestamps for HiSpeed changes.
        /// </summary>
        private void CreateHiSpeedData()
        {
            foreach (Gimmick gimmick in chart.hiSpeedGimmicks)
            {
                gimmick.CalculateTime(chart.bgmDataGimmicks);
            }

            float lastScaledTime = 0;
            float lastTime = 0;
            float lastHiSpeed = 1;

            for (int i = 0; i < chart.hiSpeedGimmicks.Count; i++)
            {
                float currentTime = chart.hiSpeedGimmicks[i].TimeMs;
                float scaledTime = lastScaledTime + (Mathf.Abs(currentTime - lastTime) * lastHiSpeed);
                chart.hiSpeedGimmicks[i].ScaledVisualTime = scaledTime;

                lastScaledTime = scaledTime;
                lastTime = chart.hiSpeedGimmicks[i].TimeMs;
                lastHiSpeed = chart.hiSpeedGimmicks[i].HiSpeed;
            }
        }

        /// <summary>
        /// Loops through every ChartObject in every list <br />
        /// and calculates the object's time in milliseconds <br />
        /// according to all BPM and TimeSignature changes.
        /// </summary>
        private void SetTime()
        {
            foreach (Note note in chart.notes)
            {
                note.CalculateTime(chart.bgmDataGimmicks);
                note.CalculateScaledTime(chart.hiSpeedGimmicks);
            }

            foreach (SyncIndicator sync in chart.syncs)
            {
                sync.CalculateTime(chart.bgmDataGimmicks);
                sync.CalculateScaledTime(chart.hiSpeedGimmicks);
            }

            foreach (BarLine obj in chart.barLines)
            {
                obj.CalculateTime(chart.bgmDataGimmicks);
                obj.CalculateScaledTime(chart.hiSpeedGimmicks);
            }

            foreach (Mask note in chart.masks)
            {
                note.CalculateTime(chart.bgmDataGimmicks);
                note.CalculateScaledTime(chart.hiSpeedGimmicks);
            }

            foreach (HoldNote hold in chart.holdNotes)
            {
                hold.CalculateTime(chart.bgmDataGimmicks);
                hold.CalculateScaledTime(chart.hiSpeedGimmicks);
            }

            foreach (Gimmick gimmick in chart.reverseGimmicks)
            {
                gimmick.CalculateTime(chart.bgmDataGimmicks);
                gimmick.CalculateScaledTime(chart.hiSpeedGimmicks);
            }

            chart.endOfChart.CalculateTime(chart.bgmDataGimmicks);
            chart.endOfChart.CalculateScaledTime(chart.hiSpeedGimmicks);
        }

    }
}
