using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using SaturnGame.Rendering;
using SaturnGame.Settings;
using UnityEngine;
using UnityEngine.Serialization;

namespace SaturnGame.RhythmGame
{
/// <summary>
/// NoteManager reads chart info from a ChartManager and manages the creation of the visual Notes representing the chart.
/// </summary>
public class NoteManager : MonoBehaviour
{
    [SerializeField] private ChartManager chartManager;
    private Chart Chart => chartManager.Chart;

    [Header("MANAGERS")]
    [SerializeField] private TimeManager timeManager;

    [Header("POOLS")]
    [SerializeField] private Transform activeObjectsContainer;
    [SerializeField] private BarLineObjectPool barLinePool;
    [SerializeField] private HoldEndObjectPool holdEndPool;
    [SerializeField] private HoldSurfaceObjectPool holdSurfacePool;
    [SerializeField] private NoteObjectPool notePool;

    [FormerlySerializedAs("r_EffectPool")] [SerializeField]
    private REffectObjectPool rEffectPool;

    [SerializeField] private SnapObjectPool snapPool;
    [SerializeField] private SwipeObjectPool swipePool;
    [SerializeField] private SyncObjectPool syncPool;

    [Header("RENDERERS")]
    [SerializeField] private GuideLaneRenderer guideLaneRenderer;

    private readonly List<NoteContainer> noteGarbage = new();
    private readonly List<SnapContainer> snapGarbage = new();
    private readonly List<SwipeContainer> swipeGarbage = new();
    private readonly List<GenericContainer> rEffectGarbage = new();
    private readonly List<BarLineContainer> barLineGarbage = new();
    private readonly List<GenericContainer> syncGarbage = new();
    private readonly List<HoldEndContainer> holdEndGarbage = new();
    private readonly List<HoldSurfaceRenderer> holdSurfaceGarbage = new();

    private int maskIndex;

    private void ProcessMasks()
    {
        if (maskIndex > Chart.Masks.Count - 1) return;

        while (maskIndex < Chart.Masks.Count && timeManager.VisualTimeMs >= Chart.Masks[maskIndex].TimeMs)
        {
            guideLaneRenderer.SetMask(Chart.Masks[maskIndex], timeManager.PlaybackSpeed);
            maskIndex++;
        }
    }

    // Warning: Process* methods are stateful - `*Index` variables persist through the lifetime of the NoteManager
    // to avoid creating duplicate note objects.
    // Currently, there isn't a good way to "restart" the chart within the same NoteManager.

    private int noteIndex;

    private void ProcessNotes()
    {
        if (noteIndex > Chart.Notes.Count - 1) return;

        // Scans through the chart note by note.
        while (noteIndex < Chart.Notes.Count &&
               ScaledVisualTime() + ScrollDuration() >= Chart.Notes[noteIndex].ScaledVisualTime)
        {
            Note currentNote = Chart.Notes[noteIndex];

            GetNote(currentNote);

            switch (currentNote)
            {
                case SnapNote snapNote:
                {
                    GetSnap(snapNote);
                    break;
                }
                case SwipeNote swipeNote:
                {
                    GetSwipe(swipeNote);
                    break;
                }
            }

            if (currentNote.BonusType is Note.NoteBonusType.RNote)
                GetR_Effect(currentNote);

            noteIndex++;
        }
    }

    private int holdIndex;

    private void ProcessHolds()
    {
        if (holdIndex > Chart.HoldNotes.Count - 1) return;

        while (holdIndex < Chart.HoldNotes.Count && ScaledVisualTime() + ScrollDuration() >=
               Chart.HoldNotes[holdIndex].Start.ScaledVisualTime)
        {
            HoldNote currentHold = Chart.HoldNotes[holdIndex];

            GetNote(currentHold);
            GetHoldEnd(currentHold.End);
            GetHoldSurface(currentHold);

            if (currentHold.BonusType is Note.NoteBonusType.RNote)
                GetR_Effect(currentHold);

            holdIndex++;
        }
    }

    private int barLineIndex;

    private void ProcessBarLines()
    {
        if (barLineIndex > Chart.BarLines.Count - 1) return;

        while (barLineIndex < Chart.BarLines.Count && ScaledVisualTime() + ScrollDuration() >=
               Chart.BarLines[barLineIndex].ScaledVisualTime)
        {
            GetBarLine(Chart.BarLines[barLineIndex].ScaledVisualTime);
            barLineIndex++;
        }
    }

