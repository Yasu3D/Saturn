// The PersistentStateManager is a PersistentSingleton that can be used to track arbitrary state that should persist
// as long as the game is booted.

using SaturnGame.Data;
using SaturnGame.RhythmGame;
using UnityEngine.Serialization;

public class PersistentStateManager : PersistentSingleton<PersistentStateManager>
{
    public Song SelectedSong;
    public SongDifficulty SelectedDifficulty;
    public ScoreData LastScoreData;
}