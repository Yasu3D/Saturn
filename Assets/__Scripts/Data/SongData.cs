using UnityEngine;

namespace SaturnGame.Data
{
    [System.Serializable]
    public class SongData
    {
        public SongData(string title, string rubi, string artist, string bpm, string folderPath,  string jacketPath, SongDifficulty[] songDiffs)
        {
            this.title = title;
            this.rubi = rubi;
            this.artist = artist;
            this.bpm = bpm;
            this.folderPath = folderPath;
            this.jacketPath = jacketPath;
            this.songDiffs = songDiffs;
        }

        public string title;
        public string rubi;
        public string artist;
        public string bpm;
        public string folderPath;
        public string jacketPath;
        public SongDifficulty[] songDiffs;
    }
}
