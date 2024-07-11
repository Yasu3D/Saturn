using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.Scripting;

namespace SaturnGame.RhythmGame
{
/// <summary>
/// ScoringManager handles input and registers judgement results.
/// </summary>
/// ScoringManager is stateful and expects to receive inputs in order.
/// A new ScoringManager should be used for each independent score.
public class ScoringManager : MonoBehaviour
{
    [Header("DEBUG")]
    [SerializeField] private TextMeshProUGUI debugText;
    public bool AutoWriteReplays = true;
    public bool WritingReplayAndExiting; // Only modify on main thread Update()

    [Header("MANAGERS")]
    [SerializeField] private TimeManager timeManager;
    [SerializeField] private ChartManager chartManager;
    [SerializeField] private ReplayManager replayManager;

    [Space(10)]

    public int CurrentCombo;
    public int LastMissChartTick;
    public bool NeedTouchHitsound;
    public bool NeedSwipeSnapHitsound;

    [Header("VISUALS")]
    [SerializeField] private CenterDisplay centerDisplay;
    [SerializeField] private JudgementDisplay judgementDisplay;
    [SerializeField] private ScoreNumberText scoreText;

    private string loadedChart;
    private Chart Chart => chartManager.Chart;

    // Notes must be sorted by note TimeMs
    private List<Note> Notes => Chart.ProcessedNotesForGameplay;

    private void ShowDebugText(string text)
    {
        //Debug.Log( $"{timeManager.VisualTimeMs}: {text}");
        if (debugText is null)
            return;

        debugText.text = $"{timeManager.GameplayTimeMs}\n" + text;
    }

    // TODO: rework this to avoid a bajillion allocations
    public ScoreData CurrentScoreData()
    {
        ScoreData data = new()
        {
            Score = 0,
            MaxScore = 0,
            JudgementCounts = new()
            {
                { Judgement.Marvelous, 0 },
                { Judgement.Great, 0 },
                { Judgement.Good, 0 },
                { Judgement.Miss, 0 },
            },
            EarlyCount = 0,
            EarlyCountByJudgement = new()
            {
                { Judgement.Marvelous, 0 },
                { Judgement.Great, 0 },
                { Judgement.Good, 0 },
            },
            LateCount = 0,
            LateCountByJudgement = new()
            {
                { Judgement.Marvelous, 0 },
                { Judgement.Great, 0 },
                { Judgement.Good, 0 },
            },
        };

        if (Notes is null || Notes.Count == 0) return data;

        // Values aren't normalized to a range of [0 <> 1,000,000] yet.
        long currentScore = 0;
        long maxTotalScore = 0;
        long maxCurrentScore = 0;

        foreach (Note note in Notes)
        {
            int noteMaxScore = 100;
            int noteScore = note.Judgement switch
            {
                null => 0,
                Judgement.None => 0,
                Judgement.Miss => 0,
                Judgement.Good => 50,
                Judgement.Great => 70,
                Judgement.Marvelous => 100,
                _ => throw new ArgumentOutOfRangeException(),
            };

            if (note.BonusType is Note.NoteBonusType.RNote)
            {
                noteMaxScore *= 2;
                noteScore *= 2;
            }

            currentScore += noteScore;
            maxTotalScore += noteMaxScore;

            if (note.Judgement is null or Judgement.None) continue;

            maxCurrentScore += noteMaxScore;
            data.JudgementCounts[note.Judgement.Value]++;

            if (note is ChainNote || note.TimeErrorMs is null || note.Judgement is Judgement.Miss) continue;

            if (note.TimeErrorMs < 0)
            {
                data.EarlyCountByJudgement[note.Judgement.Value]++;
                if (note.Judgement is not Judgement.Marvelous) data.EarlyCount++;
            }

            if (note.TimeErrorMs > 0)
            {
                data.LateCountByJudgement[note.Judgement.Value]++;
                if (note.Judgement is not Judgement.Marvelous) data.LateCount++;
            }
        }

        data.Score = maxTotalScore == 0 ? 0 : (int)(1_000_000L * currentScore / maxTotalScore);
        data.MaxScore = maxTotalScore == 0 ? 0 : (int)(1_000_000L * maxCurrentScore / maxTotalScore);
        return data;
    }

    // This should be greater than the maximum late timing window of any note.
    private const float IgnorePastNotesThreshold = 300f;

    // This should be greater than the maximum early timing window of any note.
    private const float IgnoreFutureNotesThreshold = 300f;

