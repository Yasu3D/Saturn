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
    private readonly List<Song> songs = new();

    public void LoadAllSongData()
    {
        string songPacksPath = SongPacksPath;
        Directory.CreateDirectory(songPacksPath);
        IEnumerable<string> songDirectories =
            Directory.EnumerateFiles(songPacksPath, "meta.mer", SearchOption.AllDirectories);

        songs.Clear();
        foreach (string filepath in songDirectories) songs.Add(LoadSongData(filepath));
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
        string rubi = null;
        string artist = "";
        string bpm = "";
        List<SongDifficulty> diffs = new();

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

        for (Difficulty i = 0; i <= Difficulty.Beyond; i++)
        {
            string chartFilepath = Path.Combine(folderPath, merIDs[(int)i]);

            if (!File.Exists(chartFilepath)) continue;

            SongDifficulty diff = new()
            {
                Difficulty = i,
                LastUpdatedTime = File.GetLastWriteTime(chartFilepath),
            };

            FileStream merStream = new(chartFilepath, FileMode.Open, FileAccess.Read);
            List<string> merFile = MerLoader.LoadMer(merStream);

            diff.ChartFilepath = chartFilepath;

            // Fallback preview in case it's not found in the .mer file.
            // PreviewStart will default to 0f already.
            diff.PreviewDuration = 10f;

            readerIndex = 0;
            do
            {
                string merLine = merFile[readerIndex];

                if (merLine == "#BODY") break;

                string tempLevel = MerLoader.GetMetadata(merLine, "#LEVEL");
                if (tempLevel != null)
                    diff.Level = Convert.ToDecimal(tempLevel, CultureInfo.InvariantCulture);

                string tempAudioFilepath = MerLoader.GetMetadata(merLine, "#AUDIO");
                if (tempAudioFilepath != null)
                    diff.AudioFilepath = Path.Combine(folderPath, tempAudioFilepath);

                string tempAudioOffset = MerLoader.GetMetadata(merLine, "#OFFSET");
                if (tempAudioOffset != null)
                    diff.AudioOffset = Convert.ToSingle(tempAudioOffset, CultureInfo.InvariantCulture);

                string tempCharter = MerLoader.GetMetadata(merLine, "#AUTHOR");
                if (tempCharter != null)
                    diff.Charter = tempCharter;

                string tempPreviewStart = MerLoader.GetMetadata(merLine, "#PREVIEW_TIME");
                if (!string.IsNullOrEmpty(tempPreviewStart))
                    diff.PreviewStart = Convert.ToSingle(tempPreviewStart, CultureInfo.InvariantCulture);

                // #PREVIEW_DURATION is correct, but #PREVIEW_LENGTH is used in some charts due to
                // https://github.com/muskit/WacK-Repackager/issues/5
                string tempPreviewDuration = MerLoader.GetMetadata(merLine, "#PREVIEW_DURATION") ??
                                             MerLoader.GetMetadata(merLine, "#PREVIEW_LENGTH");
                if (!string.IsNullOrEmpty(tempPreviewDuration))
                    diff.PreviewDuration = Convert.ToSingle(tempPreviewDuration, CultureInfo.InvariantCulture);
            } while (++readerIndex < merFile.Count);

            diffs.Add(diff);
        }

        if (diffs.Count == 0)
            Debug.Log($"{artist} - {title} empty diffs {folderPath}");

        return new(title, rubi, artist, bpm, folderPath, jacketPath, diffs.ToArray());
    }

    [NotNull]
    private static IEnumerable<SongListEntry> SplitEntriesBy<T>([NotNull] IEnumerable<SongListEntry> entries,
        Func<SongDifficulty, T> keySelector)
    {
        // We use SelectMany since each entry may be split into multiple.
        return entries.SelectMany(entry =>
            // Get all difficulties in the entry.
            entry.Difficulties
                // Get the corresponding SongDifficulties so that we can evaluate the keySelector.
                .Select(diff => entry.Song.SongDiffs[diff])
                // Group by the key
                .GroupBy(keySelector,
                    typeof(T) == typeof(string) ? (IEqualityComparer<T>)StringComparer.InvariantCulture : null)
                // Convert all song difficulties in the group (that is, with the same key) into a single entry.
                // If all difficulties have the same key, they will all be in this one group.
                .Select(group => new SongListEntry
                {
                    Song = entry.Song,
                    Difficulties = group.Select(diff => diff.Difficulty).ToArray(),
                }));
    }

    [NotNull]
    public SortedSongList SortSongList(GroupType groupType, SortType sortType)
    {
        SortedSongList sortedList = new()
        {
            GroupType = groupType,
            SortType = sortType,
        };

        switch (groupType)
        {
            case GroupType.Title:
            {
                List<SortedSongListGroup> groups =
                    songs.GroupByJpEnName(song => (song.Title, song.Rubi))
                        // Convert IGrouping to SortedSongListGroup
                        .Select(group => new SortedSongListGroup
                        {
                            Name = group.Key,
                            Entries = group.Select(song => new SongListEntry
                            {
                                Song = song,
                                // Include all difficulties for the song.
                                Difficulties = song.SongDiffs.Keys.ToArray(),
                            }).ToList(),
                        })
                        // Exclude empty groups.
                        .Where(group => group.Entries.Count > 0)
                        .ToList();

                sortedList.Groups = groups;
                break;
            }

            case GroupType.All:
            {
                sortedList.Groups = new()
                {
                    // There is only a single "group" when using GroupType.All
                    new()
                    {
                        Name = "All",
                        Entries = songs.Select(song => new SongListEntry
                        {
                            Song = song,
                            Difficulties = song.SongDiffs.Keys.ToArray(),
                        }).ToList(),
                    },
                };

                break;
            }

            case GroupType.Artist:
            {
                List<SortedSongListGroup> groups =
                    songs.GroupByJpEnName(song => (song.Artist, null))
                        // Convert IGrouping to SortedSongListGroup
                        .Select(group => new SortedSongListGroup
                        {
                            Name = group.Key,
                            Entries = group.Select(song => new SongListEntry
                            {
                                Song = song,
                                // Since all charts will have the same Artist, just take all difficulties.
                                Difficulties = song.SongDiffs.Keys.ToArray(),
                            }).ToList(),
                        })
                        // Exclude empty groups.
                        .Where(group => group.Entries.Count > 0)
                        .ToList();

                sortedList.Groups = groups;
                break;
            }

            case GroupType.Charter:
            {
                // Convert all songs to SongListEntrys so we can use SplitEntriesBy.
                IEnumerable<SongListEntry> songListEntries = songs.Select(song => new SongListEntry
                {
                    Song = song,
                    // Include all difficulties here, they will be split out by SplitEntriesBy.
                    Difficulties = song.SongDiffs.Keys.ToArray(),
                });
                List<SortedSongListGroup> groups =
                    // Split any songs that have multiple charters into separate entries.
                    // If multiple diffs of a song have the same charter, they will stay as a single entry.
                    SplitEntriesBy(songListEntries, songDiff => songDiff.Charter)
                        // Since entries are already split by charter, we can just take the first difficulty in the
                        // entry to determine the charter.
                        .GroupByJpEnName(entry => (entry.Song.SongDiffs[entry.Difficulties[0]].Charter, null))
                        // Convert IGrouping to SortedSongListGroup
                        .Select(group => new SortedSongListGroup
                        {
                            Name = group.Key,
                            Entries = group.ToList(),
                        })
                        // Exclude empty groups.
                        .Where(group => group.Entries.Count > 0)
                        .ToList();

                sortedList.Groups = groups;

                break;
            }

            case GroupType.Level:
            {
                List<SortedSongListGroup> groups =
                    // Use SelectMany as each song will have multiple entries, one for each diff.
                    songs.SelectMany(song => song.SongDiffs.Keys
                            // Create a separate entry for each diff.
                            .Select(diff => new SongListEntry
                            {
                                Song = song,
                                Difficulties = new[] { diff },
                            }))
                        .GroupBy(entry => SaturnMath.GetDifficultyString(
                            // Difficulties should always have exactly one item, as created above.
                            entry.Song.SongDiffs[entry.Difficulties[0]].Level))
                        // Convert IGrouping to SortedSongListGroup
                        .Select(group => new SortedSongListGroup
                        {
                            Name = group.Key,
                            Entries = group.ToList(),
                        })
                        // Exclude empty groups.
                        .Where(group => group.Entries.Count > 0)
                        // Although GroupByJpEnName should return groups that are correctly ordered, we aren't
                        // guaranteed this by normal GroupBy. Make sure to order them correctly. Note that we can't use
                        // group.Name as this would sort 10 < 2, since it's a string sort.
                        // Instead, just grab the first entry's level for sort - any should be fine.
                        .OrderBy(group => group.Entries[0].Song.SongDiffs[group.Entries[0].Difficulties[0]].Level)
                        .ToList();

                sortedList.Groups = groups;

                break;
            }

            // case GroupType.Genre:
            case GroupType.Folder:
            {
                List<SortedSongListGroup> groups =
                    songs.GroupBy(song => song.ContainingFolder)
                        // StringComparer.InvariantCulture should place parent folders above their children. Otherwise
                        // we don't care about the order too much.
                        .OrderBy(group => group.Key, StringComparer.InvariantCulture)
                        // Convert the IGrouping to SortedSongListGroup
                        .Select(group => new SortedSongListGroup
                        {
                            // We want to use "" as the key for OrderBy, so it sorts first, but it's not human readable.
                            // Use "Root" instead for the human-readable name.
                            Name = group.Key == "" ? "Root" : group.Key,
                            Entries = group.Select(song => new SongListEntry
                            {
                                Song = song,
                                Difficulties = song.SongDiffs.Keys.ToArray(),
                            }).ToList(),
                        })
                        // Exclude empty groups.
                        .Where(group => group.Entries.Count > 0).ToList();

                sortedList.Groups = groups;

                break;
            }
            // Can't implement these yet as we don't have PB data.
            // case GroupType.ClearType:
            // case GroupType.Grade:
            // {
            //     // TODO: implement other sorting keys
            //     throw new NotImplementedException();
            // }

            default:
            {
                throw new ArgumentOutOfRangeException(nameof(groupType), groupType, null);
            }
        }

        switch (sortType)
        {
            case SortType.Title:
            {
                foreach (SortedSongListGroup group in sortedList.Groups)
                {
                    group.Entries = group.Entries.OrderByJpEnName(entry => (entry.Song.Title, entry.Song.Rubi))
                        .ToList();
                }

                break;
            }

            case SortType.Artist:
            {
                foreach (SortedSongListGroup group in sortedList.Groups)
                    // Sort within each group by the Artist.
                    group.Entries = group.Entries.OrderByJpEnName(entry => (entry.Song.Artist, null)).ToList();

                break;
            }

            case SortType.Charter:
            {
                foreach (SortedSongListGroup group in sortedList.Groups)
                {
                    // An entry may have multiple charters, in which case we need to split into multiple entries.
                    // Multiple charts with the same charter will be kept together.
                    group.Entries = SplitEntriesBy(group.Entries, diff => diff.Charter)
                        // Since entries are already split by charter, we can just take the first difficulty in the
                        // entry to determine the charter.
                        .OrderByJpEnName(entry => (entry.Song.SongDiffs[entry.Difficulties[0]].Charter, null)).ToList();
                }

                break;
            }

            case SortType.Bpm:
            {
                foreach (SortedSongListGroup group in sortedList.Groups)
                    group.Entries = group.Entries.OrderBy(entry => entry.Song.Bpm).ToList();

                break;
            }

            case SortType.Level:
            {
                foreach (SortedSongListGroup group in sortedList.Groups)
                {
                    // For level sort, we must have exactly one entry per difficulty. As such, split existing entries.
                    // If there is already only one difficulty per entry, this is harmless.
                    group.Entries = group.Entries.SelectMany(entry =>
                            // For each difficulty in the entry, create a new entry with just that difficulty.
                            entry.Difficulties.Select(diff => new SongListEntry
                            {
                                Song = entry.Song,
                                Difficulties = new[] { diff },
                            }))
                        // Since there's exactly one difficulty per entry, we can just take Difficulties[0] to get the
                        // level as the sort key.
                        .OrderBy(entry => entry.Song.SongDiffs[entry.Difficulties[0]].Level)
                        .ToList();
                }

                break;
            }

            case SortType.DateUpdated:
            {
                foreach (SortedSongListGroup group in sortedList.Groups)
                {
                    // Imported charts will have a similar mtime for charts in the same song, but it might not be
                    // exactly the same. We should avoid splitting these.
                    group.Entries = group.Entries
                        .SelectMany(entry =>
                        {
                            // Order the charts by the update time, so we can go through them sequentially.
                            IOrderedEnumerable<SongDifficulty> charts = entry.Difficulties
                                .Select(diff => entry.Song.SongDiffs[diff]).OrderBy(chart => chart.LastUpdatedTime);

                            List<SongListEntry> entries = new();
                            List<Difficulty> currentDiffs = new();
                            DateTime? lastDateTime = null;
                            foreach (SongDifficulty chart in charts)
                            {
                                // If this chart is more than 1 minute newer than the last chart, split them.
                                if (lastDateTime != null &&
                                    chart.LastUpdatedTime - lastDateTime > TimeSpan.FromMinutes(1))
                                {
                                    currentDiffs.Sort();
                                    entries.Add(new()
                                    {
                                        Song = entry.Song,
                                        Difficulties = currentDiffs.ToArray(),
                                    });

                                    currentDiffs = new();
                                }

                                currentDiffs.Add(chart.Difficulty);
                                lastDateTime = chart.LastUpdatedTime;
                            }

                            if (currentDiffs.Count == 0) return entries;

                            currentDiffs.Sort();
                            entries.Add(new()
                            {
                                Song = entry.Song,
                                Difficulties = currentDiffs.ToArray(),
                            });

                            return entries;
                        })
                        .OrderByDescending(entry => entry.Song.SongDiffs[entry.Difficulties[0]].LastUpdatedTime)
                        .ToList();
                }

                break;
            }

            // case SortType.Genre:
            // {
            //     throw new NotImplementedException();
            //     break;
            // }

            case SortType.Folder:
            {
                foreach (SortedSongListGroup group in sortedList.Groups)
                {
                    group.Entries = group.Entries
                        // Order by title first to split ties within the same folder.
                        .OrderByJpEnName(entry => (entry.Song.Title, entry.Song.Rubi))
                        // OrderBy is a stable sort, so this will preserve the title sort within the same folder.
                        .OrderBy(entry => entry.Song.ContainingFolder).ToList();
                }

                break;
            }

            // case SortType.ClearType:
            // case SortType.Score:
            // {
            //     throw new NotImplementedException();
            //     break;
            // }

            default:
            {
                throw new ArgumentOutOfRangeException(nameof(sortType), sortType, null);
            }
        }

        return sortedList;
    }
}
}
