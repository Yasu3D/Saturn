using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Newtonsoft.Json;
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
        [SerializeField] private TMPro.TextMeshProUGUI DebugText;
        public bool AutoWriteReplays = true;
        // Only modify on main thread Update()
        public bool WritingReplayAndExiting = false;

        [Header("MANAGERS")]
        [SerializeField] private TimeManager timeManager;

        // If PlayingFromReplay is true, all inputs are ignored, and scoring data is read from Replay instead.
        // If false, gameplay is normal and inputs are stored into Replay as they happen.
        public bool PlayingFromReplay { get; private set; } = false;
        private int replayFrameIndex = -1;
        public struct ReplayFrame
        {
            public float TimeMs;
            public TouchState TouchState;
        }
        public List<ReplayFrame> Replay { get; private set; } = new();

        public Judgement LastJudgement { get; private set; } = Judgement.None;
        public float? LastJudgementTimeMs { get; private set; } = null;
        public bool NeedTouchHitsound = false;
        public bool NeedSwipeSnapHitsound = false;

        private string loadedChart;
        // Notes must be sorted by note TimeMs
        private List<Note> notes;

        private void ShowDebugText(string text) {
            if (DebugText is null)
                return;

            DebugText.text = $"{timeManager.VisualTimeMs}\n" + text;
        }

        public ScoreData CurrentScoreData()
        {
            ScoreData ret = new ScoreData
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

            ret.Score = maxScoreBeforeNormalization == 0 ? 0 :
                // Int conversion should be safe as max score is 1,000,000
                // (unless we fucked something up, then exception is appropriate anyway)
                Convert.ToInt32(scoreBeforeNormalization * 1_000_000L / maxScoreBeforeNormalization);

            return ret;
        }

        // This should be greater than the maximum late timing window of any note.
        const float IgnorePastNotesThreshold = 300f;
        // This should be greater than the maximum early timing window of any note.
        const float IgnoreFutureNotesThreshold = 300f;

        bool segmentsOverlap(PositionedChartElement note1, PositionedChartElement note2)
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
            bool pointWithinNote(PositionedChartElement note, int point)
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

                foreach (HoldNote holdNote in Chart.holdNotes)
                {
                    if (holdNote.End.ChartTick == note.ChartTick && segmentsOverlap(holdNote.End, note))
                    {
                        // Notes that overlap with a hold end should lose their early window (except Marvelous).
                        note.EarliestHitTimeMs =
                            Math.Max(note.EarliestHitTimeMs.Value, note.TimeMs + note.HitWindows[0].LeftMs);
                        break;
                    }
                }

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
                    // Use Max here to avoid expanding the window here if it's already truncated.
                    // This should probably actually just split the overlap - otherwise it's possible to have some part with no window.
                    // (E.g. if the latestNote is already truncated on the right side.)
                    float cutoff = Math.Max(note.EarliestHitTimeMs.Value, (latestNote.TimeMs + note.TimeMs) / 2);
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
        private void HandleInput(float hitTimeMs, TouchState touchState)
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
                            //Debug.Log($"Note {noteScanIndex}: Miss after threshold {note.TimeMs + IgnorePastNotesThreshold}");
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
                                            NeedTouchHitsound = true;
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
                                    NeedTouchHitsound = true;
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
                                            NeedTouchHitsound = true;
                                            break;
                                        }
                                    }
                                }
                                break;
                            case SwipeNote swipeNote:
                                swipeNote.MaybeUpdateMinAverageOffset(touchState);
                                if (swipeNote.Swiped(touchState))
                                {
                                    float errorMs = hitTimeMs - swipeNote.TimeMs;
                                    ShowDebugText($"{noteScanIndex} (swipe): {errorMs}");

                                    foreach (HitWindow hitWindow in swipeNote.HitWindows)
                                    {
                                        if (errorMs >= hitWindow.LeftMs && errorMs < hitWindow.RightMs)
                                        {
                                            swipeNote.Judgement = hitWindow.Judgement;
                                            swipeNote.HitTimeMs = hitTimeMs;
                                            LastJudgement = hitWindow.Judgement;
                                            LastJudgementTimeMs = hitTimeMs;
                                            NeedTouchHitsound = true;
                                            NeedSwipeSnapHitsound = true;
                                            break;
                                        }
                                    }
                                }
                                break;
                            case SnapNote snapNote:
                                // yeah yeah I know this needs to be broken up
                                bool CheckDepthChangeInRange(int rangeLeft, int rangeSize)
                                {
                                    int? prevMin = null;
                                    int? curMin = null;
                                    int? prevMax = null;
                                    int? curMax = null;
                                    foreach (int offset in Enumerable.Range(0, rangeSize))
                                    {
                                        int anglePos = (rangeLeft + offset) % 60;
                                        foreach (int depthPos in Enumerable.Range(0, 4))
                                        {
                                            if (prevTouchState.IsPressed(anglePos, depthPos))
                                            {
                                                if (prevMin is null || depthPos < prevMin)
                                                {
                                                    prevMin = depthPos;
                                                }
                                                if (prevMax is null || depthPos > prevMax)
                                                {
                                                    prevMax = depthPos;
                                                }
                                            }

                                            if (touchState.IsPressed(anglePos, depthPos))
                                            {
                                                if (curMin is null || depthPos < curMin)
                                                {
                                                    curMin = depthPos;
                                                }
                                                if (curMax is null || depthPos > curMax)
                                                {
                                                    curMax = depthPos;
                                                }
                                            }
                                            if (prevMax != curMax)
                                            {
                                                ShowDebugText($"prev {prevMax}\ncur {curMax}");
                                            }
                                        }
                                    }

                                    switch (snapNote.Direction)
                                    {
                                        case SnapNote.SnapDirection.Forward:
                                            if (curMin is not null && prevMin is not null && curMin > prevMin)
                                            {
                                                return true;
                                            }
                                            if (curMax is not null && prevMax is not null && curMax > prevMax)
                                            {
                                                return true;
                                            }
                                            return false;
                                        case SnapNote.SnapDirection.Backward:
                                            if (curMin is not null && prevMin is not null && curMin < prevMin)
                                            {
                                                return true;
                                            }
                                            if (curMax is not null && prevMax is not null && curMax < prevMax)
                                            {
                                                return true;
                                            }
                                            return false;
                                        default:
                                            throw new Exception($"Unknown enum value {snapNote.Direction}");
                                    }
                                }

                                bool hit = false;
                                // Check if we have moved up/down on any specific anglePos.
                                foreach (int offset in Enumerable.Range(0, snapNote.Size))
                                {
                                    int anglePos = (snapNote.Left + offset) % 60;
                                    if (CheckDepthChangeInRange(anglePos, 1)) {
                                        hit = true;
                                        break;
                                    }
                                }

                                if (!hit)
                                {
                                    // Check if we have moved up/down on any range of two adjacent anglePos
                                    foreach (int offset in Enumerable.Range(0, snapNote.Size - 1))
                                    {
                                        int rangeLeft = (snapNote.Left + offset) % 60;
                                        if (CheckDepthChangeInRange(rangeLeft, 2))
                                        {
                                            hit = true;
                                            break;
                                        }
                                    }
                                }

                                if (hit)
                                {
                                    // TODO: this is copy-pasted like 15 different places, fix it
                                    float errorMs = hitTimeMs - snapNote.TimeMs;
                                    ShowDebugText($"{noteScanIndex} (snap): {errorMs}");
                                    foreach (HitWindow hitWindow in snapNote.HitWindows)
                                    {
                                        if (errorMs >= hitWindow.LeftMs && errorMs < hitWindow.RightMs)
                                        {
                                            snapNote.Judgement = hitWindow.Judgement;
                                            snapNote.HitTimeMs = hitTimeMs;
                                            LastJudgement = hitWindow.Judgement;
                                            LastJudgementTimeMs = hitTimeMs;
                                            NeedTouchHitsound = true;
                                            NeedSwipeSnapHitsound = true;
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
                        //Debug.Log($"Note {noteScanIndex}: Miss after {note.LatestHitTimeMs}");

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
                        if (holdNote.CurrentlyHeld)
                        {
                            NeedTouchHitsound = true;
                        }
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
            if (PlayingFromReplay)
            {
                return;
            }
            Replay.Add(new ReplayFrame { TimeMs = timeManager.VisualTimeMs, TouchState = touchState });
            HandleInput(timeManager.VisualTimeMs, touchState);
        }

        void Update()
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
                foreach (var note in notes)
                {
                    if (note.Judgement is not null)
                    {
                        Debug.Log($"{note.Judgement} by {note.TimeErrorMs ?? 999}ms, note at {note.TimeMs}");
                    }
                }
            }

            // Warning: will not work if end of chart is after the end of the audio clip, OR if it is within one frame
            // of the end of the audio clip.
            if (Chart?.endOfChart is not null && Chart.endOfChart.TimeMs < timeManager.VisualTimeMs && !WritingReplayAndExiting)
            {
                async Awaitable EndSong()
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
                EndSong();
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

            if (Chart?.endOfChart is not null && Chart.endOfChart.TimeMs < timeManager.VisualTimeMs)
            {
                // chart is done
                ChartManager.Instance.LastScoreData = CurrentScoreData();
                SceneSwitcher.Instance.LoadScene("_SongResults");
            }
        }

        private static void jsonError(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs errorArgs)
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
            await using GZipStream compressedStream = new(replayFileStream, System.IO.Compression.CompressionLevel.Fastest);
            await using StreamWriter writer = new(compressedStream);
            var serializer = new JsonSerializer();
            serializer.Error += jsonError;
            // "threadsafe" here is not entirely accurate, but this should hopefully allow us to take a reference
            // to the Replay that will _not_ be modified as more frames are added, so the JsonSerializer can safely
            // iterate over it.
            var threadsafeReplay = Replay.Take(Replay.Count);
            await Awaitable.BackgroundThreadAsync();
            serializer.Serialize(writer, threadsafeReplay);
            await Awaitable.MainThreadAsync();
            Debug.Log($"Replay {replayFileName} successfully written!");
        }

        private async Awaitable ReadReplayFile(string replayPath)
        {
            Debug.Log($"Reading replay from {replayPath}");
            PlayingFromReplay = true;
            await using FileStream replayFileStream = File.OpenRead(replayPath);
            await using GZipStream compressedStream = new(replayFileStream, CompressionMode.Decompress);
            using StreamReader streamReader = new(compressedStream);
            var serializer = new JsonSerializer();
            serializer.Error += jsonError;
            await Awaitable.BackgroundThreadAsync();
            var readReplay = (List<ReplayFrame>) serializer.Deserialize(streamReader, typeof(List<ReplayFrame>));
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
