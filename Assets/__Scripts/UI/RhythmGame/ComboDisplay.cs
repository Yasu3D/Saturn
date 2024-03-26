using System.Globalization;
using SaturnGame.RhythmGame;
using TMPro;
using UnityEngine;

public class ComboDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;

    [Header("MANAGERS")] [SerializeField] private ScoringManager scoringManager;

    private void Update()
    {
        text.text = scoringManager.CurrentCombo.ToString(CultureInfo.InvariantCulture);
    }
}