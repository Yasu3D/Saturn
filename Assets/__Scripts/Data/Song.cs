using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

namespace SaturnGame.Data
{
[Serializable]
public class Song
{
    public Song(string title, string rubi, string artist, string bpm, string folderPath, string jacketPath,
        [NotNull] SongDifficulty[] songDiffs)
    {
        Title = title;
        Rubi = rubi;
        Artist = artist;
        Bpm = bpm;
        FolderPath = folderPath;
        JacketPath = jacketPath;
        SongDiffs = new();
        foreach (SongDifficulty songDiff in songDiffs) SongDiffs[songDiff.Difficulty] = songDiff;
    }

    public string Title;
    public string Rubi;
    public string Artist;
    public string Bpm;
    public string FolderPath;
    public string JacketPath;
    public Dictionary<Difficulty, SongDifficulty> SongDiffs;

    // From the SongPacksPath to the dir containing the song folder
    // E.g. SongPacks/PackName/subdir/mysong/meta.mer -> "PackName/subdir"
    [NotNull]
    public string ContainingFolder
    {
        get
        {
            // +1 for trailing dir separator
            int trimLength = SongDatabase.SongPacksPath.Length + 1;
            string relativePath = FolderPath[trimLength..];
            return Path.GetDirectoryName(relativePath) ?? "";
        }
    }
}
}
