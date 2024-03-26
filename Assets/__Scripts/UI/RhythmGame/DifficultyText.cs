using System;
using SaturnGame;
using SaturnGame.UI;
using TMPro;
using UnityEngine;

public class DifficultyText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private ArcTextMeshPro arc;

    [Space(10)] [SerializeField] private Color normalColor = new(0.1019f, 0.4823f, 1f, 1f);
    [SerializeField] private Color hardColor = new(1f, 0.7647f, 0f, 1f);
    [SerializeField] private Color expertColor = new(1f, 0f, 0.5176f, 1f);
    [SerializeField] private Color infernoColor = new(0.2509f, 0f, 0.2627f, 1f);
    [SerializeField] private Color beyondColor = new(0f, 0f, 0f, 1f);

    [SerializeField] private Difficulty difficulty;
    // decimal is not serializable
    private decimal difficultyLevel;


    private void Start()
    {
        SongDifficulty songDifficulty = PersistentStateManager.Instance.LastSelectedDifficulty;
        difficulty = songDifficulty.Difficulty;
        difficultyLevel = songDifficulty.Level;
        UpdateDifficultyText();
    }

    private void UpdateDifficultyText()
    {
        string diffName;
        Color color;
        switch (difficulty)
        {
            case Difficulty.Normal:
            {
                diffName = "NORMAL";
                color = normalColor;
                break;
            }

            case Difficulty.Hard:
            {
                diffName = "HARD";
                color = hardColor;
                break;
            }

            case Difficulty.Expert:
            {
                diffName = "EXPERT";
                color = expertColor;
                break;
            }

            case Difficulty.Inferno:
            {
                diffName = "INFERNO";
                color = infernoColor;
                break;
            }

            case Difficulty.Beyond:
            {
                diffName = "BEYOND";
                color = beyondColor;
                break;
            }

            default:
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        // TODO: Eventually replace with SongData.GetDifficultyString();
        string diffLevelName = SaturnMath.GetDifficultyString(difficultyLevel);

        text.text = $"{diffName}/Lv.{diffLevelName}";
        text.color = color;
        arc.UpdateText();
    }
}