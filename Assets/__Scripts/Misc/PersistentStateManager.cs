
using SaturnGame.Data;
using SaturnGame.RhythmGame;

/// <summary>
/// The PersistentStateManager is a PersistentSingleton that can be used to track arbitrary state that should persist as
/// long as the game is booted.
/// </summary>
public class PersistentStateManager : PersistentSingleton<PersistentStateManager>
{
    public Song SelectedSong;
    public SongDifficulty SelectedDifficulty;
    public ScoreData LastScoreData;
}