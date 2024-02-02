using System;
using System.Collections;
using System.Collections.Generic;
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
        [Header("MANAGERS")]
        [SerializeField] private TimeManager timeManager;

        private string loadedChart;
        // Notes must be sorted by note TimeMs
        private List<ScoringNote> notes;

        // A list of judgement windows, from smallest to largest.
        // Each judgement window is tuple of minimum error in ms, maximum error in ms, and the earned Judgement
        // TODO: Make this better lol, create a class or something.
        readonly (float left, float right, Judgement judgement)[] TouchNoteJudgementWindows =
        {
            // TODO: tweak these values
            (-45f, 45f, Judgement.Marvelous),
            (-90f, 90f, Judgement.Great),
            (-180f, 180f, Judgement.Good),
            // There is no early or late Miss window
        };
        // This should be greater than the maximum late timing window of any note.
        const float IgnorePastNotesThreshold = 300f;
        // This should be greater than the maximum early timing window of any note.
        const float IgnoreFutureNotesThreshold = 300f;

        bool segmentsOverlap(ScoringNote note1, ScoringNote note2)
        {
            // Returns x%m that is always between 0 and m-1 inclusive.
            int saneMod(int x, int m)
            {
                // https://stackoverflow.com/questions/1082917/mod-of-negative-number-is-melting-my-brain
                int r = x % m;
                return r < 0 ? r + m : r;
            }

            // Given 3 positions mod 60, return true if these are in "ascending" order without going around the circle a full time.
            // 0 -> 15 -> 30: true
            // 0 -> 30 -> 15: false
            // 45 -> 0 -> 15: true
            // 15 -> 0 -> 45: false
            // 15 -> 0 -> 10: true
            // This is equivalent to "pos2 is in the interval (pos1, pos3)" in mod60 arithmetic.
            bool inOrder(int pos1, int pos2, int pos3)
            {
                return saneMod(pos2 - pos1, 60) < saneMod(pos3 - pos1, 60);
            }

            // To test for overlap, check to see if the left side of either note is within the other.
            // Bonus reading: https://fgiesen.wordpress.com/2015/09/24/intervals-in-modular-arithmetic/
            return inOrder(note1.Left, note2.Left, note1.Right) || inOrder(note2.Left, note1.Right, note2.Right);
        }

        void LoadChart()
        {
            // TODO: other types of notes (e.g. hold notes)
            // TOOD: notes on top of each other (may only be legal for hold notes)
            notes = new();

            foreach (SimpleNote note in Chart.notes)
            {
                ScoringNote scoringNote = new(note);
                scoringNote.EarliestTimeMs = note.TimeMs + TouchNoteJudgementWindows[^1].left;
                scoringNote.LatestTimeMs = note.TimeMs + TouchNoteJudgementWindows[^1].right;

                // Look backwards through the chart to see if any notes overlap with this.
                // Potential optimization: once a note is out of range (for all possible note type windows!),
                // we know it can never come back in range. We can keep track of the minimum index into the notes
                // array that we have to care about, like minNoteIndex below.
                foreach (ScoringNote otherNote in notes)
                {
                    if (otherNote.LatestTimeMs > scoringNote.EarliestTimeMs && segmentsOverlap(scoringNote, otherNote))
                    {
                        // We have overlapping timing windows. Split the difference between the two notes.
                        // TODO: If the windows of the two notes are different sizes (e.g. touch vs swipe notes), bias the split point.
                        var cutoff = (otherNote.Note.TimeMs + note.TimeMs) / 2;
                        otherNote.LatestTimeMs = cutoff;
                        scoringNote.EarliestTimeMs = cutoff;

                        // We only expect one previous note to possibly overlap this one, so exit the loop;
                        break;
                    }
                }

                notes.Add(scoringNote);
            }
        }

        // minNoteIndex tracks the first note that we need to care about when judging future inputs. It should be greater than
        // all notes whose windows have already fully passed or who have been hit.
        int minNoteIndex = 0;
        // TODO: This is currently super basic and assumes all the notes are touch notes and all the inputs cover all segments.
        void HandleInput(float hitTimeMs)
        {
            // State that we care about:
            // - previously active segments
            // - notes that have already been judged
            if (notes is null)
            {
                Debug.LogError("Tried to judge an input, but no chart loaded");
                return;
            }

            // Warning: this maybe requires all notes to be hit in order. E.g. if note 2 is one tick after note 1,
            // you cannot hit note 2 before note 1, even if they are on opposite sides of the ring.
            // This should probably be fixed when we add segment awareness.

            int noteScanIndex = minNoteIndex;
            // Scan forward, looking for a note that can be hit by this input.
            while (noteScanIndex < notes.Count)
            {
                ScoringNote note = notes[noteScanIndex];

                if (note.Note.TimeMs + IgnorePastNotesThreshold < hitTimeMs)
                {
                    // This note can no longer be hit, we don't need to ever look at it or any notes before it again.
                    minNoteIndex = noteScanIndex + 1;
                    if (note.JudgementResult is null)
                    {
                        Debug.Log($"Note {noteScanIndex}: Miss after threshold {note.Note.TimeMs + IgnorePastNotesThreshold}");
                        note.JudgementResult = new(Judgement.Miss, null, note.Note);
                    }
                }
                else if (hitTimeMs + IgnoreFutureNotesThreshold < note.Note.TimeMs)
                {
                    // We can stop scanning since this note or any future notes cannot be hit by this input.
                    break;
                }
                // TODO: add segment checks here
                else if (note.EarliestTimeMs <= hitTimeMs && hitTimeMs < note.LatestTimeMs && note.JudgementResult is null)
                {
                    float errorMs = hitTimeMs - note.Note.TimeMs;
                    Debug.Log($"Note {noteScanIndex}: judging with offset {errorMs}");

                    foreach (var judgementWindow in TouchNoteJudgementWindows)
                    {
                        if (errorMs >= judgementWindow.left && errorMs < judgementWindow.right)
                        {
                            note.JudgementResult = new JudgementResult(judgementWindow.judgement, hitTimeMs, note.Note);
                            Debug.Log($"result: {note.JudgementResult.Judgement}");
                            break;
                        }
                    }
                }
                else if (hitTimeMs >= note.LatestTimeMs && note.JudgementResult is null)
                {
                    // The note can no longer be hit.
                    Debug.Log($"Note {noteScanIndex}: Miss after {note.LatestTimeMs}");
                    note.JudgementResult = new(Judgement.Miss, null, note.Note);
                }

                noteScanIndex++;
            }
        }

        void Update()
        {
            // note: maybe race if we don't hold LoadingLock here
            if (!ChartManager.Loading && ChartManager.LoadedChart is not null && ChartManager.LoadedChart != loadedChart)
            {
                loadedChart = ChartManager.LoadedChart;
                LoadChart();
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                HandleInput(timeManager.RawVisualTime);
                Debug.Log("judgements:");
                foreach (var note in notes)
                {
                    if (note.JudgementResult is not null)
                    {
                        Debug.Log($"{note.JudgementResult.Judgement} by {note.JudgementResult.TimeErrorMs ?? 999}ms, note at {note.JudgementResult.Note.TimeMs}");
                    }
                }
            }
        }

        /// <summary>
        /// This is a Note with some additional metadata used by the scoring manager.
        /// This is an internal class since it should only be used by the ScoringManager.
        /// </summary>
        private class ScoringNote
        {
            public readonly SimpleNote Note;
            public JudgementResult JudgementResult;
            // We can define a note as an interval in mod 60, e.g. [40, 50) for a 10-size note at position 40.
            // If the note crosses 60, still use mod 60 integers, e.g. [55, 5) for a 10-size note at position 55.
            // Left is the beginning or left side of this interval. It is the clockwise-most segment of the note. (Note, that might be the "right" side of the note if it's on the top of the ring.)
            public readonly int Left;
            // Left is the end or right side of this interval. It is the counterclockwise-most segment of the note + 1.
            public readonly int Right;
            public float? EarliestTimeMs;
            public float? LatestTimeMs;

            public ScoringNote(SimpleNote note)
            {
                Note = note;
                Left = note.Position;
                Right = (note.Position + note.Size) % 60;
            }
        }
    }

    class JudgementResult
    {
        public Judgement Judgement { get; }

        // TODO: This is not naively well-defined for HoldNotes - consider how HoldNotes should be represented.
        // Can be null if there was no hit associated with this judgement (e.g. miss)
        public float? HitTimeMs { get; }

        // FIXME: HoldNote is not a Note, so this cannot capture HoldNotes
        public SimpleNote Note { get; }

        // The error in ms of this input compared to a perfectly-timed input.
        // e.g. 5ms early will give a value of -5
        public float? TimeErrorMs
        {
            get
            {
                if (HitTimeMs is null)
                    return null;
                else
                    return HitTimeMs - Note.TimeMs;
            }
        }

        public JudgementResult(Judgement judgement, float? hitTimeMs, SimpleNote note)
        {
            Judgement = judgement;
            HitTimeMs = hitTimeMs;
            Note = note;
        }
    }

    enum Judgement
    {
        None, // Represents cases where the judgement is missing entirely, e.g. for a note that did not receive any judgement.
        Miss,
        Good,
        Great,
        Marvelous
    }
}
