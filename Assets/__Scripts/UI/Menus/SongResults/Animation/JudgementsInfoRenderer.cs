using SaturnGame.RhythmGame;
using TMPro;
using UnityEngine;

public class JudgementsInfoRenderer : MonoBehaviour
{
    [SerializeField] private TMP_Text marvelousCountText;
    [SerializeField] private TMP_Text greatCountText;
    [SerializeField] private TMP_Text goodCountText;
    [SerializeField] private TMP_Text missCountText;

    public void SetJudgementCountTexts(JudgementCountTableRow judgementCounts)
    {
        marvelousCountText.text = judgementCounts.Marvelous.Count.ToString();
        greatCountText.text = judgementCounts.Great.Count.ToString();
        goodCountText.text = judgementCounts.Good.Count.ToString();
        missCountText.text = judgementCounts.MissCount.ToString();
    }
}
