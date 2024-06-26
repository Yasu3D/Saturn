using SaturnGame.RhythmGame;
using SaturnGame.Settings;
using UnityEngine;
using TMPro;
using static SaturnGame.Settings.UISettings.ScoreDisplayMethods;

public class ScoreNumberText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI ScoreValue;

    private void Start()
    {
        ScoreValue.text = SettingsManager.Instance.PlayerSettings.UISettings.ScoreDisplayMethod switch
        {
            PlusMethod => "<mspace=0.7em>0000000</mspace>", // Plus Method
            MinusMethod => "<mspace=0.7em>1000000</mspace>", // Minus Method
            AverageMethod => "<mspace=0.7em>0000000</mspace>", // Average Method
            _ => "<mspace=0.7em>0000000</mspace>",
        };
    }
    
    public void UpdateScore(ScoreData scoreData)
    {
        int score = SettingsManager.Instance.PlayerSettings.UISettings.ScoreDisplayMethod switch
        {
            PlusMethod => scoreData.Score, // Plus Method
            MinusMethod => 1_000_000 - (scoreData.MaxScore - scoreData.Score), // Minus Method
            AverageMethod => (int)(1_000_000 * ((float)scoreData.Score / scoreData.MaxScore)), // Average Method
            _ => 0,
        };
        
        ScoreValue.text = $"<mspace=0.7em>{score:D7}</mspace>";
    }
}