    private int syncIndex;

    private void ProcessSync()
    {
        if (syncIndex > Chart.Syncs.Count - 1) return;

        while (syncIndex < Chart.Syncs.Count &&
               ScaledVisualTime() + ScrollDuration() >= Chart.Syncs[syncIndex].ScaledVisualTime)
        {
            GetSync(Chart.Syncs[syncIndex]);
            syncIndex++;
        }
    }

    private Gimmick lastHiSpeedChange = new(0, 0, Gimmick.GimmickType.HiSpeed, 1);
    private int hiSpeedIndex;

    private void ProcessHiSpeed()
    {
        if (hiSpeedIndex > Chart.Notes.Count - 1) return;

        while (hiSpeedIndex < Chart.HiSpeedGimmicks.Count &&
               Chart.HiSpeedGimmicks[hiSpeedIndex].TimeMs <= timeManager.VisualTimeMs)
        {
            lastHiSpeedChange = Chart.HiSpeedGimmicks[hiSpeedIndex];
            hiSpeedIndex++;
        }
    }

    private int reverseNoteIndex;
    private int reverseHoldNoteIndex;
    private int reverseGimmickIndex;

    private bool reverseActive;
    private float reverseStartTime;
    private float reverseMidTime;
    private float reverseEndTime;
    private float reverseMirrorTime;

    private void ProcessReverseGimmicks()
    {
        if (reverseNoteIndex > Chart.ReverseNotes.Count) return;

        if (reverseGimmickIndex < Chart.ReverseGimmicks.Count - 1 &&
            Chart.ReverseGimmicks[reverseGimmickIndex].TimeMs <= timeManager.VisualTimeMs)
        {
            switch (Chart.ReverseGimmicks[reverseGimmickIndex].Type)
            {
                case Gimmick.GimmickType.ReverseEffectStart:
                {
                    reverseStartTime = Chart.ReverseGimmicks[reverseGimmickIndex].ScaledVisualTime;
                    reverseMidTime = Chart.ReverseGimmicks[reverseGimmickIndex + 1].ScaledVisualTime;
                    reverseEndTime = Chart.ReverseGimmicks[reverseGimmickIndex + 2].ScaledVisualTime;
                    reverseMirrorTime = reverseStartTime + (reverseEndTime - reverseMidTime);
                    reverseActive = true;
                    break;
                }

                case Gimmick.GimmickType.ReverseEffectEnd:
                {
                    reverseStartTime = 0;
                    reverseMidTime = 0;
                    reverseEndTime = 0;
                    reverseActive = false;
                    break;
                }
                case Gimmick.GimmickType.None:
                case Gimmick.GimmickType.BeatsPerMinute:
                case Gimmick.GimmickType.TimeSignature:
                case Gimmick.GimmickType.HiSpeed:
                case Gimmick.GimmickType.ReverseNoteEnd:
                case Gimmick.GimmickType.StopStart:
                case Gimmick.GimmickType.StopEnd:
                default:
                {
                    throw new ArgumentOutOfRangeException(
                        $"Expected a reverse gimmick, but got a ${Chart.ReverseGimmicks[reverseGimmickIndex].Type}");
                }
            }

            reverseGimmickIndex++;
        }

        while (reverseActive && reverseNoteIndex < Chart.ReverseNotes.Count &&
               ScaledVisualTime() + 0.25f * ScrollDuration() >=
               Chart.ReverseNotes[reverseNoteIndex].ScaledVisualTime)
        {
            Note currentNote = Chart.ReverseNotes[reverseNoteIndex];

            GetNote(currentNote, true);

            switch (currentNote)
            {
                case SnapNote snapNote:
                {
                    GetSnap(snapNote, true);
                    break;
                }
                case SwipeNote swipeNote:
                {
                    GetSwipe(swipeNote, true);
                    break;
                }
            }

            if (currentNote.BonusType is Note.NoteBonusType.RNote)
                GetR_Effect(currentNote, true);

            reverseNoteIndex++;
        }

        if (reverseHoldNoteIndex != 0 && reverseHoldNoteIndex > Chart.ReverseHoldNotes.Count - 1) return;

        while (reverseHoldNoteIndex < Chart.ReverseHoldNotes.Count &&
               ScaledVisualTime() + 0.25f * ScrollDuration() >=
               Chart.ReverseHoldNotes[reverseHoldNoteIndex].Start.ScaledVisualTime)
        {
            HoldNote currentHold = Chart.ReverseHoldNotes[reverseHoldNoteIndex];

            GetNote(currentHold, true);
            GetHoldEnd(currentHold.End, true);
            GetHoldSurface(currentHold, true);

            if (currentHold.BonusType is Note.NoteBonusType.RNote)
                GetR_Effect(currentHold, true);

            reverseHoldNoteIndex++;
        }
    }

