using System;

namespace SaturnGame.Data
{
[Serializable]
public class Song
{
    public Song(string title, string rubi, string artist, string bpm, string folderPath, string jacketPath,
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
    // SongDiffs[0] should ALWAYS have Difficulty = Normal, [1] = Hard, ...and so on
    // SongDiffs should ALWAYS have 5 elements
    public SongDifficulty[] SongDiffs;
}
}