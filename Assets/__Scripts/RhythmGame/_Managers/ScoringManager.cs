using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

namespace SaturnGame.RhythmGame
{
/// <summary>
/// ScoringManager handles input and registers judgement results.
/// </summary>
/// ScoringManager is stateful and expects to receive inputs in order.
/// A new ScoringManager should be used for each independent score.
public class ScoringManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI debugText;

    public bool AutoWriteReplays = true;

    // Only modify on main thread Update()
    public bool WritingReplayAndExiting;

    [Header("MANAGERS")]
    [SerializeField] private TimeManager timeManager;
    [SerializeField] private ChartManager chartManager;
    [SerializeField] private ReplayManager replayManager;

    private Chart Chart => chartManager.Chart;

    public int CurrentCombo;
    public int LastMissChartTick;
    public Judgement LastJudgement { get; private set; } = Judgement.None;
    public float? LastJudgementTimeMs { get; private set; }
    // LastJudgement and LastHitOffsetMs are split because some judgements do not have any early/late info, such as
    // chain notes or hold ends.
    public float? LastHitErrorMs { get; private set; }
    public float? LastHitErrorMsTimeMs { get; private set; }
    public bool NeedTouchHitsound;
    public bool NeedSwipeSnapHitsound;

    private string loadedChart;

    // Notes must be sorted by note TimeMs
    private List<Note> Notes => Chart.ProcessedNotesForGameplay;

    private void ShowDebugText(string text)
    {
        if (debugText is null)
            return;

        debugText.text = $"{timeManager.VisualTimeMs}\n" + text;
    }

    public ScoreData CurrentScoreData()
    {
        ScoreData ret = new()
        {
            Score = 0,
            JudgementCounts = new Dictionary<Judgement, int>
            {
                { Judgement.Marvelous, 0 },
                { Judgement.Great, 0 },
                { Judgement.Good, 0 },
                { Judgement.Miss, 0 },
            },
            EarlyCount = 0,
            LateCount = 0,
            EarlyCountByJudgement = new Dictionary<Judgement, int>
            {
                { Judgement.Marvelous, 0 },
                { Judgement.Great, 0 },
                { Judgement.Good, 0 },
            },
            LateCountByJudgement = new Dictionary<Judgement, int>
            {
                { Judgement.Marvelous, 0 },
                { Judgement.Great, 0 },
                { Judgement.Good, 0 },
            },
        };
        if (Notes is null || Notes.Count == 0) return ret;

        long maxScoreBeforeNormalization = 0;
        long scoreBeforeNormalization = 0;
        foreach (Note note in Notes)
        {
            int noteMaxScore = 100;
            int noteEarnedScore = note.Judgement switch
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
                noteEarnedScore *= 2;
            }

            maxScoreBeforeNormalization += noteMaxScore;
            scoreBeforeNormalization += noteEarnedScore;

            if (note.Judgement is null or Judgement.None) continue;

            ret.JudgementCounts[note.Judgement.Value]++;

            if (note is ChainNote || note.TimeErrorMs is null || note.Judgement is Judgement.Miss) continue;
            switch (note.TimeErrorMs)
            {
                case < 0:
                {
                    if (note.Judgement is not Judgement.Marvelous)
                    {
                        // Marvelous notes don't count toward overall early count.
                        ret.EarlyCount++;
                    }

                    ret.EarlyCountByJudgement[note.Judgement.Value]++;
                    break;
                }
                case > 0:
                {
                    if (note.Judgement is not Judgement.Marvelous)
                    {
                        // Marvelous notes don't count toward overall late count.
                        ret.LateCount++;
                    }

                    ret.LateCountByJudgement[note.Judgement.Value]++;
                    break;
                }
            }
        }

        ret.Score = maxScoreBeforeNormalization == 0
            ? 0
            :
            // Int conversion should be safe as max score is 1,000,000
            // (unless we fucked something up, then exception is appropriate anyway)
            Convert.ToInt32(scoreBeforeNormalization * 1_000_000L / maxScoreBeforeNormalization);