    // note: maybe possible to avoid having 3 type parameters by using an interface with covariance and just using
    // PositionedChartElement or AbstractPositionedChartElementRenderer<PositionedChartElement> for TContained and
    // TRenderer, since we don't actually care about those types.
    private void UpdateContainer<TContainer, TContained, TRenderer>([NotNull] TContainer container,
        ICollection<TContainer> garbage)
        where TContainer : AbstractPositionedChartElementContainer<TContained, TRenderer>
        where TContained : PositionedChartElement
        where TRenderer : AbstractPositionedChartElementRenderer<TContained>
    {
        // Set only reverse containers active during a reverse.
        if (reverseActive)
            container.gameObject.SetActive(container.Reverse);
        else
            container.gameObject.SetActive(!container.Reverse);

        if (!container.Reverse)
        {
            AnimateObject(container, garbage, container.Note.TimeMs, container.Note.ScaledVisualTime,
                container.transform, true);
        }
        else
        {
            ReverseAnimateObject(container, garbage, container.Note.ScaledVisualTime,
                container.Note.ScaledVisualTime, 1.0f, container.transform, true);
        }
    }

    private void UpdateObjects()
    {
        foreach (NoteContainer container in notePool.ActiveObjects)
            UpdateContainer<NoteContainer, Note, NoteRenderer>(container, noteGarbage);

        foreach (SnapContainer container in snapPool.ActiveObjects)
            UpdateContainer<SnapContainer, SnapNote, SnapRenderer>(container, snapGarbage);

        foreach (SwipeContainer container in swipePool.ActiveObjects)
            UpdateContainer<SwipeContainer, SwipeNote, SwipeRenderer>(container, swipeGarbage);

        foreach (GenericContainer container in rEffectPool.ActiveObjects)
            UpdateContainer<GenericContainer, PositionedChartElement, GenericRenderer>(container, rEffectGarbage);

        foreach (HoldEndContainer container in holdEndPool.ActiveObjects)
            UpdateContainer<HoldEndContainer, HoldSegment, HoldEndRenderer>(container, holdEndGarbage);

        foreach (HoldSurfaceRenderer holdSurfaceRenderer in holdSurfacePool.ActiveObjects)
        {
            // Don't use UpdateContainer as currently hold surface management doesn't use the standard Container /
            // Renderer abstract classes. It's a bit more complicated because the spawn and despawn times are not
            // the same time.

            // Set only reverse renderers active during a reverse.
            if (reverseActive)
                holdSurfaceRenderer.gameObject.SetActive(holdSurfaceRenderer.Reverse);
            else
                holdSurfaceRenderer.gameObject.SetActive(!holdSurfaceRenderer.Reverse);

            if (!holdSurfaceRenderer.Reverse)
            {
                AnimateObject(holdSurfaceRenderer, holdSurfaceGarbage, holdSurfaceRenderer.HoldNote.End.TimeMs,
                    holdSurfaceRenderer.HoldNote.Start.ScaledVisualTime, holdSurfaceRenderer.transform, false);
            }
            else
            {
                ReverseAnimateObject(holdSurfaceRenderer, holdSurfaceGarbage,
                    holdSurfaceRenderer.HoldNote.End.ScaledVisualTime,
                    holdSurfaceRenderer.HoldNote.Start.ScaledVisualTime, 1.0f, holdSurfaceRenderer.transform,
                    false);
            }
        }

        foreach (GenericContainer container in syncPool.ActiveObjects)
            // Note: syncs do not show during reverses, but we use the reverse-aware UpdateContainer anyway for
            // consistency in the non-reverse case.
            UpdateContainer<GenericContainer, PositionedChartElement, GenericRenderer>(container, syncGarbage);

        foreach (BarLineContainer container in barLinePool.ActiveObjects)
        {
            container.gameObject.SetActive(!reverseActive);
            AnimateObject(container, barLineGarbage, container.Time, container.Time, container.transform, true);
        }
    }

