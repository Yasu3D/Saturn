using SaturnGame.Loading;
using UnityEngine;

namespace SaturnGame.RhythmGame
{
public class ChartManager : MonoBehaviour
{
    public Chart Chart;
    public AudioClip BGM;
    private async void Start()
    {
        await LoadChart();
    }

    public async Awaitable LoadChart()
    {
        SongDifficulty difficulty = PersistentStateManager.Instance.SelectedDifficulty;
        Chart = ChartLoader.LoadChart(difficulty.ChartFilepath);
        BGM = await AudioLoader.LoadBgm(difficulty.AudioFilepath, streamAudio: false);
    }
}
}