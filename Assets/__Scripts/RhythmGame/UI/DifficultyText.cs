using SaturnGame.UI;
using TMPro;
using UnityEngine;

public class DifficultyText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private ArcTextMeshPro arc;

    [Space(10)]
    [SerializeField] private Color normalColor = new();
    [SerializeField] private Color hardColor = new();
    [SerializeField] private Color expertColor = new();
    [SerializeField] private Color infernoColor = new(0.2509f, 0, 0.2627f, 1);
    [SerializeField] private Color beyondColor = new();

    [SerializeField] private int difficultyIndex = 0;
    [SerializeField] private int difficultyLevel = 0;
    void Update()
    {
        SetDifficultyText(difficultyIndex, difficultyLevel);
    }

    void SetDifficultyText(int index, int level)
    {
        string diffName;
        Color color;
        switch (index)
        {
            case 0:
                diffName = "NORMAL";
                color = normalColor;
                break;
            
            case 1:
                diffName = "HARD";
                color = hardColor;
                break;
            
            case 2:
                diffName = "EXPERT";
                color = expertColor;
                break;

            case 3:
                diffName = "INFERNO";
                color = infernoColor;
                break;
            
            default:
                diffName = "BEYOND";
                color = beyondColor;
                break;
        }

        text.text = $"{diffName}/Lv.{level}";
        text.color = color;
        arc.UpdateText();
    }
}