    private void ReleaseObjects()
    {
        // Get rid of objects by releasing them back into the pool.
        foreach (NoteContainer container in noteGarbage) notePool.ReleaseObject(container);

        foreach (SnapContainer snap in snapGarbage) snapPool.ReleaseObject(snap);

        foreach (SwipeContainer swipe in swipeGarbage) swipePool.ReleaseObject(swipe);

        foreach (GenericContainer rEffect in rEffectGarbage) rEffectPool.ReleaseObject(rEffect);

        foreach (BarLineContainer barLine in barLineGarbage) barLinePool.ReleaseObject(barLine);

        foreach (GenericContainer sync in syncGarbage) syncPool.ReleaseObject(sync);

        foreach (HoldEndContainer holdEnd in holdEndGarbage) holdEndPool.ReleaseObject(holdEnd);

        foreach (HoldSurfaceRenderer holdSurface in holdSurfaceGarbage) holdSurfacePool.ReleaseObject(holdSurface);

        // Clear the garbage lists
        noteGarbage.Clear();
        snapGarbage.Clear();
        swipeGarbage.Clear();
        rEffectGarbage.Clear();
        barLineGarbage.Clear();
        syncGarbage.Clear();
        holdEndGarbage.Clear();
        holdSurfaceGarbage.Clear();
    }


    private void AnimateObject<T>(T obj, ICollection<T> garbage, float unscaledTime, float scaledTime,
        [NotNull] Transform objectTransform, bool scale)
    {
        float distance = scaledTime - ScaledVisualTime();
        float scroll = SaturnMath.InverseLerp(ScrollDuration(), 0, distance);

        objectTransform.position = new Vector3(0, 0, Mathf.LerpUnclamped(-6, 0, scroll));

        if (scale)
        {
            float clampedScroll = Mathf.Max(0, scroll);
            objectTransform.localScale = new Vector3(clampedScroll, clampedScroll, clampedScroll);
        }

        // I love type systems.
        GameObject hitNoteGameObject = obj switch
        {
            NoteContainer { Note : { HasBeenHit: true } } container => container.gameObject,
            SnapContainer { Note : { HasBeenHit: true } } container => container.gameObject,
            SwipeContainer { Note : { HasBeenHit: true } } container => container.gameObject,
            // in GenericContainer, Note is only required to be a PositionedChartElement. The typecheck here
            // (Note: Note) will only match for GenericContainers containing Notes, that is, for R effects.
            // Syncs will not be matched as SyncIndicator is not a Note.
            GenericContainer { Note: Note { HasBeenHit: true } } container => container.gameObject,
            // This should not match:
            // - BarLineContainer
            // - HoldEndContainer
            // - GenericContainer for syncs (since SyncIndicator is not a Note)
            //   TODO: syncs should probably disappear if either note is hit.
            _ => null,
        };
        if (hitNoteGameObject is not null)
        {
            // Immediately hide a hit note and mark as garbage.
            hitNoteGameObject.SetActive(false);
            garbage.Add(obj);
            return;
        }

        // Collect all objects after passing the judgement line to return them to their pool.
        //bool pastDespawnTime = ScaledVisualTime() - ScrollDuration() * despawnTimeMultiplier >= despawnTime;
        bool pastTimestamp = unscaledTime + 500 < timeManager.VisualTimeMs;
        if (pastTimestamp) garbage.Add(obj);
    }

    private void ReverseAnimateObject<T>(T obj, ICollection<T> garbage, float time, float despawnTime,
        float despawnTimeMultiplier, [NotNull] Transform objectTransform, bool scale)
    {
        float distance = time - ScaledVisualTime();
        float scroll = SaturnMath.InverseLerp(0.25f * ScrollDuration(), 0, distance);

        objectTransform.position = new Vector3(0, 0, Mathf.LerpUnclamped(1.5f, 0, scroll));

        if (scale)
        {
            float scaledScroll = Mathf.LerpUnclamped(1.25f, 1, scroll);
            objectTransform.localScale = new Vector3(scaledScroll, scaledScroll, scaledScroll);
        }

        // Collect all objects after passing the judgement line to return them to their pool.
        if (ScaledVisualTime() - ScrollDuration() * despawnTimeMultiplier >= despawnTime) garbage.Add(obj);
    }

