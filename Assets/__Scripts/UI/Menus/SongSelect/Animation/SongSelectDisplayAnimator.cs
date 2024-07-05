using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SaturnGame.Data;
using TMPro;
using DG.Tweening;
using JetBrains.Annotations;


namespace SaturnGame.UI
{
public class SongSelectDisplayAnimator : MonoBehaviour
{
    private readonly List<Color> foregroundColors = new()
    {
        new Color(0.1019f, 0.4823f, 1.0000f, 1.0000f),
        new Color(1.0000f, 0.7647f, 0.0000f, 1.0000f),
        new Color(1.0000f, 0.0000f, 0.5176f, 1.0000f),
        new Color(0.2509f, 0.0000f, 0.2627f, 1.0000f),
        new Color(0.0000f, 0.0000f, 0.0000f, 1.0000f),
    };

    private readonly List<Color> foregroundCheckerColors = new()
    {
        new Color(0.0941f, 0.4039f, 0.8509f, 1.0000f),
        new Color(0.9450f, 0.6745f, 0.0000f, 1.0000f),
        new Color(0.8705f, 0.0000f, 0.4509f, 1.0000f),
        new Color(0.3411f, 0.0000f, 0.3137f, 1.0000f),
        new Color(0.1000f, 0.1000f, 0.1000f, 1.0000f),
    };

    private readonly List<Color> backgroundColors = new()
    {
        new Color(0.0901f, 0.2235f, 0.3254f, 1.0000f),
        new Color(0.3568f, 0.2559f, 0.1764f, 1.0000f),
        new Color(0.2169f, 0.0051f, 0.1836f, 1.0000f),
        new Color(0.1212f, 0.0457f, 0.1500f, 1.0000f),
        new Color(0.0922f, 0.0460f, 0.2169f, 1.0000f),
    };

    private readonly List<Color> backgroundCheckerColors = new()
    {
        new Color(0.0745f, 0.1803f, 0.3254f, 1.0000f),
        new Color(0.4078f, 0.2941f, 0.1764f, 1.0000f),
        new Color(0.2627f, 0.0000f, 0.1725f, 1.0000f),
        new Color(0.0666f, 0.0274f, 0.0980f, 1.0000f),
        new Color(0.0000f, 0.0000f, 0.0000f, 1.0000f),
    };

    [Header("Colors")]
    [SerializeField] private List<Image> foregroundImages = new();
    [SerializeField] private List<Image> foregroundCheckerImages = new();
    [SerializeField] private List<Image> backgroundImages = new();
    [SerializeField] private List<Image> backgroundCheckerImages = new();
    private const float ColorAnimDuration = 0.2f;
    private const Ease ColorAnimEase = Ease.OutExpo;

    [Header("Text")] [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI artistText;
    [SerializeField] private TextMeshProUGUI charterText;
    [SerializeField] private TextMeshProUGUI bpmText;
    [SerializeField] private TextMeshProUGUI difficultyNameText;
    [SerializeField] private TextMeshProUGUI difficultyLevelText;

    public void SetSongData([NotNull] Song data, Difficulty difficulty)
    {
        SongDifficulty songDiff = data.SongDiffs[difficulty];
        titleText.text = data.Title;
        artistText.text = data.Artist;
        bpmText.text = data.Bpm;


        charterText.text = songDiff.Charter;
        SetDifficulty((int)difficulty, songDiff.Level);
    }

    // TODO: change index to Difficulty enum
    private void SetDifficulty(int index, decimal? level)
    {
        int clampedIndex = Mathf.Clamp(index, 0, 4);
        string diffName = index switch
        {
            0 => "NORMAL",
            1 => "HARD",
            2 => "EXPERT",
            3 => "INFERNO",
            _ => "BEYOND",
        };

        difficultyNameText.text = diffName;
        difficultyLevelText.text = level switch { decimal val => SaturnMath.GetDifficultyString(val), null => "?" };

        foreach (Image img in foregroundImages)
        {
            img.DOColor(foregroundColors[clampedIndex], ColorAnimDuration).SetEase(ColorAnimEase);
        }

        foreach (Image img in foregroundCheckerImages)
        {
            img.DOColor(foregroundCheckerColors[clampedIndex], ColorAnimDuration).SetEase(ColorAnimEase);
        }

        foreach (Image img in backgroundImages)
        {
            img.DOColor(backgroundColors[clampedIndex], ColorAnimDuration).SetEase(ColorAnimEase);
        }

        foreach (Image img in backgroundCheckerImages)
        {
            img.DOColor(backgroundCheckerColors[clampedIndex], ColorAnimDuration).SetEase(ColorAnimEase);
        }
    }
}
}
