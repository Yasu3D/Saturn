using System;
using SaturnGame.RhythmGame;
using TMPro;
using UnityEngine;

public class EarlyLateDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;

    [Header("MANAGERS")] [SerializeField] private ScoringManager scoringManager;
    [SerializeField] private TimeManager timeManager;

    private void Update()
    {
        if (scoringManager.LastHitTimeMs is null ||
            scoringManager.LastHitTimeMs.Value + 1000 < timeManager.VisualTimeMs)
        {
            text.enabled = false;
            return;
        }

        // TODO: show ms values??
        float? judgement = scoringManager.LastHitErrorMs;
        switch (judgement)
        {
            case null:
            {
                text.enabled = false;
                return;
            }
            case < 0:
            {
                text.text = "Early";
                text.color = Color.blue;
                break;
            }
            case > 0:
            {
                text.text = "Late";
                text.color = Color.red;
                break;
            }
        }

        text.enabled = true;
    }
}