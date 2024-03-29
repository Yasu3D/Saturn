using System;
using System.Globalization;
using UnityEngine;
using DG.Tweening;
using SaturnGame.RhythmGame;
using SaturnGame.Settings;
using TMPro;

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
        ComboGroup.SetActive(Settings.UISettings.CenterDisplayInfo is 1);
        ScoreGroup.SetActive(Settings.UISettings.CenterDisplayInfo is not (0 or 1));
        
        
        currentScore = Settings.UISettings.CenterDisplayInfo switch
        {
            0 => 0, // Off
            1 => 0, // Combo
            2 => 0, // Plus Method
            3 => 1000000, // Minus Method
            4 => 0, // Average Method
            5 => 100000, // S Border
            6 => 0, // SS Border // WIP
            7 => 0, // SSS Border // WIP
            8 => 0, // Personal Best Border // WIP
            _ => 0,
        };

        ScoreValue.text = currentScore.ToString(CultureInfo.InvariantCulture);
    }

    public void UpdateCombo(int combo)
    {
        if (combo <= 0 || Settings.UISettings.CenterDisplayInfo != 1)
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
        
        int score = SettingsManager.Instance.PlayerSettings.UISettings.CenterDisplayInfo switch
        {
            0 => 0, // Off
            1 => 0, // Combo
            2 => scoreData.Score,
            3 => 1_000_000 - (scoreData.MaxScore - scoreData.Score),
            4 => (int)(1_000_000 * ((float)scoreData.Score / scoreData.MaxScore)),
            5 => Math.Max(0, 100_000 - (scoreData.MaxScore - scoreData.Score)), // WIP
            6 => Math.Max(0,  50_000 - (scoreData.MaxScore - scoreData.Score)), // WIP
            7 => Math.Max(0,  20_000 - (scoreData.MaxScore - scoreData.Score)), // WIP
            8 => scoreData.Score, // WIP until scores are saved
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