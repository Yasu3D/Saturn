using System.Globalization;
using TMPro;
using UnityEngine;

namespace SaturnGame.RhythmGame
{
public class JudgeDebugInfo : MonoBehaviour
{
    // If there is a better way to do this, please do it.
    [SerializeField] private TextMeshProUGUI totalMarvelousCount;
    [SerializeField] private TextMeshProUGUI totalMarvelousEarlyCount;
    [SerializeField] private TextMeshProUGUI totalMarvelousLateCount;
    [SerializeField] private TextMeshProUGUI totalGreatCount;
    [SerializeField] private TextMeshProUGUI totalGreatEarlyCount;
    [SerializeField] private TextMeshProUGUI totalGreatLateCount;
    [SerializeField] private TextMeshProUGUI totalGoodCount;
    [SerializeField] private TextMeshProUGUI totalGoodEarlyCount;
    [SerializeField] private TextMeshProUGUI totalGoodLateCount;
    [SerializeField] private TextMeshProUGUI totalMissCount;
    [SerializeField] private TextMeshProUGUI totalEarlyCount;
    [SerializeField] private TextMeshProUGUI totalLateCount;
    [SerializeField] private TextMeshProUGUI touchMarvelousCount;
    [SerializeField] private TextMeshProUGUI touchMarvelousEarlyCount;
    [SerializeField] private TextMeshProUGUI touchMarvelousLateCount;
    [SerializeField] private TextMeshProUGUI touchGreatCount;
    [SerializeField] private TextMeshProUGUI touchGreatEarlyCount;
    [SerializeField] private TextMeshProUGUI touchGreatLateCount;
    [SerializeField] private TextMeshProUGUI touchGoodCount;
    [SerializeField] private TextMeshProUGUI touchGoodEarlyCount;
    [SerializeField] private TextMeshProUGUI touchGoodLateCount;
    [SerializeField] private TextMeshProUGUI touchMissCount;
    [SerializeField] private TextMeshProUGUI touchEarlyCount;
    [SerializeField] private TextMeshProUGUI touchLateCount;
    [SerializeField] private TextMeshProUGUI swipeMarvelousCount;
    [SerializeField] private TextMeshProUGUI swipeMarvelousEarlyCount;
    [SerializeField] private TextMeshProUGUI swipeMarvelousLateCount;
    [SerializeField] private TextMeshProUGUI swipeGreatCount;
    [SerializeField] private TextMeshProUGUI swipeGreatEarlyCount;
    [SerializeField] private TextMeshProUGUI swipeGreatLateCount;
    [SerializeField] private TextMeshProUGUI swipeGoodCount;
    [SerializeField] private TextMeshProUGUI swipeGoodEarlyCount;
    [SerializeField] private TextMeshProUGUI swipeGoodLateCount;
    [SerializeField] private TextMeshProUGUI swipeMissCount;
    [SerializeField] private TextMeshProUGUI swipeEarlyCount;
    [SerializeField] private TextMeshProUGUI swipeLateCount;
    [SerializeField] private TextMeshProUGUI snapMarvelousCount;
    [SerializeField] private TextMeshProUGUI snapMarvelousEarlyCount;
    [SerializeField] private TextMeshProUGUI snapMarvelousLateCount;
    [SerializeField] private TextMeshProUGUI snapGreatCount;
    [SerializeField] private TextMeshProUGUI snapGreatEarlyCount;
    [SerializeField] private TextMeshProUGUI snapGreatLateCount;
    [SerializeField] private TextMeshProUGUI snapGoodCount;
    [SerializeField] private TextMeshProUGUI snapGoodEarlyCount;
    [SerializeField] private TextMeshProUGUI snapGoodLateCount;
    [SerializeField] private TextMeshProUGUI snapMissCount;
    [SerializeField] private TextMeshProUGUI snapEarlyCount;
    [SerializeField] private TextMeshProUGUI snapLateCount;
    [SerializeField] private TextMeshProUGUI holdStartMarvelousCount;
    [SerializeField] private TextMeshProUGUI holdStartMarvelousEarlyCount;
    [SerializeField] private TextMeshProUGUI holdStartMarvelousLateCount;
    [SerializeField] private TextMeshProUGUI holdStartGreatCount;
    [SerializeField] private TextMeshProUGUI holdStartGreatEarlyCount;
    [SerializeField] private TextMeshProUGUI holdStartGreatLateCount;
    [SerializeField] private TextMeshProUGUI holdStartGoodCount;
    [SerializeField] private TextMeshProUGUI holdStartGoodEarlyCount;
    [SerializeField] private TextMeshProUGUI holdStartGoodLateCount;
    [SerializeField] private TextMeshProUGUI holdStartMissCount;
    [SerializeField] private TextMeshProUGUI holdStartEarlyCount;
    [SerializeField] private TextMeshProUGUI holdStartLateCount;
    [SerializeField] private TextMeshProUGUI holdMarvelousCount;
    [SerializeField] private TextMeshProUGUI holdMarvelousEarlyCount;
    [SerializeField] private TextMeshProUGUI holdMarvelousLateCount;
    [SerializeField] private TextMeshProUGUI holdGreatCount;
    [SerializeField] private TextMeshProUGUI holdGreatEarlyCount;
    [SerializeField] private TextMeshProUGUI holdGreatLateCount;
    [SerializeField] private TextMeshProUGUI holdGoodCount;
    [SerializeField] private TextMeshProUGUI holdGoodEarlyCount;
    [SerializeField] private TextMeshProUGUI holdGoodLateCount;
    [SerializeField] private TextMeshProUGUI holdMissCount;
    [SerializeField] private TextMeshProUGUI holdEarlyCount;
    [SerializeField] private TextMeshProUGUI holdLateCount;
    [SerializeField] private TextMeshProUGUI chainMarvelousCount;
    [SerializeField] private TextMeshProUGUI chainMissCount;

