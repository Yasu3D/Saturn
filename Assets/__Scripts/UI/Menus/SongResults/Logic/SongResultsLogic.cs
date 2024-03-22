using SaturnGame.RhythmGame;
using UnityEngine;

public class SongResultsLogic : MonoBehaviour
{
    [SerializeField] private ScoreNumRenderer scoreNumRenderer;
    [SerializeField] private JudgementsInfoRenderer judgementsInfoRenderer;
    [SerializeField] private EarlyLateInfoRenderer earlyLateInfoRenderer;

    private void Start()
    {
        ScoreData scoreData = ChartManager.Instance.LastScoreData;
        scoreNumRenderer.SetScoreNum(scoreData.Score);
        judgementsInfoRenderer.SetJudgementCountTexts(scoreData.JudgementCounts);
        earlyLateInfoRenderer.SetEarlyLateCountTexts(scoreData.EarlyCount, scoreData.LateCount,
            scoreData.EarlyCountByJudgement, scoreData.LateCountByJudgement);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) SceneSwitcher.Instance.LoadScene("_SongSelect");
    }
}
