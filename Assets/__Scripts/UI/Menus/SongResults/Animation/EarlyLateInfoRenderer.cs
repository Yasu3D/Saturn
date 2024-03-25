using System.Collections.Generic;
using JetBrains.Annotations;
using SaturnGame.RhythmGame;
using TMPro;
using UnityEngine;

public class EarlyLateInfoRenderer : MonoBehaviour
{
    [SerializeField] private TMP_Text earlyCountText;
    [SerializeField] private TMP_Text lateCountText;
    [SerializeField] private TMP_Text earlyMarvelousCountText;
    [SerializeField] private TMP_Text earlyGreatCountText;
    [SerializeField] private TMP_Text earlyGoodCountText;
    [SerializeField] private TMP_Text lateMarvelousCountText;
    [SerializeField] private TMP_Text lateGreatCountText;
    [SerializeField] private TMP_Text lateGoodCountText;

    private static void SetTextsFromJudgementCounts([NotNull] Dictionary<Judgement, int> countsByJudgement,
        [NotNull] TMP_Text marvelousCountText, [NotNull] TMP_Text greatCountText,
        [NotNull] TMP_Text goodCountText)
    {
        marvelousCountText.text = countsByJudgement[Judgement.Marvelous].ToString();
        greatCountText.text = countsByJudgement[Judgement.Great].ToString();
        goodCountText.text = countsByJudgement[Judgement.Good].ToString();
    }

    public void SetEarlyLateCountTexts(int earlyCount, int lateCount, [NotNull] Dictionary<Judgement, int> earlyCounts,
        [NotNull] Dictionary<Judgement, int> lateCounts)
    {
        earlyCountText.text = earlyCount.ToString();
        lateCountText.text = lateCount.ToString();
        SetTextsFromJudgementCounts(earlyCounts, earlyMarvelousCountText, earlyGreatCountText, earlyGoodCountText);
        SetTextsFromJudgementCounts(lateCounts, lateMarvelousCountText, lateGreatCountText, lateGoodCountText);
    }
}
