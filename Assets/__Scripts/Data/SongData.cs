using System;

namespace SaturnGame.Data
{
    [Serializable]
    public class SongData
    {
        public SongData(string title, string rubi, string artist, string bpm, string folderPath, string jacketPath,
            SongDifficulty[] songDiffs)
        {
            Title = title;
            Rubi = rubi;
            Artist = artist;
            Bpm = bpm;
            FolderPath = folderPath;
            JacketPath = jacketPath;
            SongDiffs = songDiffs;
        }

        public string Title;
        public string Rubi;
        public string Artist;
        public string Bpm;
        public string FolderPath;
        public string JacketPath;
        public SongDifficulty[] SongDiffs;
    }
}
