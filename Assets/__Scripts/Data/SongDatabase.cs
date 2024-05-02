using System.Collections.Generic;
using UnityEngine;
using System.IO;
using SaturnGame.Loading;
using System;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;

namespace SaturnGame.Data
{
public class SongDatabase : MonoBehaviour
{
    [NotNull] public static string SongPacksPath => Path.Combine(Application.streamingAssetsPath, "SongPacks");
    public List<Song> Songs;

    public void LoadAllSongData()
    {
        string songPacksPath = SongPacksPath;
        Directory.CreateDirectory(songPacksPath);
        IEnumerable<string> songDirectories = Directory.EnumerateFiles(songPacksPath, "meta.mer", SearchOption.AllDirectories);

        foreach (string filepath in songDirectories) Songs.Add(LoadSongData(filepath));
    }

    [NotNull]
    public static Song LoadSongData([NotNull] string metaMerPath)
    {
        FileStream metaStream = new(metaMerPath, FileMode.Open, FileAccess.Read);
        List<string> metaFile = MerLoader.LoadMer(metaStream);

        string folderPath = Path.GetDirectoryName(metaMerPath);
        string jacketPath = Directory.GetFiles(folderPath).FirstOrDefault(file =>
        {
            string filename = Path.GetFileNameWithoutExtension(file);
            string extension = Path.GetExtension(file);

            return filename == "jacket" && extension is ".png" or ".jpg" or ".jpeg";
        });

        string title = "";
        string rubi = "";
        string artist = "";
        string bpm = "";
        SongDifficulty[] diffs = new SongDifficulty[5];

        int readerIndex = 0;
        do
        {
            string metaLine = metaFile[readerIndex];

            string tempTitle = MerLoader.GetMetadata(metaLine, "#TITLE");
            if (tempTitle != null) title = tempTitle;

            string tempRubi = MerLoader.GetMetadata(metaLine, "#RUBI_TITLE");
            if (tempRubi != null) rubi = tempRubi;

            string tempArtist = MerLoader.GetMetadata(metaLine, "#ARTIST");
            if (tempArtist != null) artist = tempArtist;

            string tempBpm = MerLoader.GetMetadata(metaLine, "#BPM");
            if (tempBpm != null) bpm = tempBpm;
        } while (++readerIndex < metaFile.Count);

        string[] merIDs = { "0.mer", "1.mer", "2.mer", "3.mer", "4.mer" };

        for (int i = 0; i < 5; i++)
        {
            string chartFilepath = Path.Combine(folderPath, merIDs[i]);
            diffs[i].Difficulty = (Difficulty)i;

            if (!File.Exists(chartFilepath))
            {
                diffs[i].Exists = false;
                continue;
            }

            FileStream merStream = new(chartFilepath, FileMode.Open, FileAccess.Read);
            List<string> merFile = MerLoader.LoadMer(merStream);

            diffs[i].Exists = true;
            diffs[i].ChartFilepath = chartFilepath;

            // Fallback preview in case it's not found in the .mer file.
            // PreviewStart will default to 0f already.
            diffs[i].PreviewDuration = 10f;

            readerIndex = 0;
            do
            {
                string merLine = merFile[readerIndex];

                if (merLine == "#BODY") break;

                string tempLevel = MerLoader.GetMetadata(merLine, "#LEVEL");
                if (tempLevel != null)
                    diffs[i].Level = Convert.ToDecimal(tempLevel, CultureInfo.InvariantCulture);

                string tempAudioFilepath = MerLoader.GetMetadata(merLine, "#AUDIO");
                if (tempAudioFilepath != null)
                    diffs[i].AudioFilepath = Path.Combine(folderPath, tempAudioFilepath);

                string tempAudioOffset = MerLoader.GetMetadata(merLine, "#OFFSET");
                if (tempAudioOffset != null)
                    diffs[i].AudioOffset = Convert.ToSingle(tempAudioOffset, CultureInfo.InvariantCulture);

                string tempCharter = MerLoader.GetMetadata(merLine, "#AUTHOR");
                if (tempCharter != null)
                    diffs[i].Charter = tempCharter;

                string tempPreviewStart = MerLoader.GetMetadata(merLine, "#PREVIEW_TIME");
                if (!string.IsNullOrEmpty(tempPreviewStart))
                    diffs[i].PreviewStart = Convert.ToSingle(tempPreviewStart, CultureInfo.InvariantCulture);

                // #PREVIEW_DURATION is correct, but #PREVIEW_LENGTH is used in some charts due to
                // https://github.com/muskit/WacK-Repackager/issues/5
                string tempPreviewDuration = MerLoader.GetMetadata(merLine, "#PREVIEW_DURATION") ??
                                             MerLoader.GetMetadata(merLine, "#PREVIEW_LENGTH");
                if (!string.IsNullOrEmpty(tempPreviewDuration))
                    diffs[i].PreviewDuration = Convert.ToSingle(tempPreviewDuration, CultureInfo.InvariantCulture);
            } while (++readerIndex < merFile.Count);
        }

        return new Song(title, rubi, artist, bpm, folderPath, jacketPath, diffs);
    }
}
}
