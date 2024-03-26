// The PersistentStateManager is a PersistentSingleton that can be used to track arbitrary state that should persist
// as long as the game is booted.

using SaturnGame.Data;

public class PersistentStateManager : PersistentSingleton<PersistentStateManager>
{
    public Song LastSelectedSong;
    public SongDifficulty LastSelectedDifficulty;
}