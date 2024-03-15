using System.Collections;
using System.Collections.Generic;
using SaturnGame.RhythmGame;
using UnityEngine;

public class JudgementsInfoRenderer : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI marvelousCountText;
    [SerializeField] private TMPro.TextMeshProUGUI greatCountText;
    [SerializeField] private TMPro.TextMeshProUGUI goodCountText;
    [SerializeField] private TMPro.TextMeshProUGUI missCountText;

    public void SetJudgementCountTexts(Dictionary<Judgement, int> judgementCounts)
    {
        // warning, not protected against missing judgement in judgementCounts
        marvelousCountText.text = judgementCounts[Judgement.Marvelous].ToString();
        greatCountText.text = judgementCounts[Judgement.Great].ToString();
        goodCountText.text = judgementCounts[Judgement.Good].ToString();
        missCountText.text = judgementCounts[Judgement.Miss].ToString();
    }
}