    private void HitNote(float hitTimeMs, [NotNull] Note note, bool needSnapSwipeHitsound = false)
    {
        Judgement lastJudgement = note.Hit(hitTimeMs);
        ScoreData currentScoreData = CurrentScoreData();

        // HoldNotes affect combo at hold end
        if (note is not HoldNote) IncrementCombo(note.ChartTick);

        scoreText.UpdateScore(currentScoreData);
        centerDisplay.UpdateScore(currentScoreData);
        float earlyLateErrorMs = 0;
        if (note is not ChainNote && lastJudgement is not Judgement.Marvelous)
            // TimeErrorMs should always be set by Hit
            earlyLateErrorMs = note.TimeErrorMs!.Value;
        judgementDisplay.ShowJudgement(lastJudgement, earlyLateErrorMs);

        NeedTouchHitsound = true;
        NeedSwipeSnapHitsound = needSnapSwipeHitsound;
    }

    private void IncrementCombo(int chartTick)
    {
        // If we missed a note that was later than the note we just hit, don't count it towards the combo.
        if (LastMissChartTick <= chartTick)
            CurrentCombo++;
        // Warning: we could still miss a note that is earlier than the note we just hit. So, the combo would be the
        // number of notes hit consecutively, but they would not actually be consecutive notes.

        centerDisplay.UpdateCombo(CurrentCombo);
    }

    private void EndCombo(int chartTick)
    {
        CurrentCombo = 0;
        LastMissChartTick = chartTick;

        centerDisplay.UpdateCombo(CurrentCombo);
    }

    private void MaybeCalculateHitForNote(float timeMs, TouchState touchState, [NotNull] Note note)
    {
        if (note.HitWindowsEvaluated) return;

        if (timeMs < note.EarliestHitTimeMs)
        {
            // This note cannot be hit yet.
            return;
        }

        if (timeMs >= note.LatestHitTimeMs)
        {
            switch (note)
            {
                case TouchNote:
                case SwipeNote:
                case SnapNote:
                {
                    // These note types require an input within the window to hit, so we know they can't have been hit
                    // between the last input and this one.
                    note.MissHit();
                    EndCombo(note.ChartTick);
                    break;
                }
                case ChainNote chainNote:
                {
                    // TODO: consolidate this logic with case where EarliestHitTImeMs <= timeMs < LatestHitTimeMs?
                    // If the note's window overlaps with the range from prevTouchTimeMs to timeMs, and it was held in
                    // the mean time, mark as touched.
                    // Note: for checking the time window overlaps, the other boundary is checked above
                    // (timeMs >= LatestHitTimeMs > EarliestHitTimeMs), so we only check
                    // prevTouchTimeMs < chainNote.LatestHitTimeMs here.
                    if (prevTouchTimeMs < chainNote.LatestHitTimeMs && chainNote.Touched(prevTouchState))
                        chainNote.HasBeenTouched = true;

                    if (chainNote.HasBeenTouched && timeMs >= chainNote.TimeMs)
                        HitNote(chainNote.TimeMs, chainNote);
                    else
                        chainNote.MissHit();

                    break;
                }
                case HoldNote holdNote:
                {
                    holdNote.MissHit();
                    // HoldNotes affect combo at hold end.
                    activeHolds.Add(holdNote);
                    break;
                }
            }

            ScoreData currentScoreData = CurrentScoreData();
            scoreText.UpdateScore(currentScoreData);
            centerDisplay.UpdateScore(currentScoreData);
            judgementDisplay.ShowJudgement(Judgement.Miss, 0);
            return;
        }

        // If we've reached this point, the note can be hit by this input.
        switch (note)
        {
            case TouchNote:
            {
                if (!note.Touched(newSegments)) break;

                HitNote(timeMs, note);
                break;
            }
            case ChainNote chainNote:
            {
                if (chainNote.Touched(touchState) || chainNote.Touched(prevTouchState))
                    chainNote.HasBeenTouched = true;

                if (chainNote.HasBeenTouched && timeMs >= chainNote.TimeMs)
                    // warning, inconsistent HitTimeMs if touched by prevTouchState but not this one.
                    HitNote(note.TimeMs, chainNote);
                break;
            }
            case HoldNote holdNote:
            {
                if (!holdNote.Touched(newSegments)) break;

                //float errorMs = hitTimeMs - holdNote.TimeMs;
                //ShowDebugText($"{noteScanIndex} (hold): {errorMs} {holdNote.Left} {holdNote.Right}");

                HitNote(timeMs, holdNote);
                activeHolds.Add(holdNote);
                break;
            }
            case SwipeNote swipeNote:
            {
                swipeNote.MaybeUpdateMinAverageOffset(prevTouchState);
                if (!swipeNote.Swiped(touchState)) break;

                //float errorMs = hitTimeMs - swipeNote.TimeMs;
                //ShowDebugText($"{noteScanIndex} (swipe): {errorMs}");

                HitNote(timeMs, swipeNote, needSnapSwipeHitsound: true);
                break;
            }
            case SnapNote snapNote:
            {
                if (!snapNote.Snapped(prevTouchState, touchState)) break;

                //float errorMs = hitTimeMs - snapNote.TimeMs;
                //ShowDebugText($"{noteScanIndex} (snap): {errorMs}");

                HitNote(timeMs, snapNote, needSnapSwipeHitsound: true);
                break;
            }
        }
    }

