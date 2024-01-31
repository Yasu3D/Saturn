using SaturnGame.UI;
using SaturnGame.RhythmGame;
using TMPro;
using UnityEngine;

public class ScoreText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private ArcTextMeshPro arc;
    [SerializeField] [Range(0, 1000000)] private int displayedScore;

    [Header("MANAGERS")]
    [SerializeField] private ScoringManager scoringManager;

    private int prevScore = 0;
    void Update()
    {
        displayedScore = scoringManager.CurrentScore();
        if (displayedScore != prevScore)
        {
            text.text = $"<mspace=0.7em>{displayedScore:D7}</mspace>";
            arc.UpdateText();
            prevScore = displayedScore;
        }
    }
}
