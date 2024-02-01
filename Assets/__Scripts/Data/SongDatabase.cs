using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using SaturnGame.Loading;
using System;

namespace SaturnGame.Data
{
    public class SongDatabase : MonoBehaviour
    {
        public List<SongData> songs;

        public void LoadAllSongData()
        {
            string songpacksPath = Path.Combine(Application.streamingAssetsPath, "Songpacks");
            var songDirectories = Directory.EnumerateFiles(songpacksPath, "meta.mer", SearchOption.AllDirectories);

            foreach(var filepath in songDirectories)
            {
                LoadSongData(filepath);
            }
        }
    
        public void LoadSongData(string path)
        {
            FileStream metaStream = new(path, FileMode.Open, FileAccess.Read);
            List<string> metaFile = MerLoader.LoadMer(metaStream);

            string folderPath = Path.GetDirectoryName(path);
            string title = "";
            string rubi = "";
            string artist = "";
            string bpm = "";
            string jacketPath = "";
            SongDifficulty[] diffs = new SongDifficulty[5];

            int readerIndex = 0;
            do
            {
                string metaLine = metaFile[readerIndex];

                string tempTitle = MerLoader.GetMetadata(metaLine, "#TITLE");
                if (tempTitle != null) title = tempTitle;

                string tempRubi = MerLoader.GetMetadata(metaLine, "#RUBI");
                if (tempRubi != null) rubi = tempRubi;

                string tempArtist = MerLoader.GetMetadata(metaLine, "#ARTIST");
                if (tempArtist != null) artist = tempArtist;

                string tempBpm = MerLoader.GetMetadata(metaLine, "#BPM");
                if (tempBpm != null) bpm = tempBpm;
                
                string tempJacketPath = MerLoader.GetMetadata(metaLine, "#JACKET_FILE_PATH");
                if (tempJacketPath != null) jacketPath = Path.Combine(folderPath, tempJacketPath);
            }
            while (++readerIndex < metaFile.Count);

            string[] merIDs = {"00.mer", "01.mer", "02.mer", "03.mer", "04.mer"};

            for (int i = 0; i < 5; i++)
            {
                string chartFilepath = Path.Combine(folderPath, merIDs[i]);
                diffs[i].diffName = (DifficultyName) i;

                if (!File.Exists(chartFilepath))
                {
                    diffs[i].exists = false;
                    continue;
                }

                FileStream merStream = new(chartFilepath, FileMode.Open, FileAccess.Read);
                List<string> merFile = MerLoader.LoadMer(merStream);

                diffs[i].exists = true;
                diffs[i].chartFilepath = chartFilepath;

                readerIndex = 0;
                do
                {
                    string merLine = merFile[readerIndex];

                    Debug.Log(merLine);

                    if (merLine == "#BODY") break;

                    string tempLevel = MerLoader.GetMetadata(merLine, "#LEVEL");
                    if (tempLevel != null) diffs[i].diffLevel = Convert.ToSingle(tempLevel);

                    string tempAudioFilepath = MerLoader.GetMetadata(merLine, "#MUSIC_FILE_PATH");
                    if (tempAudioFilepath != null) diffs[i].audioFilepath = Path.Combine(folderPath, tempAudioFilepath);

                    string tempAudioOffset = MerLoader.GetMetadata(merLine, "#OFFSET");
                    if (tempAudioOffset != null) diffs[i].audioOffset = Convert.ToSingle(tempAudioOffset);

                    string tempCharter = MerLoader.GetMetadata(merLine, "#AUTHOR");
                    if (tempCharter != null) diffs[i].charter = tempCharter;

                    string tempPreviewStart = MerLoader.GetMetadata(merLine, "#PREVIEW_TIME");
                    if (tempPreviewStart != null) diffs[i].previewStart = Convert.ToSingle(tempPreviewStart);
                    if (tempPreviewStart == "") diffs[i].previewStart = 0;

                    string tempPreviewDuration = MerLoader.GetMetadata(merLine, "#PREVIEW_DURATION");
                    if (tempPreviewDuration != null) diffs[i].previewDuration = Convert.ToSingle(tempPreviewDuration);
                    if (tempPreviewDuration == "") diffs[i].previewDuration = 10;
                }
                while (++readerIndex < merFile.Count);
            }

            songs.Add(new(title, rubi, artist, bpm, folderPath, jacketPath, diffs));
        }
    }
}