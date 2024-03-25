using System.Globalization;
using SaturnGame.UI;
using TMPro;
using UnityEngine;

public class DifficultyText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private ArcTextMeshPro arc;

    [Space(10)]
    [SerializeField] private Color normalColor = new(0.1019f, 0.4823f, 1f, 1f);
    [SerializeField] private Color hardColor = new(1f, 0.7647f, 0f, 1f);
    [SerializeField] private Color expertColor = new(1f, 0f, 0.5176f, 1f);
    [SerializeField] private Color infernoColor = new(0.2509f, 0f, 0.2627f, 1f);
    [SerializeField] private Color beyondColor = new(0f, 0f, 0f, 1f);

    [SerializeField] private int difficultyIndex;
    [SerializeField] private float difficultyLevel;

    private void Update()
    {
        SetDifficultyText(difficultyIndex, difficultyLevel);
    }

    private void SetDifficultyText(int index, float level)
    {
        string diffName;
        Color color;
        switch (index)
        {
            case 0:
            {
                diffName = "NORMAL";
                color = normalColor;
                break;
            }

            case 1:
            {
                diffName = "HARD";
                color = hardColor;
                break;
            }

            case 2:
            {
                diffName = "EXPERT";
                color = expertColor;
                break;
            }

            case 3:
            {
                diffName = "INFERNO";
                color = infernoColor;
                break;
            }

            default:
            {
                diffName = "BEYOND";
                color = beyondColor;
                break;
            }
        }

        // TODO: Eventually replace with SongData.GetDifficultyString();
        string diffLevel = Mathf.Floor(level).ToString(CultureInfo.InvariantCulture);
        if (level % 1 > 0.6f) diffLevel += "+";

        text.text = $"{diffName}/Lv.{diffLevel}";
        text.color = color;
        arc.UpdateText();
    }
}
