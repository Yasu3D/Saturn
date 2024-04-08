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
    [SerializeField] private TextMeshProUGUI DebugText;
    public bool AutoWriteReplays = true;
    public bool WritingReplayAndExiting; // Only modify on main thread Update()
    
    [Header("MANAGERS")]
    [SerializeField] private TimeManager TimeManager;
    [SerializeField] private ChartManager ChartManager;
    [SerializeField] private ReplayManager ReplayManager;

    [Space(10)]
    
    public int CurrentCombo;
    public int LastMissChartTick;
    public float? LastHitErrorMs { get; private set; }
    public float? LastHitTimeMs { get; private set; }
    public bool NeedTouchHitsound;
    public bool NeedSwipeSnapHitsound;

    [Header("VISUALS")]
    [SerializeField] private CenterDisplay CenterDisplay; 
    [SerializeField] private JudgementDisplay JudgementDisplay;
    [SerializeField] private ScoreNumberText ScoreText;
    
    private string loadedChart;
    private Chart Chart => ChartManager.Chart;

    // Notes must be sorted by note TimeMs
    private List<Note> Notes => Chart.ProcessedNotesForGameplay;

    private void ShowDebugText(string text)
    {
        if (DebugText is null)
            return;

        DebugText.text = $"{TimeManager.VisualTimeMs}\n" + text;
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
        
        if (note is not ChainNote && lastJudgement is not Judgement.Marvelous)
        {
            LastHitErrorMs = note.TimeErrorMs;
            LastHitTimeMs = hitTimeMs;
        }

        // HoldNotes affect combo at hold end
        if (note is not HoldNote) IncrementCombo(note.ChartTick);
        
        ScoreText.UpdateScore(currentScoreData);
        CenterDisplay.UpdateScore(currentScoreData);
        JudgementDisplay.ShowJudgement(lastJudgement, LastHitErrorMs ?? 0);
        
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

        CenterDisplay.UpdateCombo(CurrentCombo);
    }

    private void EndCombo(int chartTick)
    {
        CurrentCombo = 0;
        LastMissChartTick = chartTick;
        
        CenterDisplay.UpdateCombo(CurrentCombo);
    }

    private void MaybeCalculateHitForNote(float hitTimeMs, TouchState touchState, [NotNull] Note note)
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
            // HoldNotes affect combo at hold end.
            if (note is not HoldNote)
                EndCombo(note.ChartTick);

            if (note is HoldNote holdNote)
                activeHolds.Add(holdNote);
            
            ScoreData currentScoreData = CurrentScoreData();
            ScoreText.UpdateScore(currentScoreData);
            CenterDisplay.UpdateScore(currentScoreData);
            JudgementDisplay.ShowJudgement(Judgement.Miss, 0);
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
    private bool UpdateHold(float hitTimeMs, TouchState touchState, [NotNull] HoldNote holdNote)
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
            ShowDebugText($"HoldNote\nStart: {holdNote.StartJudgement}\nHeld: {holdNote.Held}\nDropped: {holdNote.Dropped}");
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
            }

            ScoreData currentScoreData = CurrentScoreData();
            ScoreText.UpdateScore(currentScoreData);
            CenterDisplay.UpdateScore(currentScoreData);
            JudgementDisplay.ShowJudgement(judgement, 0);
            
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
    private TouchState prevTouchState = TouchState.CreateNew();
    private TouchState newSegments = TouchState.CreateNew();

    public void HandleInput(TouchState touchState, float hitTimeMs)
    {
        if (Notes is null)
        {
            Debug.LogError("Tried to judge an input, but no chart loaded");
            return;
        }

        try
        {
            touchState.WriteSegmentsPressedSince(ref newSegments, prevTouchState);

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

                MaybeCalculateHitForNote(hitTimeMs, touchState, note);
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
            // Don't use direct assignment, since the segment data may not be valid next call.
            touchState.CopyTo(ref prevTouchState);
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
        if (Chart?.EndOfChart is not null && Chart.EndOfChart.TimeMs < TimeManager.VisualTimeMs &&
            !WritingReplayAndExiting)
        {
            // chart is done
            WritingReplayAndExiting = true;

            // re-enable GC now that gameplay is finished.
            if (!Application.isEditor)
                GarbageCollector.GCMode = GarbageCollector.Mode.Enabled;
                
            if (AutoWriteReplays && !ReplayManager.PlayingFromReplay)
                await ReplayManager.WriteReplayFile();

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
