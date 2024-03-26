using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using CompressionLevel = System.IO.Compression.CompressionLevel;
using ErrorEventArgs = Newtonsoft.Json.Serialization.ErrorEventArgs;

namespace SaturnGame.RhythmGame
{
/// <summary>
/// ScoringManager handles input and registers judgement results.
/// </summary>
/// ScoringManager is stateful and expects to receive inputs in order.
/// A new ScoringManager should be used for each independent score.
public class ScoringManager : MonoBehaviour
{
    private static ChartManager ChartManager => ChartManager.Instance;
    private static Chart Chart => ChartManager.Chart;
    [SerializeField] private TextMeshProUGUI debugText;

    public bool AutoWriteReplays = true;

    // Only modify on main thread Update()
    public bool WritingReplayAndExiting;

    [Header("MANAGERS")] [SerializeField] private TimeManager timeManager;

    // If PlayingFromReplay is true, all inputs are ignored, and scoring data is read from Replay instead.
    // If false, gameplay is normal and inputs are stored into Replay as they happen.
    private bool PlayingFromReplay { get; set; }
    private int replayFrameIndex = -1;

    private struct ReplayFrame
    {
        public float TimeMs;
        public TouchState TouchState;
    }

    private List<ReplayFrame> Replay { get; set; } = new();

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
    private List<Note> notes;

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
        if (notes is null || notes.Count == 0)
        {
            return ret;
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
                {
                    break;
                }
                case Judgement.Good:
                {
                    scoreBeforeNormalization += 50;
                    break;
                }
                case Judgement.Great:
                {
                    scoreBeforeNormalization += 70;
                    break;
                }
                case Judgement.Marvelous:
                {
                    scoreBeforeNormalization += 100;
                    break;
                }
            }

