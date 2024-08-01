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

    private static void SetTextFromJudgementCountsCell(JudgementCountTableCell judgementCountsCell, TMP_Text earlyText,
        TMP_Text lateText)
    {
        earlyText.text = judgementCountsCell.EarlyCount.ToString();
        lateText.text = judgementCountsCell.LateCount.ToString();
    }

    public void SetEarlyLateCountTexts(JudgementCountTableRow judgementCounts)
    {
        earlyCountText.text = judgementCounts.TotalEarlyLate.EarlyCount.ToString();
        lateCountText.text = judgementCounts.TotalEarlyLate.LateCount.ToString();
        SetTextFromJudgementCountsCell(judgementCounts.Marvelous, earlyMarvelousCountText, lateMarvelousCountText);
        SetTextFromJudgementCountsCell(judgementCounts.Great, earlyGreatCountText, lateGreatCountText);
        SetTextFromJudgementCountsCell(judgementCounts.Good, earlyGoodCountText, lateGoodCountText);
    }
}
