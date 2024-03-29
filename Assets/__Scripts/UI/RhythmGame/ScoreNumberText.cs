using System;
using SaturnGame.RhythmGame;
using SaturnGame.Settings;
using UnityEngine;
using TMPro;

public class ScoreNumberText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI ScoreValue;

    private void Start()
    {
        ScoreValue.text = SettingsManager.Instance.PlayerSettings.UISettings.ScoreDisplayMethod switch
        {
            0 => "<mspace=0.7em>0000000</mspace>", // Plus Method
            1 => "<mspace=0.7em>1000000</mspace>", // Minus Method
            2 => "<mspace=0.7em>0000000</mspace>", // Average Method
            _ => "<mspace=0.7em>0000000</mspace>",
        };
    }
    
    public void UpdateScore(ScoreData scoreData)
    {
        int score = SettingsManager.Instance.PlayerSettings.UISettings.ScoreDisplayMethod switch
        {
            0 => scoreData.Score, // Plus Method
            1 => 1_000_000 - (scoreData.MaxScore - scoreData.Score), // Minus Method
            2 => (int)(1_000_000 * ((float)scoreData.Score / scoreData.MaxScore)), // Average Method
            _ => 0,
        };
        
        ScoreValue.text = $"<mspace=0.7em>{score:D7}</mspace>";
    }
}