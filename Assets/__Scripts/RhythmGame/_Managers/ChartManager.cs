using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine;
using SaturnGame.Settings;

namespace SaturnGame.RhythmGame
{
    public class ChartManager : MonoBehaviour
    {
        [Header("REFERENCES")]
        [SerializeField] private BgmManager bgmManager;

        [Header("METADATA")]
        public string musicFilePath = "";
        public float audioOffset = 0;
        public float movieOffset = 0;

        [Header("NOTES")]
        [SerializeField] private List<Gimmick> beatsPerMinuteGimmicks;
        [SerializeField] private List<Gimmick> timeSignatureGimmicks;
        [Space(10)]
        public List<Gimmick> bgmDataGimmicks; // BgmData = BeatsPerMinute *and* TimeSignature.
        public List<Gimmick> hiSpeedGimmicks;
        public List<Gimmick> stopGimmicks;
        public List<Gimmick> reverseGimmicks;
        public List<Note> notes;
        public List<HoldNote> holdNotes;
        public List<Note> masks;
        public List<ChartObject> barLines;
        public List<Note> syncs;
        public ChartObject endOfChart;

        private int readerIndex = 0;


        /// <summary>
        /// Parses a <c>.mer</c> file and creates lists of objects from it.
        /// </summary>
        /// <param name="merStream"></param>
        /// <returns></returns>
        public async Task<bool> LoadChart(Stream merStream)
        {
            if (merStream == null)
            {
                Debug.LogWarning("[Chart Load] File stream is null! Aborting...");
                return false;
            }

            List<string> merFile = GetFileFromStream(merStream);

            Debug.Log("[Chart Load] Clearing lists...");
            beatsPerMinuteGimmicks.Clear();
            timeSignatureGimmicks.Clear();
            bgmDataGimmicks.Clear();
            hiSpeedGimmicks.Clear();
            stopGimmicks.Clear();
            reverseGimmicks.Clear();
            notes.Clear();
            holdNotes.Clear();
            masks.Clear();

            Debug.Log("[Chart Load] Resetting reader index...");
            readerIndex = 0;

            Debug.Log("[Chart Load] Parsing Metadata...");
            await Task.Run(() => ParseMetadata(merFile));
            Debug.Log("[Chart Load] Parsing Chart...");
            await Task.Run(() => ParseChart(merFile));
            Debug.Log("[Chart Load] Generating Bar Lines...");
            await Task.Run(() => GenerateBarLines());
            Debug.Log("[Chart Load] Creating BgmData...");
            await Task.Run(() => CreateBgmData());
            Debug.Log("[Chart Load] Creating HiSpeedData...");
            await Task.Run(() => CreateHiSpeedData());
            Debug.Log("[Chart Load] Calculating Time...");
            await Task.Run(() => SetTime());

            if (SettingsManager.Instance.PlayerSettings.GameSettings.MirrorNotes)
            {
                Debug.Log("[Chart Load] Mirroring Chart...");
                await Task.Run(() => MirrorChart());
            }

            Debug.Log("[Chart Load] Checking for errors...");

            if (CheckLoadErrors().passed)
            {
                Debug.Log("[Chart Load] Chart loaded successfully!");
                return true;
            }

            Debug.LogError($"Chart load failed! | {CheckLoadErrors().error}");
            return false;
        }



        /// <summary>
        /// Converts a Stream into a List of strings for parsing.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        private List<string> GetFileFromStream(Stream stream)
        {
            List<string> lines = new List<string>();
            StreamReader reader = new StreamReader(stream);
            while (!reader.EndOfStream)
                lines.Add(reader.ReadLine() ?? "");
            return lines;
        }


        /// <summary>
        /// Parses Metadata tags like "#OFFSET"
        /// </summary>
        /// <param name="input"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        private string GetMetadata(string input, string tag)
        {
            if (input.Contains(tag))
                return input.Substring(input.IndexOf(tag, StringComparison.Ordinal) + tag.Length);

            return null;;
        }


