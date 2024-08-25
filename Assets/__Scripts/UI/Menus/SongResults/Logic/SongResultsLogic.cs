using System;
using SaturnGame;
using SaturnGame.Data;
using SaturnGame.RhythmGame;
using TMPro;
using UnityEngine;

public class SongResultsLogic : MonoBehaviour
{
    [SerializeField] private TMP_Text songTitleText;
    [SerializeField] private TMP_Text difficultyText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private ScoreNumRenderer scoreNumRenderer;
    [SerializeField] private JudgementsInfoRenderer judgementsInfoRenderer;
    [SerializeField] private EarlyLateInfoRenderer earlyLateInfoRenderer;

    private void Start()
    {
        Song song = PersistentStateManager.Instance.SelectedSong;
        songTitleText.text = song.Title;
        SongDifficulty songDifficulty = PersistentStateManager.Instance.SelectedDifficultyInfo;
        difficultyText.text = songDifficulty.Difficulty switch
        {
            Difficulty.Normal => "NORMAL",
            Difficulty.Hard => "HARD",
            Difficulty.Expert => "EXPERT",
            Difficulty.Inferno => "INFERNO",
            Difficulty.Beyond => "BEYOND",
            _ => throw new ArgumentOutOfRangeException(),
        };
        levelText.text = SaturnMath.GetDifficultyString(songDifficulty.Level);
        ScoreData scoreData = PersistentStateManager.Instance.LastScoreData;
        Debug.Log(scoreData);
        scoreNumRenderer.SetScoreNum(scoreData.Score);
        judgementsInfoRenderer.SetJudgementCountTexts(scoreData.JudgementCounts.Total);
        earlyLateInfoRenderer.SetEarlyLateCountTexts(scoreData.JudgementCounts.Total);
    }

    public void OnContinue()
    {
        SceneSwitcher.Instance.LoadScene("_SongSelect");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) OnContinue();
    }
}
