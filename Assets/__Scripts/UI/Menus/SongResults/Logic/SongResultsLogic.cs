using System.Collections;
using System.Collections.Generic;
using SaturnGame.RhythmGame;
using UnityEngine;

public class SongResultsLogic : MonoBehaviour
{
    [SerializeField] private ScoreNumRenderer scoreNumRenderer;
    [SerializeField] private JudgementsInfoRenderer judgementsInfoRenderer;
    [SerializeField] private EarlyLateInfoRenderer earlyLateInfoRenderer;

    void Start()
    {
        var scoreData = ChartManager.Instance.LastScoreData;
        scoreNumRenderer.SetScoreNum(scoreData.Score);
        judgementsInfoRenderer.SetJudgementCountTexts(scoreData.JudgementCounts);
        earlyLateInfoRenderer.SetEarlyLateCountTexts(scoreData.EarlyCount, scoreData.LateCount,
            scoreData.EarlyCountByJudgement, scoreData.LateCountByJudgement);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SceneSwitcher.Instance.LoadScene("_SongSelect");
        }
    }
}