        /// <summary>
        /// Loops through a .mer file's metadata tags until it<br />
        /// either finds a <c>#BODY</c> tag or runs out of lines to parse.
        /// </summary>
        /// <param name="merFile"></param>
        /// <param name="readerIndex"></param>
        private void ParseMetadata(List<string> merFile)
        {
            if (merFile == null) return;

            do
            {
                string merLine = merFile[readerIndex];

                var tempMusicFilePath = GetMetadata(merLine, "#MUSIC_FILE_PATH ");
                if (tempMusicFilePath != null) musicFilePath = tempMusicFilePath;

                var tempAudioOffset = GetMetadata(merLine, "#OFFSET ");
                if (tempAudioOffset != null) audioOffset = Convert.ToSingle(tempAudioOffset);

                var tempMovieOffset = GetMetadata(merLine, "#MOVIEOFFSET ");
                if (tempMovieOffset != null) movieOffset = Convert.ToSingle(tempMovieOffset);

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
        /// <param name="readerIndex"></param>
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
                        endOfChart = new(measure, tick);
                        continue;
                    }

                    // skip hold segment and hold end. they're handled separately.
                    if (noteTypeID is 10 or 11) continue;

                    int position = Convert.ToInt32(splitLine[5]);
                    int size = Convert.ToInt32(splitLine[6]);

                    tempNote = new Note(measure, tick, noteTypeID, position, size);

                    CheckSync(tempNote, lastNote);

                    // hold notes
                    if (noteTypeID is 9 or 25)
                    {
                        // .... it ain't pretty but it does the job. I hope.
                        // start another loop that begins at the hold start
                        // and looks for a referenced note.
                        int referencedNoteIndex = Convert.ToInt32(splitLine[8]);
                        List<Note> holdSegments = new() { tempNote };

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

                                Note tempSegmentNote = new(tempMeasure, tempTick, tempNoteTypeID, tempPosition, tempSize, tempRenderFlag);
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
                        holdNotes.Add(hold);
                        
                        continue;
                    }

                    // mask notes
                    if (noteTypeID is 12 or 13)
                    {
                        int dir = Convert.ToInt32(splitLine[8]);
                        tempNote.MaskDirection = (ObjectEnums.MaskDirection) dir;
                        masks.Add(tempNote);
                        continue;
                    }
                    
                    // all other notes
                    lastNote = tempNote;
                    notes.Add(tempNote);
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

                    tempGimmick = new Gimmick (measure, tick, objectID, value1, value2);
                    
                    // sort gimmicks by type
                    switch (tempGimmick.GimmickType)
                    {
                        case ObjectEnums.GimmickType.BeatsPerMinute:
                            beatsPerMinuteGimmicks.Add(tempGimmick);
                            break;
                        case ObjectEnums.GimmickType.TimeSignature:
                            timeSignatureGimmicks.Add(tempGimmick);
                            break;
                        case ObjectEnums.GimmickType.HiSpeed:
                            hiSpeedGimmicks.Add(tempGimmick);
                            break;
                        case ObjectEnums.GimmickType.StopStart:
                            //stopGimmicks.Add(tempGimmick);
                            // Convert Stops to 0/1 HiSpeed changes internally since they're functionally identical(?)
                            tempGimmick.GimmickType = ObjectEnums.GimmickType.HiSpeed;
                            tempGimmick.HiSpeed = 0;
                            hiSpeedGimmicks.Add(tempGimmick);
                            break;
                        case ObjectEnums.GimmickType.StopEnd:
                            //stopGimmicks.Add(tempGimmick);
                            // Same as above.
                            tempGimmick.GimmickType = ObjectEnums.GimmickType.HiSpeed;
                            tempGimmick.HiSpeed = 1;
                            hiSpeedGimmicks.Add(tempGimmick);
                            break;
                        case ObjectEnums.GimmickType.ReverseEffectStart:
                        case ObjectEnums.GimmickType.ReverseEffectEnd:
                        case ObjectEnums.GimmickType.ReverseNoteEnd:
                            reverseGimmicks.Add(tempGimmick);
                            break;
                    }
                }
            }
        }


        /// <summary>
        /// Check for common errors that may happen during a chart load.
        /// </summary>
        /// <returns></returns>
        private (bool passed, string error) CheckLoadErrors()
        {
            // I made the checks separate to spare the next person reading this an aneyurism.
            // It's also organized from most likely to least likely, so it doesn't matter much.
            if (bgmManager.bgmClip is null)
                return (false, "BgmClip not found!");

            if (endOfChart is null)
                return (false, "Chart is missing End of Chart note!");

            if (notes.Last().Time > bgmManager.bgmClip.length * 1000) // conv. to ms
                return (false, "Chart is longer than audio!");

            if (musicFilePath is "")
                return (false, "Music file path is missing!");

            if (bgmDataGimmicks.Count == 0)
                return (false, "Chart is missing BPM and TimeSignature data!");

            return (true, "");
        }



        /// <summary>
        /// Generates a Bar Line every Measure.
        /// </summary>
        private void GenerateBarLines()
        {
            for (int i = 0; i <= endOfChart.Measure; i++)
                barLines.Add(new ChartObject(i, 0));
        }


        /// <summary>
        /// Check if the last parsed note is on the same timestamp as the current note. <br />
        /// This should efficiently and cleanly detect any simultaneous notes.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="last"></param>
        private void CheckSync(Note current, Note last)
        {
            if (last == null) return;
            if (last.NoteType is ObjectEnums.NoteType.MaskAdd or ObjectEnums.NoteType.MaskRemove) return;
            if (current.NoteType is ObjectEnums.NoteType.MaskAdd or ObjectEnums.NoteType.MaskRemove) return;

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
            
            int position0 = SaturnMath.Modulo(note0.Position + note0.Size - 1, 60); // pos + 1 // size  - 2
            int size0 = SaturnMath.Modulo(note1.Position - position0, 60) + 1;  // pos + 1 // shift - 1

            int position1 = SaturnMath.Modulo(note1.Position + note1.Size - 1, 60); // pos + 1 // size  - 2
            int size1 = SaturnMath.Modulo(note0.Position - position1, 60) + 1;  // pos + 1 // shift - 1

            int finalPosition = size0 > size1 ? position1 : position0;
            int finalSize = Mathf.Min(size0, size1);

            Note sync = new(measure, tick, ObjectEnums.NoteType.None, ObjectEnums.BonusType.None, finalPosition, finalSize);
            syncs.Add(sync);
        }


        /// <summary>
        /// Mirrors a note along an axis.
        /// </summary>
        /// <remarks>
        /// Axis 30 = horizontal mirror <br/>
        /// Axis 0 = vertical mirror
        /// </remarks>
        private void MirrorObject(Note note, int axis = 30)
        {
            int newPos = SaturnMath.Modulo(axis - note.Size - note.Position, 60);

            if (note.NoteType is ObjectEnums.NoteType.SwipeClockwise)
                note.NoteType = ObjectEnums.NoteType.SwipeCounterclockwise;

            else if (note.NoteType is ObjectEnums.NoteType.SwipeCounterclockwise)
                note.NoteType = ObjectEnums.NoteType.SwipeClockwise;

            note.Position = newPos;
        }


        /// <summary>
        /// Mirrors an entire chart.
        /// </summary>
        private void MirrorChart()
        {
            foreach (Note note in notes)
                MirrorObject(note);

            foreach (Note mask in masks)
                MirrorObject(mask);

            foreach (Note sync in syncs)
                MirrorObject(sync);

            foreach (HoldNote hold in holdNotes)
            {
                foreach (Note note in hold.Notes)
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
            if (beatsPerMinuteGimmicks.Count == 0 || timeSignatureGimmicks.Count == 0) return;

            float lastBpm = beatsPerMinuteGimmicks[0].BeatsPerMinute;
            TimeSignature lastTimeSig = timeSignatureGimmicks[0].TimeSig;

            // merge both lists and sort by timestamp
            bgmDataGimmicks = beatsPerMinuteGimmicks.Concat(timeSignatureGimmicks).OrderBy(x => x.Measure * 1920 + x.Tick).ToList();
            
            bgmDataGimmicks[0].BeatsPerMinute = lastBpm;
            bgmDataGimmicks[0].TimeSig = lastTimeSig;

            int lastTick = 0;

            List<Gimmick> obsoleteGimmicks = new();

            for (int i = 1; i < bgmDataGimmicks.Count; i++)
            {
                int currentTick = bgmDataGimmicks[i].Measure * 1920 + bgmDataGimmicks[i].Tick;

                // Handles two gimmicks at the same time, in case a chart changes
                // BeatsPerMinute and TimeSignature simultaneously.
                if (currentTick == lastTick)
                {
                    // if this is a bpm change, then last change must've been a time sig change.
                    if (bgmDataGimmicks[i].GimmickType is ObjectEnums.GimmickType.BeatsPerMinute)
                    {
                        bgmDataGimmicks[i - 1].BeatsPerMinute = bgmDataGimmicks[i].BeatsPerMinute;
                        lastBpm = bgmDataGimmicks[i].BeatsPerMinute;
                    }
                    if (bgmDataGimmicks[i].GimmickType is ObjectEnums.GimmickType.TimeSignature)
                    {
                        bgmDataGimmicks[i - 1].TimeSig = bgmDataGimmicks[i].TimeSig;
                        lastTimeSig = bgmDataGimmicks[i].TimeSig;
                    }

                    // send gimmick to list for removal later
                    obsoleteGimmicks.Add(bgmDataGimmicks[i]);
                    continue;
                }

                if (bgmDataGimmicks[i].GimmickType is ObjectEnums.GimmickType.BeatsPerMinute)
                {
                    bgmDataGimmicks[i].TimeSig = lastTimeSig;
                    lastBpm = bgmDataGimmicks[i].BeatsPerMinute;
                }

                if (bgmDataGimmicks[i].GimmickType is ObjectEnums.GimmickType.TimeSignature)
                {
                    bgmDataGimmicks[i].BeatsPerMinute = lastBpm;
                    lastTimeSig = bgmDataGimmicks[i].TimeSig;
                }

                lastTick = currentTick;
            }

            // clear obsolete gimmicks
            foreach (Gimmick gimmick in obsoleteGimmicks)
                bgmDataGimmicks.Remove(gimmick);

            obsoleteGimmicks.Clear();

            bgmDataGimmicks[0].Time = 0;
            for (int i = 1; i < bgmDataGimmicks.Count; i++)
            {
                float lastTime = bgmDataGimmicks[i - 1].Time;
                float currentMeasure = (bgmDataGimmicks[i].Measure * 1920 + bgmDataGimmicks[i].Tick) * SaturnMath.tickToMeasure;
                float lastMeasure = (bgmDataGimmicks[i - 1].Measure * 1920 + bgmDataGimmicks[i - 1].Tick) * SaturnMath.tickToMeasure;
                float timeSig = bgmDataGimmicks[i - 1].TimeSig.Ratio;
                float bpm = bgmDataGimmicks[i - 1].BeatsPerMinute;

                float time = lastTime + ((currentMeasure - lastMeasure) * (4 * timeSig * (60000f / bpm)));
                bgmDataGimmicks[i].Time = time;
            }
        }


        /// <summary>
        /// Calculates scaled timestamps for HiSpeed changes.
        /// </summary>
        private void CreateHiSpeedData()
        {
            foreach (Gimmick gimmick in hiSpeedGimmicks)
            {
                gimmick.Time = CalculateTime(gimmick);
            }

            float lastScaledTime = 0;
            float lastTime = 0;
            float lastHiSpeed = 1;
            
            for (int i = 0; i < hiSpeedGimmicks.Count; i++)
            {
                float currentTime = hiSpeedGimmicks[i].Time;
                float scaledTime = lastScaledTime + (Mathf.Abs(currentTime - lastTime) * lastHiSpeed);
                hiSpeedGimmicks[i].ScaledVisualTime = scaledTime;

                lastScaledTime = scaledTime;
                lastTime = hiSpeedGimmicks[i].Time;
                lastHiSpeed = hiSpeedGimmicks[i].HiSpeed;
            }
        }



        /// <summary>
        /// Loops through every ChartObject in every list <br />
        /// and calculates the object's time in milliseconds <br />
        /// according to all BPM and TimeSignature changes.
        /// </summary>
        private void SetTime()
        {
            foreach (Note note in notes)
            {
                note.Time = CalculateTime(note);
                note.ScaledVisualTime = CalculateScaledTime(note);
            }

            foreach (Note sync in syncs)
            {
                sync.Time = CalculateTime(sync);
                sync.ScaledVisualTime = CalculateScaledTime(sync);
            }

            foreach (ChartObject obj in barLines)
            {
                obj.Time = CalculateTime(obj);
                obj.ScaledVisualTime = CalculateScaledTime(obj);
            }

            foreach (Note note in masks)
            {
                note.Time = CalculateTime(note);
                note.ScaledVisualTime = CalculateScaledTime(note);
            }

            // especially this makes me want to shoot myself
            foreach (HoldNote hold in holdNotes)
            {
                foreach (Note note in hold.Notes)
                {
                    note.Time = CalculateTime(note);
                    note.ScaledVisualTime = CalculateScaledTime(note);
                }
            }

            foreach (Gimmick gimmick in stopGimmicks)
            {
                gimmick.Time = CalculateTime(gimmick);
                gimmick.ScaledVisualTime = CalculateScaledTime(gimmick);
            }

            foreach (Gimmick gimmick in reverseGimmicks)
            {
                gimmick.Time = CalculateTime(gimmick);
                gimmick.ScaledVisualTime = CalculateScaledTime(gimmick);
            }

            endOfChart.Time = CalculateTime(endOfChart);
            endOfChart.ScaledVisualTime = CalculateScaledTime(endOfChart);
        }


        /// <summary>
        /// Calculates the object's time in milliseconds <br />
        /// according to all BPM and TimeSignature changes.
        /// </summary>
        private float CalculateTime(ChartObject chartObject)
        {
            if (bgmDataGimmicks.Count == 0)
            {
                Debug.LogError($"Cannot calculate Time of {chartObject} because no bgmData has been set! Use CreateBgmData() to generate bgmData.");
                return -1;
            }

            int timeStamp = chartObject.Measure * 1920 + chartObject.Tick;
            Gimmick lastBgmData = bgmDataGimmicks.LastOrDefault(x => x.Measure * 1920 + x.Tick < timeStamp) ?? bgmDataGimmicks[0];

            float lastTime = lastBgmData.Time;
            float currentMeasure = (chartObject.Measure * 1920 + chartObject.Tick) * SaturnMath.tickToMeasure;
            float lastMeasure = (lastBgmData.Measure * 1920 + lastBgmData.Tick) * SaturnMath.tickToMeasure;
            float timeSig = lastBgmData.TimeSig.Ratio;
            float bpm = lastBgmData.BeatsPerMinute;

            float time = lastTime + ((currentMeasure - lastMeasure) * (4 * timeSig * (60000f / bpm)));
            return time;
        }


        /// <summary>
        /// Calculates a scaled timestamp for a chartObject. <br />
        /// <b>This is slow and should not run every frame!</b>
        /// </summary>
        /// <remarks>
        /// This function also relies on scaled HiSpeed timestamps to already be calculated. <br />
        /// Make sure <c>CreateHiSpeedData()</c> has already been called before this.
        /// </remarks>
        private float CalculateScaledTime(ChartObject chartObject)
        {
            if (hiSpeedGimmicks.Count == 0)
                return chartObject.Time;

            Gimmick lastHiSpeed = hiSpeedGimmicks.LastOrDefault(x => x.Time <= chartObject.Time);
            float hiSpeedScaledTime = lastHiSpeed != null ? lastHiSpeed.ScaledVisualTime : 0;
            float hiSpeedTime = lastHiSpeed != null ? lastHiSpeed.Time : 0;
            float hiSpeed = lastHiSpeed != null ? lastHiSpeed.HiSpeed : 1;

            float scaledTime = hiSpeedScaledTime + ((chartObject.Time - hiSpeedTime) * hiSpeed);
            return scaledTime;
        }




        // DELETE THIS ON SIGHT THANKS
        async void Update()
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                string filepath = Path.Combine(Application.streamingAssetsPath, "SongPacks/DONOTSHIP/test2.mer");
                if (File.Exists(filepath))
                {
                    FileStream fileStream = new(filepath, FileMode.Open, FileAccess.Read);
                    await LoadChart(fileStream);
                }
                else Debug.Log("File not found");
            }
        }
    }
}
