using System;
using System.Globalization;
using UnityEngine;
using DG.Tweening;
using SaturnGame.RhythmGame;
using SaturnGame.Settings;
using TMPro;
using static SaturnGame.Settings.UiSettings.CenterDisplayInfoOptions;

public class CenterDisplay : MonoBehaviour
{
    [SerializeField] private CanvasGroup DisplayGroup;

    [SerializeField] private GameObject ComboGroup;
    [SerializeField] private GameObject ScoreGroup;

    [SerializeField] private RectTransform ComboText;
    [SerializeField] private TextMeshProUGUI ComboValue;
    [SerializeField] private TextMeshProUGUI ScoreValue;
    private static PlayerSettings Settings => SettingsManager.Instance.PlayerSettings;
    private Sequence currentSequence;
    private int currentScore; // only to keep track if it changes to play the animation.

    private void Start()
    {
        ComboGroup.SetActive(Settings.UiSettings.CenterDisplayInfo is Combo);
        ScoreGroup.SetActive(Settings.UiSettings.CenterDisplayInfo is not (Off or Combo));

        currentScore = Settings.UiSettings.CenterDisplayInfo switch
        {
            Off => 0,
            Combo => 0,
            PlusMethod => 0,
            MinusMethod => 1000000,
            AverageMethod => 0,
            SBorder => 100000,
            SsBorder => 0, // WIP
            SssBorder => 0, // WIP
            PersonalBestBorder => 0, // WIP
            _ => 0,
        };

        ScoreValue.text = currentScore.ToString(CultureInfo.InvariantCulture);
    }

    public void UpdateCombo(int combo)
    {
        if (combo <= 0 || Settings.UiSettings.CenterDisplayInfo != Combo)
        {
            currentSequence.Kill(true);
            ComboGroup.SetActive(false);
            return;
        }

        ComboGroup.SetActive(true);
        ComboValue.text = combo.ToString(CultureInfo.InvariantCulture);

        currentSequence.Kill(true);
        currentSequence = DOTween.Sequence();
        currentSequence.Join(ComboText.DOScale(1.6f, 0));
        currentSequence.Join(ComboText.DOAnchorPosY(40, 0));
        currentSequence.Join(ComboValue.rectTransform.DOScale(1.6f, 0));
        currentSequence.Join(ComboValue.rectTransform.DOAnchorPosY(90, 0));

        currentSequence.Join(ComboText.DOScale(1, 0.21f).SetEase(Ease.OutSine));
        currentSequence.Join(ComboText.DOAnchorPosY(42, 0.21f).SetEase(Ease.OutSine));
        currentSequence.Join(ComboValue.rectTransform.DOScale(1, 0.21f).SetEase(Ease.OutSine));
        currentSequence.Join(ComboValue.rectTransform.DOAnchorPosY(73, 0.21f).SetEase(Ease.OutSine));
    }

    public void UpdateScore(ScoreData scoreData)
    {
        //Debug.Log($"{scoreData.MaxScore}");

        int score = SettingsManager.Instance.PlayerSettings.UiSettings.CenterDisplayInfo switch
        {
            Off => 0,
            Combo => 0,
            PlusMethod => scoreData.Score,
            MinusMethod => 1_000_000 - (scoreData.MaxScore - scoreData.Score),
            AverageMethod => (int)(1_000_000 * ((float)scoreData.Score / scoreData.MaxScore)),
            SBorder => Math.Max(0, 100_000 - (scoreData.MaxScore - scoreData.Score)), // WIP
            SsBorder => Math.Max(0,  50_000 - (scoreData.MaxScore - scoreData.Score)), // WIP
            SssBorder => Math.Max(0,  20_000 - (scoreData.MaxScore - scoreData.Score)), // WIP
            PersonalBestBorder => scoreData.Score, // WIP until scores are saved
            _ => 0,
        };

        ScoreValue.text = score.ToString(CultureInfo.InvariantCulture);

        if (currentScore == score) return;
        currentScore = score;

        currentSequence.Kill(true);
        currentSequence = DOTween.Sequence();

        currentSequence.Join(ScoreValue.rectTransform.DOAnchorPosY(54, 0));
        currentSequence.Join(ScoreValue.rectTransform.DOScale(1.35f, 0));

        currentSequence.Join(ScoreValue.rectTransform.DOAnchorPosY(57, 0.21f).SetEase(Ease.OutSine));
        currentSequence.Join(ScoreValue.rectTransform.DOScale(1, 0.21f).SetEase(Ease.OutSine));
    }
}
