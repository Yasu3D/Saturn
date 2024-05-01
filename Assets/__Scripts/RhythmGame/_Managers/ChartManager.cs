using SaturnGame.Loading;
using UnityEngine;

namespace SaturnGame.RhythmGame
{
public class ChartManager : MonoBehaviour
{
    [SerializeField] private TimeManager timeManager;
    public Chart Chart;
    public AudioClip BGM;
    private async void Start()
    {
        // Note: Chart / BGM may still be loading when the scene finishes loading.
        await LoadChartAndStart();
    }

    public async Awaitable LoadChartAndStart()
    {
        SongDifficulty difficulty = PersistentStateManager.Instance.SelectedDifficulty;
        Chart = ChartLoader.LoadChart(difficulty.ChartFilepath);
        BGM = await AudioLoader.LoadBgm(difficulty.AudioFilepath, streamAudio: false);

        // Wait for a couple seconds to start, to avoid starting while the loading animation finishes
        await Awaitable.WaitForSecondsAsync(2f);

        timeManager.StartSong();
    }
}
}