    // Updates the current state of the HoldNote, given prevTouchState was held from prevTouchTimeMs to timeMs.
    // Returns true if the hold is completed and can be removed from further consideration.
    private bool UpdateHold(float timeMs, [NotNull] HoldNote holdNote)
    {
        if (timeMs < holdNote.Start.TimeMs)
        {
            // Hold was hit early, and we haven't started the actual hold body.
            // Skip doing any judgement on this hold for now.
            return false;
        }

        // Check all segments overlapping the time window between prevTouchTimeMs and timeMs
        // Don't include the last segment, which is the hold end.
        for (int i = 0; i < holdNote.Notes.Length - 1; i++)
        {
            HoldSegment curSegment = holdNote.Notes[i];
            HoldSegment nextSegment = holdNote.Notes[i + 1];
            if (curSegment.TimeMs >= timeMs || nextSegment.TimeMs < prevTouchTimeMs)
                // segment is out of time window
                continue;
            //ShowDebugText($"check segment {i} ({curSegment.TimeMs} - {nextSegment.TimeMs})");
            if (curSegment.Touched(prevTouchState))
            {
                holdNote.Held = true;
                if (!holdNote.CurrentlyHeld)
                {
                    // This is a re-grab, see if the hold was not held long enough to count as dropped.
                    float dropTimeMs = curSegment.TimeMs - holdNote.LastHeldTimeMs!.Value;
                    if (dropTimeMs > HoldNote.LeniencyMs && !holdNote.Dropped)
                    {
                        ShowDebugText($"dropped hold after {dropTimeMs} ({curSegment.TimeMs} - {holdNote.LastHeldTimeMs})");
                        holdNote.Dropped = true;
                    }
                }

                holdNote.CurrentlyHeld = true;
                // The note should be held up until at least the earlier of the next segment start or the current time.
                holdNote.LastHeldTimeMs = Math.Min(timeMs, nextSegment.TimeMs);
            }
            else
                holdNote.CurrentlyHeld = false;
        }

        if (timeMs < holdNote.End.TimeMs) return false;

        // Hold note is finished.
        if (holdNote is { CurrentlyHeld: false, Dropped: false } &&
            holdNote.End.TimeMs - holdNote.LastHeldTimeMs > HoldNote.LeniencyMs)
        {
            holdNote.Dropped = true;
            ShowDebugText($"dropped hold end after {holdNote.End.TimeMs - holdNote.LastHeldTimeMs}");
        }

        Judgement judgement = holdNote.Judge();
        ShowDebugText($"HoldNote\nStart: {holdNote.StartJudgement}\nCurrentlyHeld: {holdNote.CurrentlyHeld}\nHeld: {holdNote.Held}\nDropped: {holdNote.Dropped}");
        if (holdNote.CurrentlyHeld) NeedTouchHitsound = true;

        switch (judgement)
        {
            case Judgement.Marvelous:
            case Judgement.Great:
            case Judgement.Good:
            {
                IncrementCombo(holdNote.End.ChartTick);
                break;
            }
            case Judgement.Miss:
            {
                EndCombo(holdNote.End.ChartTick);
                break;
            }
            case Judgement.None:
            default:
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        ScoreData currentScoreData = CurrentScoreData();
        scoreText.UpdateScore(currentScoreData);
        centerDisplay.UpdateScore(currentScoreData);
        judgementDisplay.ShowJudgement(judgement, 0);

        return true;

    }

    // minNoteIndex tracks the first note that we need to care about when judging future inputs. It should be greater than
    // all notes whose windows have already fully passed or who have been hit.
    private int minNoteIndex;
    private readonly List<HoldNote> activeHolds = new();
    private float prevTouchTimeMs;
    private TouchState prevTouchState = TouchState.CreateNew();
    private TouchState newSegments = TouchState.CreateNew();

    // - HandleInput must be called with strictly increasing hitTimeMs for each consecutive call.
    // - HandleInput is not thread-safe. It is the caller's responsibility to ensure that multiple HandleInput
    //   invocations are never running in parallel. There is no guarantee that HandleInput will be called from the main
    //   thread or any specific thread.
    // - HandleInput may be called multiple times per frame, or multiple times in between frames in another thread.
    // - A call HandleInput(touchState1, time1), followed by a call HandleInput(touchState2, time2) will assume that
    //   touchState1 was held for the entire time between time1 and time2. And in turn, touchState2 will be assumed to
    //   be held until the next HandleInput call.
    //   Thus, consecutive calls with the same touchState are idempotent in terms of the final judgement. They may
    //   result in notes being assigned a judgement more quickly, but the judgements will ultimately be the same.
    // - HandleInput can also be called with a null touchState - this means that we know the previous input was held
    //   until at least this timeMs, and we can assign any judgements as appropriate based on that. (This is useful for
    //   triggering scoring updates during sparse input methods such as replays.)
    public void HandleInput(TouchState? touchState, float timeMs)
    {
        if (Notes is null)
        {
            Debug.LogError("Tried to judge an input, but no chart loaded");
            return;
        }

        TouchState currentTouchState = touchState ?? prevTouchState;

        try
        {
            currentTouchState.WriteSegmentsPressedSince(ref newSegments, prevTouchState);

            // Scan forward, looking for a note that can be hit by this input.
            for (int noteScanIndex = minNoteIndex; noteScanIndex < Notes.Count; noteScanIndex++)
            {
                Note note = Notes[noteScanIndex];

                if (timeMs + IgnoreFutureNotesThreshold < note.TimeMs)
                {
                    // We can stop scanning since this note or any future notes cannot be hit by this input.
                    break;
                }

                if (note.TimeMs + IgnorePastNotesThreshold < timeMs)
                {
                    // This note can no longer be hit, we don't need to ever look at it or any notes before it again.
                    minNoteIndex = noteScanIndex + 1;
                    // We continue on since we still may need to mark this note as missed ("MissHit" it).
                }

                MaybeCalculateHitForNote(timeMs, currentTouchState, note);
            }

            // Note: not using a foreach because we remove finished holds as we iterate
            for (int i = 0; i < activeHolds.Count;)
            {
                HoldNote holdNote = activeHolds[i];

                bool holdFinished = UpdateHold(timeMs, holdNote);

                if (holdFinished)
                    activeHolds.Remove(holdNote);
                else
                    i++;
            }
        }
        finally
        {
            prevTouchTimeMs = timeMs;
            // Don't use direct assignment, since the segment data may not be valid next call.
            currentTouchState.CopyTo(ref prevTouchState);
        }
    }

    private void Start()
    {
        // Disable automatic GC during gameplay to avoid lag spikes
        // Warning: if allocations are too high, this can cause OOM
        // Can't change GCMode in editor
        if (Application.isEditor) return;
        GarbageCollector.GCMode = GarbageCollector.Mode.Manual;
    }

    private async void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("judgements:");
            foreach (Note note in Notes?.Where(note => note.Judgement is not null) ?? Enumerable.Empty<Note>())
            {
                Debug.Log($"{note.Judgement} by {note.TimeErrorMs ?? 999}ms, note at {note.TimeMs}");
            }
        }

        // Warning: will not work if end of chart is after the end of the audio clip, OR if it is within one frame
        // of the end of the audio clip.
        // TODO: move this logic somewhere else lol
        if (Chart?.EndOfChart is not null && Chart.EndOfChart.TimeMs < timeManager.GameplayTimeMs &&
            !WritingReplayAndExiting)
        {
            // chart is done
            WritingReplayAndExiting = true;

            // re-enable GC now that gameplay is finished.
            if (!Application.isEditor)
                GarbageCollector.GCMode = GarbageCollector.Mode.Enabled;

            if (AutoWriteReplays && !replayManager.PlayingFromReplay)
                await replayManager.WriteReplayFile();

            PersistentStateManager.Instance.LastScoreData = CurrentScoreData();
            SceneSwitcher.Instance.LoadScene("_SongResults");
        }
    }
}

public struct ScoreData
{
    public int Score;
    public int MaxScore;
    public Dictionary<Judgement, int> JudgementCounts;
    public int EarlyCount;
    public Dictionary<Judgement, int> EarlyCountByJudgement;
    public int LateCount;
    public Dictionary<Judgement, int> LateCountByJudgement;
}
}
