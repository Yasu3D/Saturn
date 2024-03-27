using SaturnGame.RhythmGame;
using SaturnGame.UI;
using TMPro;
using UnityEngine;

public class ScoreNumberText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private ArcTextMeshPro arc;
    [SerializeField] [Range(0, 1000000)] private int displayedScore;

    [Header("MANAGERS")] [SerializeField] private ScoringManager scoringManager;

    private int prevScore;

    private void Update()
    {
        displayedScore = scoringManager.CurrentScoreData().Score;
        if (displayedScore == prevScore) return;

        // ReSharper disable StringLiteralTypo
        text.text = $"<mspace=0.7em>{displayedScore:D7}</mspace>";
        // ReSharper restore StringLiteralTypo
        arc.UpdateText();
        prevScore = displayedScore;
    }
}