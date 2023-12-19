using SaturnGame.UI;
using TMPro;
using UnityEngine;

public class ScoreText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private ArcTextMeshPro arc;
    [SerializeField] [Range(0, 1000000)] private int displayedScore;
    private int prevScore = 0;
    void Update()
    {
        if (displayedScore != prevScore)
        {
            text.text = $"<mspace=0.7em>{displayedScore:D7}</mspace>";
            arc.UpdateText();
            prevScore = displayedScore;
        }
    }
}
