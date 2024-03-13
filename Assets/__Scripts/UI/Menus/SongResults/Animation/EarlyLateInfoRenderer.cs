using System.Collections;
using System.Collections.Generic;
using SaturnGame.RhythmGame;
using UnityEngine;
using UnityEngine.Serialization;

public class EarlyLateInfoRenderer : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI earlyCountText;
    [SerializeField] private TMPro.TextMeshProUGUI lateCountText;
    [SerializeField] private TMPro.TextMeshProUGUI earlyMarvelousCountText;
    [SerializeField] private TMPro.TextMeshProUGUI earlyGreatCountText;
    [SerializeField] private TMPro.TextMeshProUGUI earlyGoodCountText;
    [SerializeField] private TMPro.TextMeshProUGUI lateMarvelousCountText;
    [SerializeField] private TMPro.TextMeshProUGUI lateGreatCountText;
    [SerializeField] private TMPro.TextMeshProUGUI lateGoodCountText;

    private void setTextsFromJudgementCounts(Dictionary<Judgement, int> countsByJudgement,
        TMPro.TextMeshProUGUI marvelousCountText, TMPro.TextMeshProUGUI greatCountText,
        TMPro.TextMeshProUGUI goodCountText)
    {
        marvelousCountText.text = countsByJudgement[Judgement.Marvelous].ToString();
        greatCountText.text = countsByJudgement[Judgement.Great].ToString();
        goodCountText.text = countsByJudgement[Judgement.Good].ToString();
    }
    
    public void SetEarlyLateCountTexts(int earlyCount, int lateCount, Dictionary<Judgement, int> earlyCounts, Dictionary<Judgement, int> lateCounts)
    {
        earlyCountText.text = earlyCount.ToString();
        lateCountText.text = lateCount.ToString();
        setTextsFromJudgementCounts(earlyCounts, earlyMarvelousCountText, earlyGreatCountText, earlyGoodCountText);
        setTextsFromJudgementCounts(lateCounts, lateMarvelousCountText, lateGreatCountText, lateGoodCountText);
    }
}
