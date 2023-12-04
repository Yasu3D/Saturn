using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SaturnGame.RhythmGame
{
    public class Chart : MonoBehaviour
    {
        // ==== METADATA =======
        [Header("METADATA")]
        public string musicFilePath = "";
        public float audioOffset = 0;
        public float movieOffset = 0;

        // ==== NOTES ==========
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

        private const float tickToMeasure = 0.00052083333333333333f; // = 1/1920

        public void LoadChart(Stream merStream)
        {
            if (merStream == null) return;

            List<string> merFile = GetFileFromStream(merStream);

            int index = 0;

            do
            {
                string merLine = merFile[index];

                var tempMusicFilePath = GetMetadata(merLine, "#MUSIC_FILE_PATH ");
                if (tempMusicFilePath != null) musicFilePath = tempMusicFilePath;

                var tempAudioOffset = GetMetadata(merLine, "#OFFSET ");
                if (tempAudioOffset != null) audioOffset = Convert.ToSingle(tempAudioOffset);

                var tempMovieOffset = GetMetadata(merLine, "#MOVIEOFFSET ");
                if (tempMovieOffset != null) movieOffset = Convert.ToSingle(tempMovieOffset);

                if (merLine.Contains("#BODY"))
                {
                    index++;
                    break;
                }
            }
            while (++index < merFile.Count);

            Note tempNote;
            Note lastNote = null; // make lastNote start as null so the compiler doesn't scream
            Gimmick tempGimmick;

            beatsPerMinuteGimmicks.Clear();
            timeSignatureGimmicks.Clear();

            bgmDataGimmicks.Clear();
            hiSpeedGimmicks.Clear();
            stopGimmicks.Clear();
            reverseGimmicks.Clear();

            notes.Clear();
            holdNotes.Clear();
            masks.Clear();

            for (int i = index; i < merFile.Count; i++)
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
                        case ObjectEnums.GimmickType.StopEnd:
                            stopGimmicks.Add(tempGimmick);
                            break;
                        case ObjectEnums.GimmickType.ReverseEffectStart:
                        case ObjectEnums.GimmickType.ReverseEffectEnd:
                        case ObjectEnums.GimmickType.ReverseNoteEnd:
                            reverseGimmicks.Add(tempGimmick);
                            break;
                    }
                }
            }

            CreateBgmDataGimmicks();
            SetTime();
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
        /// Check if the last parsed note is on the same timestamp as the current note. <br />
        /// This should efficiently and cleanly detect any simultaneous notes.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="last"></param>
        private void CheckSync(Note current, Note last)
        {
            if (last == null) return;

            if (current.Measure == last.Measure && current.Tick == last.Tick)
            {
                last.IsSync = true;
                current.IsSync = true;
            }
        }

        /// <summary>
        /// A rather bulky function that ""cleanly"" merges BeatsPerMinuteGimmicks <br />
        /// and TimeSignatureGimmicks into one list. My only excuse for the bulk <br />
        /// is that most charts don't have many BPM/TimeSig changes.
        /// </summary>
        private void CreateBgmDataGimmicks()
        {
            if (beatsPerMinuteGimmicks.Count == 0 || timeSignatureGimmicks.Count == 0) return;

            float lastBpm = beatsPerMinuteGimmicks[0].BeatsPerMinute;
            TimeSignature lastTimeSig = timeSignatureGimmicks[0].TimeSig;

            // merge both lists and sort by timestamp
            bgmDataGimmicks = beatsPerMinuteGimmicks.Concat(timeSignatureGimmicks).OrderBy(x => x.Measure * 1920 + x.Tick).ToList();
            
            bgmDataGimmicks[0].BeatsPerMinute = lastBpm;
            bgmDataGimmicks[0].TimeSig = lastTimeSig;

            int lastTime = 0;

            for (int i = 1; i < bgmDataGimmicks.Count; i++)
            {
                int currentTime = bgmDataGimmicks[i].Measure * 1920 + bgmDataGimmicks[i].Tick;

                // Handles two gimmicks at the same time, in case a chart changes
                // BeatsPerMinute and TimeSignature simultaneously.
                if (lastTime == currentTime)
                {
                    // if this is a bpm change, then last change must've been a time sig change.
                    if (bgmDataGimmicks[i].GimmickType is ObjectEnums.GimmickType.BeatsPerMinute)
                        bgmDataGimmicks[i - 1].BeatsPerMinute = bgmDataGimmicks[i].BeatsPerMinute;
                    else
                        bgmDataGimmicks[i - 1].TimeSig = bgmDataGimmicks[i].TimeSig;

                    // remove duplicate gimmick 
                    bgmDataGimmicks.RemoveAt(i);
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

                lastTime = currentTime;
            }

            bgmDataGimmicks[0].Time = 0;
            for (int i = 1; i < bgmDataGimmicks.Count; i++)
            {
                float currentMeasure = (bgmDataGimmicks[i].Measure * 1920 + bgmDataGimmicks[i].Tick) * tickToMeasure;
                float lastMeasure = (bgmDataGimmicks[i - 1].Measure * 1920 + bgmDataGimmicks[i - 1].Tick) * tickToMeasure;
                float timeSig = bgmDataGimmicks[i - 1].TimeSig.Ratio;
                float bpm = bgmDataGimmicks[i - 1].BeatsPerMinute;

                float bgmTime = (currentMeasure - lastMeasure) * (4 * timeSig * (60000f / bpm));

                bgmDataGimmicks[i].Time = bgmTime;
            }
        }

        // this function makes me want to shoot myself
        private void SetTime()
        {
            foreach (Note note in notes)
            {
                note.Time = CalculateTime(note);
            }

            foreach (Note note in masks)
            {
                note.Time = CalculateTime(note);
            }

            // especially this makes me want to shoot myself
            foreach (HoldNote hold in holdNotes)
            {
                foreach (Note note in hold.Notes)
                {
                    note.Time = CalculateTime(note);
                }
            }

            // maybe TODO move this somewhere else. Need to figure out how to handle hispeed first.
            foreach (Gimmick gimmick in hiSpeedGimmicks)
            {
                gimmick.Time = CalculateTime(gimmick);
            }

            foreach (Gimmick gimmick in stopGimmicks)
            {
                gimmick.Time = CalculateTime(gimmick);
            }

            foreach (Gimmick gimmick in reverseGimmicks)
            {
                gimmick.Time = CalculateTime(gimmick);
            }
        }

        private float CalculateTime(ChartObject chartObject)
        {
            int timeStamp = chartObject.Measure * 1920 + chartObject.Tick;

            Gimmick lastBgmData = bgmDataGimmicks.LastOrDefault(x => x.Measure * 1920 + x.Tick < timeStamp) ?? bgmDataGimmicks[0];

            int lastTimestamp = lastBgmData.Measure * 1920 + lastBgmData.Tick;

            float tickTime = 60000.0f / lastBgmData.BeatsPerMinute * 4 * lastBgmData.TimeSig.Ratio * (timeStamp - lastTimestamp) + lastBgmData.Time;
            return tickTime * tickToMeasure;
        }

        // DELETE THIS ON SIGHT THANKS
        void Start()
        {
            string filepath = Path.Combine(Application.streamingAssetsPath, "SongPacks/DONOTSHIP/test.mer");
            if (File.Exists(filepath))
            {
                FileStream fileStream = new(filepath, FileMode.Open, FileAccess.Read);
                LoadChart(fileStream);
            }
            else Debug.Log("File not found");

            ChartObject testObject = new()
            {
                Measure = 1,
                Tick = 0
            };
            
            Debug.Log(CalculateTime(testObject));
        }
    }
}
