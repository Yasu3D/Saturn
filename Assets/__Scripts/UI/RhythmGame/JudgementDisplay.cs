using System;
using SaturnGame.RhythmGame;
using TMPro;
using UnityEngine;

public class JudgementDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;

    [Header("MANAGERS")] [SerializeField] private ScoringManager scoringManager;
    [SerializeField] private TimeManager timeManager;

    private void Update()
    {
        if (scoringManager.LastJudgementTimeMs is null ||
            scoringManager.LastJudgementTimeMs.Value + 1000 < timeManager.VisualTimeMs)
        {
            text.enabled = false;
            return;
        }

        Judgement judgement = scoringManager.LastJudgement;
        switch (judgement)
        {
            case Judgement.None:
            {
                // ???
                text.text = "unknown";
                text.color = Color.white;
                break;
            }
            case Judgement.Miss:
            {
                text.text = "Miss";
                text.color = Color.grey;
                break;
            }
            case Judgement.Good:
            {
                text.text = "Good";
                text.color = Color.blue;
                break;
            }
            case Judgement.Great:
            {
                text.text = "Great";
                text.color = Color.green;
                break;
            }
            case Judgement.Marvelous:
            {
                text.text = "Marvelous";
                text.color = Color.magenta;
                break;
            }
            default:
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        text.enabled = true;
    }
}