    [NotNull]
    private TContainer SetupContainer<TContainer, TContained, TRenderer>(
        [NotNull] MonoBehaviourPool<TContainer> pool, TContained input, bool reverse, bool setRenderer = true)
        where TContained : PositionedChartElement
        where TRenderer : AbstractPositionedChartElementRenderer<TContained>
        where TContainer : AbstractPositionedChartElementContainer<TContained, TRenderer>
    {
        TContainer container = pool.GetObject();

        container.Note = input;
        if (setRenderer)
            // This is optional as the caller may want to change some thing before calling SetRenderer.
            container.Renderer.SetRenderer(input);
        container.Reverse = reverse;

        container.transform.SetParent(activeObjectsContainer);
        container.gameObject.SetActive(true);

        return container;
    }

    private void GetNote([NotNull] Note input, bool reverse = false)
    {
        NoteContainer container =
            SetupContainer<NoteContainer, Note, NoteRenderer>(notePool, input, reverse, setRenderer: false);

        int noteWidth = SettingsManager.Instance.PlayerSettings.DesignSettings.NoteWidth;
        container.Renderer.Width = noteWidth;
        container.Renderer.SetRenderer(input);
    }

    private void GetSnap(SnapNote input, bool reverse = false)
    {
        SetupContainer<SnapContainer, SnapNote, SnapRenderer>(snapPool, input, reverse);
    }

    private void GetSwipe(SwipeNote input, bool reverse = false)
    {
        SetupContainer<SwipeContainer, SwipeNote, SwipeRenderer>(swipePool, input, reverse);
    }

    private void GetHoldEnd(HoldSegment input, bool reverse = false)
    {
        SetupContainer<HoldEndContainer, HoldSegment, HoldEndRenderer>(holdEndPool, input, reverse);
    }

    private void GetHoldSurface(HoldNote input, bool reverse = false)
    {
        HoldSurfaceRenderer holdSurfaceRenderer = holdSurfacePool.GetObject();

        holdSurfaceRenderer.TimeManager = timeManager;

        holdSurfaceRenderer.SetRenderer(input);
        holdSurfaceRenderer.GenerateMesh(ScrollDuration());
        holdSurfaceRenderer.Reverse = reverse;

        Transform rendererTransform = holdSurfaceRenderer.transform;
        rendererTransform.SetParent(activeObjectsContainer);
        rendererTransform.localScale = Vector3.one;
        holdSurfaceRenderer.gameObject.SetActive(true);
    }

    private void GetR_Effect(Note input, bool reverse = false)
    {
        SetupContainer<GenericContainer, PositionedChartElement, GenericRenderer>(rEffectPool, input, reverse);
    }

    private void GetBarLine(float timestamp)
    {
        BarLineContainer container = barLinePool.GetObject();

        container.Time = timestamp;
        container.transform.SetParent(activeObjectsContainer);
        container.gameObject.SetActive(true);
    }

    private void GetSync(SyncIndicator input)
    {
        SetupContainer<GenericContainer, PositionedChartElement, GenericRenderer>(syncPool, input, reverse: false);
    }


    private float GetScaledTime(float input)
    {
        float hiSpeed = lastHiSpeedChange.HiSpeed;
        float hiSpeedTime = lastHiSpeedChange.TimeMs;
        float hiSpeedScaledTime = lastHiSpeedChange.ScaledVisualTime;

        return hiSpeedScaledTime + (input - hiSpeedTime) * hiSpeed;
    }

    private static float ScrollDuration()
    {
        // A Note scrolling from it's spawn point to the judgement line at NoteSpeed 1.0 takes
        // approximately 3266.667 milliseconds. This is 10x that, because
        // NoteSpeed is stored as an integer that's 10x the actual value.

        return 32660.667f / SettingsManager.Instance.PlayerSettings.GameSettings.NoteSpeed;
    }

    private float ScaledVisualTime()
    {
        if (!reverseActive) return GetScaledTime(timeManager.VisualTimeMs);

        float progress = SaturnMath.InverseLerp(reverseStartTime, reverseMidTime,
            GetScaledTime(timeManager.VisualTimeMs));
        float ease = SaturnMath.Ease.Reverse(progress);
        return Mathf.Lerp(reverseStartTime, reverseMirrorTime, ease);
    }

    private void Update()
    {
        if (timeManager.State != TimeManager.SongState.Playing) return;

        ProcessHiSpeed();
        ProcessReverseGimmicks();

        ProcessMasks();

        if (!reverseActive)
        {
            ProcessNotes();
            ProcessHolds();
        }

        ProcessSync();
        ProcessBarLines();

        UpdateObjects();
        ReleaseObjects();
    }
}
}
