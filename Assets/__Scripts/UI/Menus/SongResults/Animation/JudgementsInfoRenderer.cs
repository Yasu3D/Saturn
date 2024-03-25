using System.Collections.Generic;
using JetBrains.Annotations;
using SaturnGame.RhythmGame;
using TMPro;
using UnityEngine;

public class JudgementsInfoRenderer : MonoBehaviour
{
    [SerializeField] private TMP_Text marvelousCountText;
    [SerializeField] private TMP_Text greatCountText;
    [SerializeField] private TMP_Text goodCountText;
    [SerializeField] private TMP_Text missCountText;

    public void SetJudgementCountTexts([NotNull] Dictionary<Judgement, int> judgementCounts)
    {
        // warning, not protected against missing judgement in judgementCounts
        marvelousCountText.text = judgementCounts[Judgement.Marvelous].ToString();
        greatCountText.text = judgementCounts[Judgement.Great].ToString();
        goodCountText.text = judgementCounts[Judgement.Good].ToString();
        missCountText.text = judgementCounts[Judgement.Miss].ToString();
    }
}
