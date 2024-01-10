using UnityEngine;

namespace SaturnGame.Data
{
    [System.Serializable]
    public class SongData
    {
        public SongData(string title, string artist, string bpm, string folderPath,  string jacketPath)
        {
            this.title = title;
            this.artist = artist;
            this.bpm = bpm;
            this.folderPath = folderPath;
            this.jacketPath = jacketPath;
        }

        public Texture2D jacket;
        public string title;
        public string artist;
        public string bpm;
        public string folderPath;
        public string jacketPath;

        /*public string GetDifficultyString()
        {
            return ((int)difficulty).ToString() + (difficulty % 1 > 0.6f ? "+" : "");
        }

        public string GetDifficultyString(float difficulty)
        {
            return ((int)difficulty).ToString() + (difficulty % 1 > 0.6f ? "+" : "");
        }*/
    }
}
