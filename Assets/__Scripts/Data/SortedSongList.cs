using System.Collections.Generic;
using System.Linq;

namespace SaturnGame.Data
{
public class SortedSongList
{
    public GroupType GroupType;
    public SortType SortType;
    public List<SortedSongListGroup> Groups;

    // Recursively find the group and entry index at the given offset from the given entry.
    // E.g. if offset is -2, find the entry that is 2 to the left.
    public (int, int) RelativeSongIndexes(int groupIndex, int entryIndex, int offset)
    {
        while (offset != 0)
        {
            switch (offset)
            {
                case < 0:
                {
                    if (entryIndex == 0)
                    {
                        groupIndex = SaturnMath.Modulo(groupIndex - 1, Groups.Count);
                        entryIndex = Groups[groupIndex].Entries.Count - 1;
                    }
                    else
                        entryIndex -= 1;

                    offset += 1;
                    continue;
                }
                case > 0:
                {
                    if (entryIndex == Groups[groupIndex].Entries.Count - 1)
                    {
                        groupIndex = SaturnMath.Modulo(groupIndex + 1, Groups.Count);
                        entryIndex = 0;
                    }
                    else
                        entryIndex += 1;

                    offset -= 1;
                    continue;
                }
            }
        }

        return (groupIndex, entryIndex);
    }

    // Recursively find the song entry at the given offset from the given entry.
    // E.g. if offset is -2, find the entry that is 2 to the left.
    public SongListEntry RelativeSongEntry(int groupIndex, int entryIndex, int offset)
    {
        (int offsetGroupIndex, int offsetEntryIndex) = RelativeSongIndexes(groupIndex, entryIndex, offset);
        return Groups[offsetGroupIndex].Entries[offsetEntryIndex];
    }

    // Get the group and entry index of the given folder path and difficulty.
    public (int, int)? FindSongFolder(string folderPath, Difficulty difficulty)
    {
        (int, int)? bestMatch = null;

        for (int groupIndex = 0; groupIndex < Groups.Count; groupIndex++)
        {
            for (int songIndex = 0; songIndex < Groups[groupIndex].Entries.Count; songIndex++)
            {
                if (Groups[groupIndex].Entries[songIndex].Song.FolderPath == folderPath)
                {
                    if (Groups[groupIndex].Entries[songIndex].Difficulties.Contains(difficulty))
                        return (groupIndex, songIndex);

                    // Save this one anyway in case we don't find a difficulty match. But keep looking.
                    bestMatch = (groupIndex, songIndex);
                }
            }
        }

        return bestMatch;
    }
}

public enum GroupType
{
    Title,
    Artist,
    Charter,
    Level,
    //Genre,
    Folder,
    //ClearType,
    //Grade,
    All,
}

public enum SortType
{
    Title,
    Artist,
    Charter,
    Bpm,
    Level,
    //DateUpdated,
    //Genre,
    Folder,
    //ClearType,
    //Score,
}

public class SortedSongListGroup
{
    public string Name;
    public List<SongListEntry> Entries;
}

public class SongListEntry
{
    public Song Song;
    public Difficulty[] Difficulties;
}
}
