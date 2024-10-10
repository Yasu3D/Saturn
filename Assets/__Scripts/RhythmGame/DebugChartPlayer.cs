using System.IO;
using SaturnGame.Data;
using SaturnGame.RhythmGame;
using SaturnGame.Settings;
using UnityEngine;

public class DebugChartPlayer : MonoBehaviour
{
    [SerializeField] private ChartManager chartManager;
    [SerializeField] private string folderInSongPacks = "DONOTSHIP/";
    [SerializeField] private Difficulty difficulty;

    private async void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            string folderPath = Path.Combine(SongDatabase.SongPacksPath, folderInSongPacks, "meta.mer");
            Song song = SongDatabase.MER_LoadSong(folderPath);
            SongDifficulty songDiff = song.SongDiffs[difficulty];
            PersistentStateManager.Instance.SelectedSong = song;
            PersistentStateManager.Instance.SelectedDifficultyInfo = songDiff;
            await chartManager.LoadChartAndStart();
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            int speed = ++SettingsManager.Instance.PlayerSettings.GameSettings.NoteSpeed;
            Debug.Log($"Note speed increased to {speed / 10m}");
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            int speed = --SettingsManager.Instance.PlayerSettings.GameSettings.NoteSpeed;
            Debug.Log($"Note speed decreased to {speed / 10m}");
        }
    }
}