        return ret;
    }

    // This should be greater than the maximum late timing window of any note.
    private const float IgnorePastNotesThreshold = 300f;

    // This should be greater than the maximum early timing window of any note.
    private const float IgnoreFutureNotesThreshold = 300f;

    private void HitNote(float hitTimeMs, [NotNull] Note note, bool needSnapSwipeHitsound = false)
    {
        LastJudgement = note.Hit(hitTimeMs);
        LastJudgementTimeMs = hitTimeMs;
        if (note is not ChainNote && LastJudgement is not Judgement.Marvelous)
        {
            // TODO: option to allow showing Early/Late on Marvelous judgements
            LastHitErrorMs = note.TimeErrorMs;
            LastHitErrorMsTimeMs = hitTimeMs;
        }

        // HoldNotes affect combo at hold end
        if (note is not HoldNote)
            IncrementCombo(note.ChartTick);

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
    }

    private void EndCombo(int chartTick)
    {
        CurrentCombo = 0;
        LastMissChartTick = chartTick;
    }

    private void MaybeCalculateHitForNote(float hitTimeMs, [NotNull] TouchState touchState, [NotNull] Note note,
        [NotNull] TouchState newSegments)
    {
        if (note.IsHit) return;

        if (hitTimeMs < note.EarliestHitTimeMs)
        {
            // This note cannot be hit yet.
            return;
        }

        if (hitTimeMs >= note.LatestHitTimeMs)
        {
            // The note can no longer be hit.
            //Debug.Log($"Note {noteScanIndex}: Miss after {note.LatestHitTimeMs}");

            note.MissHit();
            LastJudgement = Judgement.Miss;
            LastJudgementTimeMs = hitTimeMs;
            // HoldNotes affect combo at hold end.
            if (note is not HoldNote)
                EndCombo(note.ChartTick);

            if (note is HoldNote holdNote)
                activeHolds.Add(holdNote);

            return;
        }

        // If we've reached this point, the note can be hit by this input.
        switch (note)
        {
            case TouchNote:
            {
                if (!note.Touched(newSegments)) break;

                HitNote(hitTimeMs, note);
                break;
            }
            case ChainNote chainNote:
            {
                if (chainNote.Touched(touchState))
                    chainNote.HasBeenTouched = true;

                if (chainNote.HasBeenTouched && hitTimeMs >= chainNote.TimeMs)
                    HitNote(hitTimeMs, chainNote);
                break;
            }
            case HoldNote holdNote:
            {
                if (!holdNote.Touched(newSegments)) break;

                //float errorMs = hitTimeMs - holdNote.TimeMs;
                //ShowDebugText($"{noteScanIndex} (hold): {errorMs} {holdNote.Left} {holdNote.Right}");

                HitNote(hitTimeMs, holdNote);
                activeHolds.Add(holdNote);
                break;
            }
            case SwipeNote swipeNote:
            {
                swipeNote.MaybeUpdateMinAverageOffset(touchState);
                if (!swipeNote.Swiped(touchState)) break;

                //float errorMs = hitTimeMs - swipeNote.TimeMs;
                //ShowDebugText($"{noteScanIndex} (swipe): {errorMs}");

                HitNote(hitTimeMs, swipeNote, needSnapSwipeHitsound: true);
                break;
            }
            case SnapNote snapNote:
            {
                if (!snapNote.Snapped(prevTouchState, touchState)) break;

                //float errorMs = hitTimeMs - snapNote.TimeMs;
                //ShowDebugText($"{noteScanIndex} (snap): {errorMs}");

                HitNote(hitTimeMs, snapNote, needSnapSwipeHitsound: true);
                break;
            }
        }
    }

    // Updates the current state of the HoldNote.
    // Returns true if the hold is completed and can be removed from further consideration.
    private bool UpdateHold(float hitTimeMs, [NotNull] TouchState touchState, [NotNull] HoldNote holdNote)
    {
        if (hitTimeMs < holdNote.Start.TimeMs)
        {
            // Hold was hit early, and we haven't started the actual hold body.
            // Skip doing any judgement on this hold for now.
            return false;
        }

        if (!holdNote.CurrentlyHeld)
        {
            // The note has been dropped for some time, calculate how long that is.
            // The drop window starts at lastHeldTimeMs.
            // The drop window is evaluated up until now, but not past the end of the note.
            float droppedUntil = Math.Min(hitTimeMs, holdNote.End.TimeMs);
            System.Diagnostics.Debug.Assert(holdNote.LastHeldTimeMs != null,
                "LastHeldTimeMs is null on an active hold");
            float dropTimeMs = droppedUntil - holdNote.LastHeldTimeMs.Value;
            if (dropTimeMs > HoldNote.LeniencyMs && !holdNote.Dropped)
            {
                ShowDebugText($"dropped hold after {dropTimeMs}");
                holdNote.Dropped = true;
            }
        }

        if (hitTimeMs > holdNote.End.TimeMs)
        {
            // Hold note is finished.
            Judgement judgement = holdNote.Judge();
            ShowDebugText(
                $"HoldNote\nStart: {holdNote.StartJudgement}\nHeld: {holdNote.Held}\nDropped: {holdNote.Dropped}");
            LastJudgement = judgement;
            LastJudgementTimeMs = hitTimeMs;
            if (holdNote.CurrentlyHeld)
                NeedTouchHitsound = true;
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
            }

            return true;
        }

        if (!holdNote.CurrentSegmentFor(hitTimeMs).Touched(touchState))
        {
            holdNote.CurrentlyHeld = false;
            return false;
        }

        holdNote.CurrentlyHeld = true;
        holdNote.LastHeldTimeMs = hitTimeMs;
        holdNote.Held = true;
        return false;
    }

    // minNoteIndex tracks the first note that we need to care about when judging future inputs. It should be greater than
    // all notes whose windows have already fully passed or who have been hit.
    private int minNoteIndex;
    private readonly List<HoldNote> activeHolds = new();
    private TouchState prevTouchState;

    public void HandleInput(TouchState touchState, float hitTimeMs)
    {
        if (Notes is null)
        {
            Debug.LogError("Tried to judge an input, but no chart loaded");
            return;
        }

        try
        {
            TouchState newSegments = touchState.SegmentsPressedSince(prevTouchState);

            // Scan forward, looking for a note that can be hit by this input.
            for (int noteScanIndex = minNoteIndex; noteScanIndex < Notes.Count; noteScanIndex++)
            {
                Note note = Notes[noteScanIndex];

                if (hitTimeMs + IgnoreFutureNotesThreshold < note.TimeMs)
                {
                    // We can stop scanning since this note or any future notes cannot be hit by this input.
                    break;
                }

                if (note.TimeMs + IgnorePastNotesThreshold < hitTimeMs)
                {
                    // This note can no longer be hit, we don't need to ever look at it or any notes before it again.
                    minNoteIndex = noteScanIndex + 1;
                    // We continue on since we still may need to mark this note as missed ("MissHit" it).
                }

                MaybeCalculateHitForNote(hitTimeMs, touchState, note, newSegments);
            }

            // Note: not using a foreach because we remove finished holds as we iterate
            for (int i = 0; i < activeHolds.Count;)
            {
                HoldNote holdNote = activeHolds[i];

                bool holdFinished = UpdateHold(hitTimeMs, touchState, holdNote);

                if (holdFinished)
                    activeHolds.Remove(holdNote);
                else
                    i++;
            }
        }
        finally
        {
            prevTouchState = touchState;
        }
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
        if (Chart?.EndOfChart is not null && Chart.EndOfChart.TimeMs < timeManager.VisualTimeMs &&
            !WritingReplayAndExiting)
        {
            async Awaitable endSong()
            {
                WritingReplayAndExiting = true;
                if (AutoWriteReplays && !replayManager.PlayingFromReplay)
                    await replayManager.WriteReplayFile();

                PersistentStateManager.Instance.LastScoreData = CurrentScoreData();
                SceneSwitcher.Instance.LoadScene("_SongResults");
            }
            // chart is done
            await endSong();
        }
    }
}

public struct ScoreData
{
    public int Score;
    public Dictionary<Judgement, int> JudgementCounts;
    public int EarlyCount;
    public int LateCount;
    public Dictionary<Judgement, int> EarlyCountByJudgement;
    public Dictionary<Judgement, int> LateCountByJudgement;
}
}