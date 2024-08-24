using System;
using JetBrains.Annotations;
using SaturnGame.Data;
using SaturnGame.RhythmGame;
using SaturnGame.Settings;
using Tomlyn.Helpers;
using Tomlyn.Syntax;

/// <summary>
/// The PersistentStateManager is a PersistentSingleton that can be used to track arbitrary state that should persist as
/// long as the game is booted.
/// </summary>
public class PersistentStateManager : PersistentSingleton<PersistentStateManager>
{
    // Note: at the moment we save this to disk every time it changes (including every time you move around in song select)
    // We may need to change that.
    private PersistentStateData underlyingData;

    public GroupType SelectedGroupType
    {
        get => underlyingData.SelectedGroupType;
        set
        {
            underlyingData.SelectedGroupType = value;
            underlyingData.SaveToFile();
        }
    }

    public SortType SelectedSortType
    {
        get => underlyingData.SelectedSortType;
        set
        {
            underlyingData.SelectedSortType = value;
            underlyingData.SaveToFile();
        }
    }

    public string SelectedSongFolderPath => underlyingData.SelectedSongFolder;

    [CanBeNull] private Song cachedSelectedSong;

    [CanBeNull]
    public Song SelectedSong
    {
        get => cachedSelectedSong?.FolderPath == underlyingData.SelectedSongFolder ? cachedSelectedSong : null;
        set
        {
            cachedSelectedSong = value;
            underlyingData.SelectedSongFolder = value?.FolderPath;
            underlyingData.SaveToFile();
        }
    }

    public Difficulty SelectedDifficulty => underlyingData.SelectedDifficulty;
    public SongDifficulty SelectedDifficultyInfo
    {
        // Note: the "?? new()" here is kind of abusive, but it will give us an empty SongDifficulty with
        // Difficulty = NORMAL. Proper protection is in SongSelectLogic. This is a bit of a mess, but it's not worth it
        // to worry about right now.
        get => SelectedSong?.SongDiffs[underlyingData.SelectedDifficulty] ?? new();
        set
        {
            // If this is not a diff of the currently selected song, shit may break
            if (SelectedSong == null || !SelectedSong.SongDiffs.ContainsValue(value))
                throw new ArgumentException("SongDifficulty is not from the SelectedSong");

            underlyingData.SelectedDifficulty = value.Difficulty;
            underlyingData.SaveToFile();
        }
    }

    // This is not persisted across game restarts.
    public ScoreData LastScoreData;

    protected override void Awake()
    {
        base.Awake();

        underlyingData = PersistentStateData.Load();
    }
}

public class PersistentStateData : TomlPersistedData<PersistentStateData>
{
    // If new fields are added before SelectedGroupType, update the leading trivia (AddTriviaToNewFile) to be before the first one.
    public GroupType SelectedGroupType { get; set; } = GroupType.Title;
    public SortType SelectedSortType { get; set; } = SortType.Title;
    public string SelectedSongFolder { get; set; }
    public Difficulty SelectedDifficulty { get; set; } = Difficulty.Normal;

    protected override string TomlFile => "state.toml";

    protected override void AddTriviaToNewFile()
    {
        SetLeadingTrivia(TomlNamingHelper.PascalToSnakeCase(nameof(SelectedGroupType)), new()
        {
            new(TokenKind.Comment,
                "# Don't mess with this unless you know what you're doing."),
            new(TokenKind.NewLine, "\n"),
        });
    }
}
