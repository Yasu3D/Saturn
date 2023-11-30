using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
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
        public List<Gimmick> gimmicks;
        public List<Note> notes;
        public List<HoldNote> holdNotes;
        public List<Note> masks;

        public void LoadChart(Stream merStream)
        {
            if (merStream == null) return;

            List<string> merFile = GetFileFromStream(merStream);

            int index = 0;

            do
            {
                string merLine = merFile[index];

                var tempMusicFilePath = GetTag(merLine, "#MUSIC_FILE_PATH ");
                if (tempMusicFilePath != null) musicFilePath = tempMusicFilePath;

                var tempAudioOffset = GetTag(merLine, "#OFFSET ");
                if (tempAudioOffset != null) audioOffset = Convert.ToSingle(tempAudioOffset);

                var tempMovieOffset = GetTag(merLine, "#MOVIEOFFSET ");
                if (tempMovieOffset != null) movieOffset = Convert.ToSingle(tempMovieOffset);

                if (merLine.Contains("#BODY"))
                {
                    index++;
                    break;
                }
            }
            while (++index < merFile.Count);

            Note tempNote;
            Gimmick tempGimmick;

            gimmicks.Clear();
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

                if (objectID == 0) continue;

                if (objectID == 1)
                {
                    // create a note
                    int noteTypeID = Convert.ToInt32(splitLine[3]);

                    int position = Convert.ToInt32(splitLine[5]);
                    int size = Convert.ToInt32(splitLine[6]);
                    tempNote = new Note(measure, tick, noteTypeID, position, size);

                    // hold segment/end are handled separately.
                    if (noteTypeID is 10 or 11)
                    {
                        continue;
                    }

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
                        masks.Add(tempNote);
                        continue;
                    }
                    
                    // all other notes
                    notes.Add(tempNote);
                }

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
                    gimmicks.Add(tempGimmick);
                }
            }
        }

        private List<string> GetFileFromStream(Stream stream)
        {
            List<string> lines = new List<string>();
            StreamReader reader = new StreamReader(stream);
            while (!reader.EndOfStream)
                lines.Add(reader.ReadLine() ?? "");
            return lines;
        }

        private string GetTag(string input, string tag)
        {
            if (input.Contains(tag))
                return input.Substring(input.IndexOf(tag, StringComparison.Ordinal) + tag.Length);

            return null;;
        }



        // DELETE THIS ON SIGHT THANKS
        void Start()
        {
            string filepath = Path.Combine(Application.streamingAssetsPath, "/Songpacks/DONOTSHIP/test.mer");
            if (File.Exists(filepath))
            {
                FileStream fileStream = new(filepath, FileMode.Open, FileAccess.Read);
                LoadChart(fileStream);
            }
        }
    }
}
