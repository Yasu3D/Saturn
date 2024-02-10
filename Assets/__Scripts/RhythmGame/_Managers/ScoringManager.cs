using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Rendering.Universal;
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
        private Chart Chart => ChartManager.Instance.chart;
        private ChartManager ChartManager => ChartManager.Instance;
        private TMPro.TextMeshProUGUI debugText => ViewRectController.Instance?.DebugText;
        [Header("MANAGERS")]
        [SerializeField] private TimeManager timeManager;

        public Judgement LastJudgement { get; private set; } = Judgement.None;
        public float? LastJudgementTimeMs { get; private set; } = null;

        private string loadedChart;
        // Notes must be sorted by note TimeMs
        private List<Note> notes;

        private void ShowDebugText(string text) {
            if (debugText is null)
                return;

            debugText.text = $"{timeManager.VisualTime}\n" + text;
        }

		public int CurrentScore() {
			if (notes is null || notes.Count() == 0)
			{
                return 0;
            }

            long maxScoreBeforeNormalization = 0;
            long scoreBeforeNormalization = 0;
			foreach (Note note in notes)
            {
                maxScoreBeforeNormalization += 100;
                if (note.Judgement is null)
                {
                    continue;
                }
                switch (note.Judgement)
                {
                    case Judgement.None:
                    case Judgement.Miss:
                        break;
                    case Judgement.Good:
                        scoreBeforeNormalization += 50;
                        break;
                    case Judgement.Great:
                        scoreBeforeNormalization += 70;
                        break;
                    case Judgement.Marvelous:
                        scoreBeforeNormalization += 100;
                        break;
                }
            }

			if (maxScoreBeforeNormalization == 0)
			{
				// Not sure how this should be possible but ok
                return 0;
            }

			// Int conversion should be safe as max score is 1,000,000
			// (unless we fucked something up, then exception is appropriate anyway)
            return Convert.ToInt32((scoreBeforeNormalization * 1_000_000L) / maxScoreBeforeNormalization);
        }

        // This should be greater than the maximum late timing window of any note.
        const float IgnorePastNotesThreshold = 300f;
        // This should be greater than the maximum early timing window of any note.
        const float IgnoreFutureNotesThreshold = 300f;

        bool segmentsOverlap(Note note1, Note note2)
        {
			if (note1.Left == note1.Right || note2.Left == note2.Right)
			{
				// Full circle notes always overlap
				return true;
			}

            // Bonus reading: https://fgiesen.wordpress.com/2015/09/24/intervals-in-modular-arithmetic/
            // Each note is a half-open interval in mod60.

            // See point_in_half_open_interval in the link.
            // This returns true iff point is in the half-open interval [note.Left, note.Right) (mod 60)
            bool pointWithinNote(Note note, int point)
            {
                return SaturnMath.Modulo(point - note.Left, 60) < SaturnMath.Modulo(note.Right - note.Left, 60);
            }

            // See half_open_intervals_overlap in the link.
            // We know that the interval size is positive, so we don't need to worry about that case.
            return pointWithinNote(note1, note2.Left) || pointWithinNote(note2, note1.Left);
        }

        // TODO: just move this into the normal chart loading (ChartManager)
        void LoadChart()
        {
            List<Note> allNotesFromChart = Chart.notes.Concat(Chart.holdNotes).OrderBy(note => note.TimeMs).ToList();
            // TODO: swipe notes within a hold... that is gonna be hell lmao
            // TODO: holds with a swipe on the hold start take on the timing window of the swipe??
            notes = new();

            foreach (Note note in allNotesFromChart)
            {
                note.EarliestHitTimeMs = note.TimeMs + note.HitWindows[^1].LeftMs;
                note.LatestHitTimeMs = note.TimeMs + note.HitWindows[^1].RightMs;

                List<Note> overlappingNotes = new();

                // Look backwards through the chart to see if any notes overlap with this.
                // Potential optimization: once a note is out of range (for all possible note type windows!),
                // we know it can never come back in range. We can keep track of the minimum index into the notes
                // array that we have to care about, like minNoteIndex below.
                foreach (Note otherNote in notes)
                {
                    if (otherNote.ChartTick == note.ChartTick)
                    {
                        // Don't try to do anything with notes that are at exactly the same time.
                        // Furthermore, we know we can't find any other notes from an earlier ChartTick, so just break out of the loop.
                        break;
                    }

                    if (otherNote.LatestHitTimeMs.Value > note.EarliestHitTimeMs.Value && segmentsOverlap(note, otherNote))
                    {
                        overlappingNotes.Add(otherNote);
                    }
                }

                if (overlappingNotes.Count > 0)
                {
                    // We have overlapping timing windows. Split the difference between the two closest notes.
                    Note latestNote = overlappingNotes.OrderByDescending(note => note.TimeMs).First();
                    // TODO: If the windows of the two notes are different sizes (e.g. touch vs swipe notes), bias the split point.
                    float cutoff = (latestNote.TimeMs + note.TimeMs) / 2;
                    note.EarliestHitTimeMs = cutoff;
                    foreach (Note otherNote in overlappingNotes)
                    {
                        otherNote.LatestHitTimeMs = Math.Min(otherNote.LatestHitTimeMs.Value, cutoff);
                    }
                }

                notes.Add(note);
            }
        }

        // minNoteIndex tracks the first note that we need to care about when judging future inputs. It should be greater than
        // all notes whose windows have already fully passed or who have been hit.
        int minNoteIndex = 0;
        List<HoldNote> activeHolds = new();
		TouchState prevTouchState;
        // TODO: This is currently super basic and assumes all the notes are touch notes.
        void HandleInput(float hitTimeMs, TouchState touchState)
        {
            if (notes is null)
            {
                Debug.LogError("Tried to judge an input, but no chart loaded");
                return;
            }

            try
            {
                var newSegments = touchState.SegmentsPressedSince(prevTouchState);

                int noteScanIndex = minNoteIndex;
                // Scan forward, looking for a note that can be hit by this input.
                while (noteScanIndex < notes.Count)
                {
                    Note note = notes[noteScanIndex];

                    if (note.TimeMs + IgnorePastNotesThreshold < hitTimeMs)
                    {
                        // This note can no longer be hit, we don't need to ever look at it or any notes before it again.
                        minNoteIndex = noteScanIndex + 1;
                        if (!note.Hit)
                        {
                            Debug.Log($"Note {noteScanIndex}: Miss after threshold {note.TimeMs + IgnorePastNotesThreshold}");
                            if (note is HoldNote holdNote)
                            {
                                holdNote.StartJudgement = Judgement.Miss;
                                // In this case, lastHeldTimeMs can be set to the beginning of the note, since
                                // that's when the hold leniency window should begin.
                                holdNote.LastHeldTimeMs = holdNote.TimeMs;
                                holdNote.CurrentlyHeld = false;
                                holdNote.Held = false;
                                holdNote.Dropped = false;

                                if (hitTimeMs < holdNote.End.TimeMs)
                                {
                                    activeHolds.Add(holdNote);
                                }
                                else
                                {
                                    holdNote.Dropped = true;
                                    // hold note is already over, judge
                                    holdNote.Judgement = Judgement.Miss;
                                }
                            }
                            else
                            {
                                note.Judgement = Judgement.Miss;
                                note.HitTimeMs = null;
                                LastJudgement = Judgement.Miss;
                                LastJudgementTimeMs = hitTimeMs;
                            }
                        }
                    }
                    else if (hitTimeMs + IgnoreFutureNotesThreshold < note.TimeMs)
                    {
                        // We can stop scanning since this note or any future notes cannot be hit by this input.
                        break;
                    }
                    else if (note.EarliestHitTimeMs <= hitTimeMs && hitTimeMs < note.LatestHitTimeMs && !note.Hit)
                    {
                        switch (note)
                        {
                            case SwipeNote:
                            case SnapNote:
                            case TouchNote:
                                if (note.Touched(newSegments))
                                {
                                    float errorMs = hitTimeMs - note.TimeMs;
                                    ShowDebugText($"{noteScanIndex}: {errorMs}");

                                    foreach (HitWindow hitWindow in note.HitWindows)
                                    {
                                        if (errorMs >= hitWindow.LeftMs && errorMs < hitWindow.RightMs)
                                        {
                                            note.Judgement = hitWindow.Judgement;
                                            note.HitTimeMs = hitTimeMs;
                                            LastJudgement = hitWindow.Judgement;
                                            LastJudgementTimeMs = hitTimeMs;
                                            break;
                                        }
                                    }
                                }
                                break;
                            case ChainNote:
                                // Warning: need to adjust judgement and hitsounds to play at the exact time of the note, even if it is hit early.
                                // Warning: even if the input is the same, this requires HandleInput to be called within the note's timing window. Ideally, any chain notes between this input and the last should be hit.
                                // Warning: currently, the timing window is totally wrong, it's huge.
                                if (note.Touched(touchState))
                                {
                                    ShowDebugText($"{noteScanIndex}: chain");
                                    note.Judgement = Judgement.Marvelous;
                                    note.HitTimeMs = hitTimeMs;
                                    LastJudgement = Judgement.Marvelous;
                                    LastJudgementTimeMs = hitTimeMs;
                                }
                                break;
                            case HoldNote holdNote:
                                if (holdNote.Touched(newSegments))
                                {
                                    float errorMs = hitTimeMs - holdNote.TimeMs;
                                    ShowDebugText($"{noteScanIndex} (hold): {errorMs} {holdNote.Left} {holdNote.Right}");

                                    foreach (HitWindow hitWindow in holdNote.HitWindows)
                                    {
                                        if (errorMs >= hitWindow.LeftMs && errorMs < hitWindow.RightMs)
                                        {
                                            holdNote.StartJudgement = hitWindow.Judgement;
                                            holdNote.HitTimeMs = hitTimeMs;
                                            holdNote.Held = true;
                                            holdNote.Dropped = false;
                                            holdNote.CurrentlyHeld = true;
                                            holdNote.LastHeldTimeMs = hitTimeMs;
                                            activeHolds.Add(holdNote);

                                            LastJudgement = hitWindow.Judgement;
                                            LastJudgementTimeMs = hitTimeMs;
                                            break;
                                        }
                                    }
                                }
                                break;
                        }
                    }
                    else if (hitTimeMs >= note.LatestHitTimeMs && !note.Hit)
                    {
                        // The note can no longer be hit.
                        Debug.Log($"Note {noteScanIndex}: Miss after {note.LatestHitTimeMs}");

                        // TODO: this is copy paste from the first if case
                        if (note is HoldNote holdNote)
                        {
                            holdNote.StartJudgement = Judgement.Miss;
                            // In this case, lastHeldTimeMs can be set to the beginning of the note, since
                            // that's when the hold leniency window should begin.
                            holdNote.LastHeldTimeMs = holdNote.TimeMs;
                            holdNote.CurrentlyHeld = false;
                            holdNote.Held = false;
                            holdNote.Dropped = false;

                            if (hitTimeMs < holdNote.End.TimeMs)
                            {
                                activeHolds.Add(holdNote);
                            }
                            else
                            {
                                // hold note is already over, judge
                                holdNote.Judgement = Judgement.Miss;
                            }
                        }
                        else
                        {
                            note.Judgement = Judgement.Miss;
                            note.HitTimeMs = null;
                            LastJudgement = Judgement.Miss;
                            LastJudgementTimeMs = hitTimeMs;
                        }
                    }

                    noteScanIndex++;
                }

                // Note: not using a foreach because we remove finished holds as we iterate
                for (int i = 0; i < activeHolds.Count; i++)
                {
                    HoldNote holdNote = activeHolds[i];

                    if (hitTimeMs < holdNote.Start.TimeMs)
                    {
                        // Hold was hit early, and we haven't started the actual hold body.
                        // Skip doing any judgement on this hold for now.
                        continue;
                    }

                    if (!holdNote.CurrentlyHeld)
                    {
                        // The note has been dropped for some time, calculate how long that is.
                        // The drop window starts at lastHeldTimeMs.
                        // The drop window is evaluated up until now, but not past the end of the note.
                        float droppedUntil = Math.Min(hitTimeMs, holdNote.End.TimeMs);
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
                        LastJudgement = judgement;
                        LastJudgementTimeMs = hitTimeMs;
                        activeHolds.Remove(holdNote);

                        // Since we removed an element, the current index should go back one
                        i--;
                    }
                    else if (holdNote.CurrentSegmentFor(hitTimeMs).Touched(touchState))
                    {
                        holdNote.CurrentlyHeld = true;
                        holdNote.LastHeldTimeMs = hitTimeMs;
                        holdNote.Held = true;
                    }
                    else
                    {
                        holdNote.CurrentlyHeld = false;
                    }
                }
            }
			finally
			{
                prevTouchState = touchState;
            }
        }

		public void NewTouchState(TouchState touchState) {
			HandleInput(timeManager.VisualTime, touchState);
		}

        void Update()
        {
            // Maybe find a way to not call this *every frame*. - yasu
            // (It's not super problematic for now but I think it may make more sense to call this once when starting a song.)

            // note: maybe race if we don't hold LoadingLock here
            if (!ChartManager.Loading && ChartManager.LoadedChart is not null && ChartManager.LoadedChart != loadedChart)
            {
                loadedChart = ChartManager.LoadedChart;
                LoadChart();
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                HandleInput(timeManager.VisualTime, null);
                Debug.Log("judgements:");
                foreach (var note in notes)
                {
                    if (note.Judgement is not null)
                    {
                        Debug.Log($"{note.Judgement} by {note.TimeErrorMs ?? 999}ms, note at {note.TimeMs}");
                    }
                }
            }
        }
    }
}