    public void UpdateWithNewInfo(ScoreData scoreData)
    {
        totalMarvelousCount.text = scoreData.JudgementCounts.Total.Marvelous.Count.ToString(CultureInfo.CurrentCulture);
        totalMarvelousEarlyCount.text =
            scoreData.JudgementCounts.Total.Marvelous.EarlyCount.ToString(CultureInfo.CurrentCulture);
        totalMarvelousLateCount.text =
            scoreData.JudgementCounts.Total.Marvelous.LateCount.ToString(CultureInfo.CurrentCulture);
        totalGreatCount.text = scoreData.JudgementCounts.Total.Great.Count.ToString(CultureInfo.CurrentCulture);
        totalGreatEarlyCount.text =
            scoreData.JudgementCounts.Total.Great.EarlyCount.ToString(CultureInfo.CurrentCulture);
        totalGreatLateCount.text = scoreData.JudgementCounts.Total.Great.LateCount.ToString(CultureInfo.CurrentCulture);
        totalGoodCount.text = scoreData.JudgementCounts.Total.Good.Count.ToString(CultureInfo.CurrentCulture);
        totalGoodEarlyCount.text = scoreData.JudgementCounts.Total.Good.EarlyCount.ToString(CultureInfo.CurrentCulture);
        totalGoodLateCount.text = scoreData.JudgementCounts.Total.Good.LateCount.ToString(CultureInfo.CurrentCulture);
        totalMissCount.text = scoreData.JudgementCounts.Total.MissCount.ToString(CultureInfo.CurrentCulture);
        totalEarlyCount.text =
            scoreData.JudgementCounts.Total.TotalEarlyLate.EarlyCount.ToString(CultureInfo.CurrentCulture);
        totalLateCount.text =
            scoreData.JudgementCounts.Total.TotalEarlyLate.LateCount.ToString(CultureInfo.CurrentCulture);

        touchMarvelousCount.text = scoreData.JudgementCounts.Touch.Marvelous.Count.ToString(CultureInfo.CurrentCulture);
        touchMarvelousEarlyCount.text =
            scoreData.JudgementCounts.Touch.Marvelous.EarlyCount.ToString(CultureInfo.CurrentCulture);
        touchMarvelousLateCount.text =
            scoreData.JudgementCounts.Touch.Marvelous.LateCount.ToString(CultureInfo.CurrentCulture);
        touchGreatCount.text = scoreData.JudgementCounts.Touch.Great.Count.ToString(CultureInfo.CurrentCulture);
        touchGreatEarlyCount.text =
            scoreData.JudgementCounts.Touch.Great.EarlyCount.ToString(CultureInfo.CurrentCulture);
        touchGreatLateCount.text = scoreData.JudgementCounts.Touch.Great.LateCount.ToString(CultureInfo.CurrentCulture);
        touchGoodCount.text = scoreData.JudgementCounts.Touch.Good.Count.ToString(CultureInfo.CurrentCulture);
        touchGoodEarlyCount.text = scoreData.JudgementCounts.Touch.Good.EarlyCount.ToString(CultureInfo.CurrentCulture);
        touchGoodLateCount.text = scoreData.JudgementCounts.Touch.Good.LateCount.ToString(CultureInfo.CurrentCulture);
        touchMissCount.text = scoreData.JudgementCounts.Touch.MissCount.ToString(CultureInfo.CurrentCulture);
        touchEarlyCount.text =
            scoreData.JudgementCounts.Touch.TotalEarlyLate.EarlyCount.ToString(CultureInfo.CurrentCulture);
        touchLateCount.text =
            scoreData.JudgementCounts.Touch.TotalEarlyLate.LateCount.ToString(CultureInfo.CurrentCulture);

        swipeMarvelousCount.text = scoreData.JudgementCounts.Swipe.Marvelous.Count.ToString(CultureInfo.CurrentCulture);
        swipeMarvelousEarlyCount.text =
            scoreData.JudgementCounts.Swipe.Marvelous.EarlyCount.ToString(CultureInfo.CurrentCulture);
        swipeMarvelousLateCount.text =
            scoreData.JudgementCounts.Swipe.Marvelous.LateCount.ToString(CultureInfo.CurrentCulture);
        swipeGreatCount.text = scoreData.JudgementCounts.Swipe.Great.Count.ToString(CultureInfo.CurrentCulture);
        swipeGreatEarlyCount.text =
            scoreData.JudgementCounts.Swipe.Great.EarlyCount.ToString(CultureInfo.CurrentCulture);
        swipeGreatLateCount.text = scoreData.JudgementCounts.Swipe.Great.LateCount.ToString(CultureInfo.CurrentCulture);
        swipeGoodCount.text = scoreData.JudgementCounts.Swipe.Good.Count.ToString(CultureInfo.CurrentCulture);
        swipeGoodEarlyCount.text = scoreData.JudgementCounts.Swipe.Good.EarlyCount.ToString(CultureInfo.CurrentCulture);
        swipeGoodLateCount.text = scoreData.JudgementCounts.Swipe.Good.LateCount.ToString(CultureInfo.CurrentCulture);
        swipeMissCount.text = scoreData.JudgementCounts.Swipe.MissCount.ToString(CultureInfo.CurrentCulture);
        swipeEarlyCount.text =
            scoreData.JudgementCounts.Swipe.TotalEarlyLate.EarlyCount.ToString(CultureInfo.CurrentCulture);
        swipeLateCount.text =
            scoreData.JudgementCounts.Swipe.TotalEarlyLate.LateCount.ToString(CultureInfo.CurrentCulture);

        snapMarvelousCount.text = scoreData.JudgementCounts.Snap.Marvelous.Count.ToString(CultureInfo.CurrentCulture);
        snapMarvelousEarlyCount.text =
            scoreData.JudgementCounts.Snap.Marvelous.EarlyCount.ToString(CultureInfo.CurrentCulture);
        snapMarvelousLateCount.text =
            scoreData.JudgementCounts.Snap.Marvelous.LateCount.ToString(CultureInfo.CurrentCulture);
        snapGreatCount.text = scoreData.JudgementCounts.Snap.Great.Count.ToString(CultureInfo.CurrentCulture);
        snapGreatEarlyCount.text = scoreData.JudgementCounts.Snap.Great.EarlyCount.ToString(CultureInfo.CurrentCulture);
        snapGreatLateCount.text = scoreData.JudgementCounts.Snap.Great.LateCount.ToString(CultureInfo.CurrentCulture);
        snapGoodCount.text = scoreData.JudgementCounts.Snap.Good.Count.ToString(CultureInfo.CurrentCulture);
        snapGoodEarlyCount.text = scoreData.JudgementCounts.Snap.Good.EarlyCount.ToString(CultureInfo.CurrentCulture);
        snapGoodLateCount.text = scoreData.JudgementCounts.Snap.Good.LateCount.ToString(CultureInfo.CurrentCulture);
        snapMissCount.text = scoreData.JudgementCounts.Snap.MissCount.ToString(CultureInfo.CurrentCulture);
        snapEarlyCount.text =
            scoreData.JudgementCounts.Snap.TotalEarlyLate.EarlyCount.ToString(CultureInfo.CurrentCulture);
        snapLateCount.text =
            scoreData.JudgementCounts.Snap.TotalEarlyLate.LateCount.ToString(CultureInfo.CurrentCulture);

        holdStartMarvelousCount.text =
            scoreData.JudgementCounts.HoldStart.Marvelous.Count.ToString(CultureInfo.CurrentCulture);
        holdStartMarvelousEarlyCount.text =
            scoreData.JudgementCounts.HoldStart.Marvelous.EarlyCount.ToString(CultureInfo.CurrentCulture);
        holdStartMarvelousLateCount.text =
            scoreData.JudgementCounts.HoldStart.Marvelous.LateCount.ToString(CultureInfo.CurrentCulture);
        holdStartGreatCount.text = scoreData.JudgementCounts.HoldStart.Great.Count.ToString(CultureInfo.CurrentCulture);
        holdStartGreatEarlyCount.text =
            scoreData.JudgementCounts.HoldStart.Great.EarlyCount.ToString(CultureInfo.CurrentCulture);
        holdStartGreatLateCount.text =
            scoreData.JudgementCounts.HoldStart.Great.LateCount.ToString(CultureInfo.CurrentCulture);
        holdStartGoodCount.text = scoreData.JudgementCounts.HoldStart.Good.Count.ToString(CultureInfo.CurrentCulture);
        holdStartGoodEarlyCount.text =
            scoreData.JudgementCounts.HoldStart.Good.EarlyCount.ToString(CultureInfo.CurrentCulture);
        holdStartGoodLateCount.text =
            scoreData.JudgementCounts.HoldStart.Good.LateCount.ToString(CultureInfo.CurrentCulture);
        holdStartMissCount.text = scoreData.JudgementCounts.HoldStart.MissCount.ToString(CultureInfo.CurrentCulture);
        holdStartEarlyCount.text =
            scoreData.JudgementCounts.HoldStart.TotalEarlyLate.EarlyCount.ToString(CultureInfo.CurrentCulture);
        holdStartLateCount.text =
            scoreData.JudgementCounts.HoldStart.TotalEarlyLate.LateCount.ToString(CultureInfo.CurrentCulture);

        holdMarvelousCount.text = scoreData.JudgementCounts.Hold.Marvelous.Count.ToString(CultureInfo.CurrentCulture);
        holdMarvelousEarlyCount.text =
            scoreData.JudgementCounts.Hold.Marvelous.EarlyCount.ToString(CultureInfo.CurrentCulture);
        holdMarvelousLateCount.text =
            scoreData.JudgementCounts.Hold.Marvelous.LateCount.ToString(CultureInfo.CurrentCulture);
        holdGreatCount.text = scoreData.JudgementCounts.Hold.Great.Count.ToString(CultureInfo.CurrentCulture);
        holdGreatEarlyCount.text = scoreData.JudgementCounts.Hold.Great.EarlyCount.ToString(CultureInfo.CurrentCulture);
        holdGreatLateCount.text = scoreData.JudgementCounts.Hold.Great.LateCount.ToString(CultureInfo.CurrentCulture);
        holdGoodCount.text = scoreData.JudgementCounts.Hold.Good.Count.ToString(CultureInfo.CurrentCulture);
        holdGoodEarlyCount.text = scoreData.JudgementCounts.Hold.Good.EarlyCount.ToString(CultureInfo.CurrentCulture);
        holdGoodLateCount.text = scoreData.JudgementCounts.Hold.Good.LateCount.ToString(CultureInfo.CurrentCulture);
        holdMissCount.text = scoreData.JudgementCounts.Hold.MissCount.ToString(CultureInfo.CurrentCulture);
        holdEarlyCount.text =
            scoreData.JudgementCounts.Hold.TotalEarlyLate.EarlyCount.ToString(CultureInfo.CurrentCulture);
        holdLateCount.text =
            scoreData.JudgementCounts.Hold.TotalEarlyLate.LateCount.ToString(CultureInfo.CurrentCulture);

        chainMarvelousCount.text = scoreData.JudgementCounts.Chain.MarvelousCount.ToString(CultureInfo.CurrentCulture);
        chainMissCount.text = scoreData.JudgementCounts.Chain.MissCount.ToString(CultureInfo.CurrentCulture);
    }

    // TODO: Update note type colors with the actual colors set by the player.
}
}
