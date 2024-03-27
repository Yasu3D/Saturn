// The PersistentStateManager is a PersistentSingleton that can be used to track arbitrary state that should persist
// as long as the game is booted.

public class PersistentStateManager : PersistentSingleton<PersistentStateManager>
{
    public string LastSelectedSongPath;
    public int LastSelectedDifficulty;
}