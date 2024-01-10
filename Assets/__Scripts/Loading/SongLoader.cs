using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using SaturnGame.Data;

namespace SaturnGame.Loading
{
    public class SongLoader : MonoBehaviour
    {
        public List<SongData> songs;

        void Awake()
        {
            LoadSongs();
        }

        public void LoadSongs()
        {
            string songpacksPath = Path.Combine(Application.streamingAssetsPath, "Songpacks");
            var songDirectories = Directory.EnumerateFiles(songpacksPath, "meta.mer", SearchOption.AllDirectories);

            foreach(var filepath in songDirectories)
            {
                FileStream metaStream = new(filepath, FileMode.Open, FileAccess.Read);
                List<string> metaFile = MerLoader.LoadMer(metaStream);

                string folderPath = Path.GetDirectoryName(filepath);
                string title = "";
                string artist = "";
                string bpm = "";
                string jacketPath = "";

                int readerIndex = 0;
                do
                {
                    string metaLine = metaFile[readerIndex];

                    string tempTitle = MerLoader.GetMetadata(metaLine, "#TITLE");
                    if (tempTitle != null) title = tempTitle;

                    string tempArtist = MerLoader.GetMetadata(metaLine, "#ARTIST");
                    if (tempArtist != null) artist = tempArtist;

                    string tempBpm = MerLoader.GetMetadata(metaLine, "#BPM");
                    if (tempBpm != null) bpm = tempBpm;
                    
                    string tempJacketPath = MerLoader.GetMetadata(metaLine, "#JACKET_FILE_PATH");
                    if (tempJacketPath != null) jacketPath = Path.Combine(folderPath, tempJacketPath);
                }
                while (++readerIndex < metaFile.Count);

                songs.Add(new(title, artist, bpm, folderPath, jacketPath));
            }
        }
    }
}