            if (note.Judgement is not Judgement.None)
            {
                ret.JudgementCounts[note.Judgement.Value]++;
                if (note is not ChainNote && note.TimeErrorMs is not null && note.Judgement is not Judgement.Miss)
                {
                    if (note.TimeErrorMs < 0)
                    {
                        if (note.Judgement is not Judgement.Marvelous)
                        {
                            // Marvelous notes don't count toward overall early count.
                            ret.EarlyCount++;
                        }

                        ret.EarlyCountByJudgement[note.Judgement.Value]++;
                    }
                    else if (note.TimeErrorMs > 0)
                    {
                        if (note.Judgement is not Judgement.Marvelous)
                        {
                            // Marvelous notes don't count toward overall late count.
                            ret.LateCount++;
                        }

                        ret.LateCountByJudgement[note.Judgement.Value]++;
                    }
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

    private static bool SegmentsOverlap([NotNull] PositionedChartElement note1, [NotNull] PositionedChartElement note2)
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
        bool pointWithinNote([NotNull] PositionedChartElement note, int point)
        {
            return SaturnMath.Modulo(point - note.Left, 60) < SaturnMath.Modulo(note.Right - note.Left, 60);
        }

        // See half_open_intervals_overlap in the link.
        // We know that the interval size is positive, so we don't need to worry about that case.
        return pointWithinNote(note1, note2.Left) || pointWithinNote(note2, note1.Left);
    }

    // TODO: just move this into the normal chart loading (ChartManager)
    private void LoadChart()
    {
        List<Note> allNotesFromChart = Chart.Notes.Concat(Chart.HoldNotes).OrderBy(note => note.TimeMs).ToList();
        // TODO: swipe notes within a hold... that is gonna be hell lmao
        // TODO: holds with a swipe on the hold start take on the timing window of the swipe??
        notes = new List<Note>();

        foreach (Note note in allNotesFromChart)
        {
            note.EarliestHitTimeMs = note.TimeMs + note.HitWindows[^1].LeftMs;
            note.LatestHitTimeMs = note.TimeMs + note.HitWindows[^1].RightMs;

            if (Chart.HoldNotes.Any(holdNote =>
                    holdNote.End.ChartTick == note.ChartTick && SegmentsOverlap(holdNote.End, note)))
            {
                // Notes that overlap with a hold end should lose their early window (except Marvelous).
                note.EarliestHitTimeMs =
                    Math.Max(note.EarliestHitTimeMs.Value, note.TimeMs + note.HitWindows[0].LeftMs);
            }

            // Look backwards through the chart to see if any notes overlap with this.
            // Potential optimization: once a note is out of range (for all possible note type windows!),
            // we know it can never come back in range. We can keep track of the minimum index into the notes
            // array that we have to care about, like minNoteIndex below.
            List<Note> overlappingNotes = notes
                // Don't try to do anything with notes that are at exactly the same time. Furthermore, we know we
                // can't find any other notes from an earlier ChartTick, so just break out of the loop.
                .TakeWhile(otherNote => otherNote.ChartTick != note.ChartTick)
                .Where(otherNote =>
                {
                    // This should always be true but we check it anyway to shut up the compiler.
                    System.Diagnostics.Debug.Assert(otherNote.LatestHitTimeMs != null,
                        "already added note doesn't have LatestHitTimeMs");

                    return otherNote.LatestHitTimeMs.Value > note.EarliestHitTimeMs.Value &&
                           SegmentsOverlap(note, otherNote);
                })
                .ToList();

            if (overlappingNotes.Any())
            {
                // We have overlapping timing windows. Split the difference between the two closest notes.
                Note latestNote = overlappingNotes.OrderByDescending(overlappingNote => overlappingNote.TimeMs).First();
                // TODO: If the windows of the two notes are different sizes (e.g. touch vs swipe notes), bias the split point.
                // Use Max here to avoid expanding the window here if it's already truncated.
                // This should probably actually just split the overlap - otherwise it's possible to have some part with no window.
                // (E.g. if the latestNote is already truncated on the right side.)
                // TODO: it's possible that the LatestHitTimeMs of the latest note is already truncated and before
                // the cutoff, in which case we may have some period of time between the notes with no window.
                // Think of a simpler way to represent this logic.
                float cutoff = Math.Max(note.EarliestHitTimeMs.Value, (latestNote.TimeMs + note.TimeMs) / 2);
                note.EarliestHitTimeMs = cutoff;
                foreach (Note otherNote in overlappingNotes)
                    otherNote.LatestHitTimeMs = Math.Min(otherNote.LatestHitTimeMs!.Value, cutoff);
            }

            notes.Add(note);
        }
    }

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
            case ChainNote:
            {
                if (!note.Touched(touchState)) break;

                // Warning: need to adjust judgement and hitsounds to play at the exact time of the note, even if it is hit early.
                // Warning: even if the input is the same, this requires HandleInput to be called within the note's timing window.
                // Ideally, any chain notes between this input and the last should be hit.
                // Warning: currently, the timing window is totally wrong, it's huge.
                //ShowDebugText($"{noteScanIndex}: chain");
                HitNote(hitTimeMs, note);
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

    private void HandleInput(float hitTimeMs, TouchState touchState)
    {
        if (notes is null)
        {
            Debug.LogError("Tried to judge an input, but no chart loaded");
            return;
        }

        try
        {
            TouchState newSegments = touchState.SegmentsPressedSince(prevTouchState);

            // Scan forward, looking for a note that can be hit by this input.
            for (int noteScanIndex = minNoteIndex; noteScanIndex < notes.Count; noteScanIndex++)
            {
                Note note = notes[noteScanIndex];

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


    public void NewTouchState(TouchState touchState)
    {
        if (PlayingFromReplay)
        {
            return;
        }

        Replay.Add(new ReplayFrame { TimeMs = timeManager.VisualTimeMs, TouchState = touchState });
        HandleInput(timeManager.VisualTimeMs, touchState);
    }

    private void Update()
    {
        // Maybe find a way to not call this *every frame*. - yasu
        // (It's not super problematic for now but I think it may make more sense to call this once when starting a song.)

        // note: maybe race if we don't hold LoadingLock here
        if (!ChartManager.Loading && ChartManager.LoadedChart is not null && ChartManager.LoadedChart != loadedChart)
        {
            LoadChart();
            loadedChart = ChartManager.LoadedChart;
        }

        if (PlayingFromReplay && replayFrameIndex >= 0 && loadedChart == ChartManager.LoadedChart && notes is not null)
        {
            while (replayFrameIndex < Replay.Count && Replay[replayFrameIndex].TimeMs <= timeManager.VisualTimeMs)
            {
                HandleInput(Replay[replayFrameIndex].TimeMs, Replay[replayFrameIndex].TouchState);
                replayFrameIndex++;
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("judgements:");
            foreach (Note note in notes?.Where(note => note.Judgement is not null) ?? Enumerable.Empty<Note>())
            {
                Debug.Log($"{note.Judgement} by {note.TimeErrorMs ?? 999}ms, note at {note.TimeMs}");
            }
        }

        // Warning: will not work if end of chart is after the end of the audio clip, OR if it is within one frame
        // of the end of the audio clip.
        if (Chart?.EndOfChart is not null && Chart.EndOfChart.TimeMs < timeManager.VisualTimeMs &&
            !WritingReplayAndExiting)
        {
            async Awaitable endSong()
            {
                WritingReplayAndExiting = true;
                if (AutoWriteReplays)
                {
                    await WriteReplayFile();
                }

                ChartManager.Instance.LastScoreData = CurrentScoreData();
                SceneSwitcher.Instance.LoadScene("_SongResults");
            }
            // chart is done
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            // Don't await within Update()
            endSong();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        if (Input.GetKeyDown(KeyCode.F12))
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            // Don't await within Update()
            WriteReplayFile();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        if (Input.GetKeyDown(KeyCode.F11))
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            // Don't await within Update()

            // For now, copy the replay you want to view to "replay.json.gz" in the persistentDataPath.
            ReadReplayFile(Path.Combine(Application.persistentDataPath, "replay.json.gz"));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }
    }

    private static void JsonError(object sender, [NotNull] ErrorEventArgs errorArgs)
    {
        Debug.LogError(errorArgs.ErrorContext.Error);
        errorArgs.ErrorContext.Handled = true;
    }

    // TODO maybe: stream to file continuously rather than all at the end
    private async Awaitable WriteReplayFile()
    {
        string chartRelativePath = Path.GetRelativePath(Application.streamingAssetsPath, ChartManager.LoadedChart);
        string timestamp = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
        string replayFileName = $"replay-{chartRelativePath}-{timestamp}.json.gz";
        string escapedReplayFileName = String.Join('_', replayFileName.Split(Path.GetInvalidFileNameChars()));
        string replayPath = Path.Combine(Application.persistentDataPath, escapedReplayFileName);
        Debug.Log($"Writing replay with {Replay.Count} frames to {replayPath}...");
        await using FileStream replayFileStream = File.Create(replayPath);
        await using GZipStream compressedStream = new(replayFileStream, CompressionLevel.Fastest);
        await using StreamWriter writer = new(compressedStream);
        JsonSerializer serializer = new();
        serializer.Error += JsonError;
        // "threadsafe" here is not entirely accurate, but this should hopefully allow us to take a reference
        // to the Replay that will _not_ be modified as more frames are added, so the JsonSerializer can safely
        // iterate over it.
        IEnumerable<ReplayFrame> threadsafeReplay = Replay.Take(Replay.Count);
        await Awaitable.BackgroundThreadAsync();
        serializer.Serialize(writer, threadsafeReplay);
        await Awaitable.MainThreadAsync();
        Debug.Log($"Replay {replayFileName} successfully written!");
    }

    private async Awaitable ReadReplayFile([NotNull] string replayPath)
    {
        Debug.Log($"Reading replay from {replayPath}");
        PlayingFromReplay = true;
        await using FileStream replayFileStream = File.OpenRead(replayPath);
        await using GZipStream compressedStream = new(replayFileStream, CompressionMode.Decompress);
        using StreamReader streamReader = new(compressedStream);
        JsonSerializer serializer = new();
        serializer.Error += JsonError;
        await Awaitable.BackgroundThreadAsync();
        List<ReplayFrame> readReplay =
            (List<ReplayFrame>)serializer.Deserialize(streamReader, typeof(List<ReplayFrame>));
        await Awaitable.MainThreadAsync();
        if (readReplay is null)
        {
            PlayingFromReplay = false;
            throw new Exception("Failed to read replay");
        }

        Replay = readReplay;
        Debug.Log($"Loaded replay {replayPath} with {Replay.Count} frames");
        replayFrameIndex = 0;
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