using System.Collections.Generic;
using SaturnGame.Rendering;
using SaturnGame.Settings;
using UnityEngine;

namespace SaturnGame.RhythmGame
{
    /// <summary>
    /// NoteManager reads chart info from a ChartManager and manages the creation of the visual Notes representing the chart.
    /// </summary>
    public class NoteManager : MonoBehaviour
    {
        private Chart Chart => ChartManager.Instance.chart;

        [Header("MANAGERS")]
        [SerializeField] private TimeManager timeManager;
        [SerializeField] private BgmManager bgmManager;

        [Header("POOLS")]
        [SerializeField] private Transform activeObjectsContainer;
        [SerializeField] private BarLineObjectPool barLinePool;
        [SerializeField] private HoldEndObjectPool holdEndPool;
        [SerializeField] private HoldSurfaceObjectPool holdSurfacePool;
        [SerializeField] private NoteObjectPool notePool;
        [SerializeField] private R_EffectObjectPool r_EffectPool;
        [SerializeField] private SnapObjectPool snapPool;
        [SerializeField] private SwipeObjectPool swipePool;
        [SerializeField] private SyncObjectPool syncPool;

        [Header("RENDERERS")]
        [SerializeField] private GuideLaneRenderer guideLaneRenderer;

        private readonly List<NoteContainer> noteGarbage = new();
        private readonly List<SnapContainer> snapGarbage = new();
        private readonly List<SwipeContainer> swipeGarbage = new();
        private readonly List<GenericContainer> r_EffectGarbage = new();
        private readonly List<BarLineContainer> barLineGarbage = new();
        private readonly List<GenericContainer> syncGarbage = new();
        private readonly List<HoldEndContainer> holdEndGarbage = new();
        private readonly List<HoldSurfaceRenderer> holdSurfaceGarbage = new();

        private int maskIndex = 0;
        private void ProcessMasks()
        {
            if (maskIndex > Chart.masks.Count - 1) return;

            while (maskIndex < Chart.masks.Count && timeManager.VisualTimeMs >= Chart.masks[maskIndex].TimeMs)
            {
                guideLaneRenderer.SetMask(Chart.masks[maskIndex], timeManager.PlaybackSpeed);
                maskIndex++;
            }
        }

        // Warning: Process* methods are stateful - `*Index` variables persist through the lifetime of the NoteManager
        // to avoid creating duplicate note objects.
        // Currently, there isn't a good way to "restart" the chart within the same NoteManager.

        private int noteIndex = 0;
        private void ProcessNotes()
        {
            if (noteIndex > Chart.notes.Count - 1) return;

            // Scans through the chart note by note.
            while (noteIndex < Chart.notes.Count && ScaledVisualTime() + ScrollDuration() >= Chart.notes[noteIndex].ScaledVisualTime)
            {
                Note currentNote = Chart.notes[noteIndex];

                GetNote(currentNote);

                if (currentNote is SnapNote snapNote)
                    GetSnap(snapNote);

                if (currentNote is SwipeNote swipeNote)
                    GetSwipe(swipeNote);

                if (currentNote.BonusType is Note.NoteBonusType.R_Note)
                    GetR_Effect(currentNote);

                noteIndex++;
            }
        }

        private int holdIndex = 0;
        private void ProcessHolds()
        {
            if (holdIndex > Chart.holdNotes.Count - 1) return;

            while (holdIndex < Chart.holdNotes.Count && ScaledVisualTime() + ScrollDuration() >= Chart.holdNotes[holdIndex].Start.ScaledVisualTime)
            {
                HoldNote currentHold = Chart.holdNotes[holdIndex];

                GetNote(currentHold);
                GetHoldEnd(currentHold.End);
                GetHoldSurface(currentHold);

                if (currentHold.BonusType is Note.NoteBonusType.R_Note)
                    GetR_Effect(currentHold);

                holdIndex++;
            }
        }

        private int barLineIndex = 0;
        private void ProcessBarLines()
        {
            if (barLineIndex > Chart.barLines.Count - 1) return;

            while (barLineIndex < Chart.barLines.Count && ScaledVisualTime() + ScrollDuration() >= Chart.barLines[barLineIndex].ScaledVisualTime)
            {
                GetBarLine(Chart.barLines[barLineIndex].ScaledVisualTime);
                barLineIndex++;
            }
        }

        private int syncIndex = 0;
        private void ProcessSync()
        {
            if (syncIndex > Chart.syncs.Count - 1) return;

            while (syncIndex < Chart.syncs.Count && ScaledVisualTime() + ScrollDuration() >= Chart.syncs[syncIndex].ScaledVisualTime)
            {
                GetSync(Chart.syncs[syncIndex]);
                syncIndex++;
            }
        }

        private Gimmick bgmData;
        private int bgmDataIndex = 0;
        private void ProcessBgmData()
        {
            if (bgmDataIndex > Chart.notes.Count - 1) return;

            while (bgmDataIndex < Chart.bgmDataGimmicks.Count && Chart.bgmDataGimmicks[bgmDataIndex].TimeMs <= timeManager.VisualTimeMs)
            {
                bgmData = Chart.bgmDataGimmicks[bgmDataIndex];

                if (bgmData != null) bgmManager.UpdateBgmData(bgmData.BeatsPerMinute, bgmData.TimeSig);
                bgmDataIndex++;
            }
        }

        private Gimmick lastHiSpeedChange = new(0, 0, Gimmick.GimmickType.HiSpeed, 1, null);
        private int hiSpeedIndex = 0;
        private void ProcessHiSpeed()
        {
            if (hiSpeedIndex > Chart.notes.Count - 1) return;

            while (hiSpeedIndex < Chart.hiSpeedGimmicks.Count && Chart.hiSpeedGimmicks[hiSpeedIndex].TimeMs <= timeManager.VisualTimeMs)
            {
                lastHiSpeedChange = Chart.hiSpeedGimmicks[hiSpeedIndex];
                hiSpeedIndex++;
            }
        }

        private int reverseNoteIndex = 0;
        private int reverseHoldNoteIndex = 0;
        private int reverseGimmickIndex = 0;

        private bool reverseActive = false;
        private float reverseStartTime = 0;
        private float reverseMidTime = 0;
        private float reverseEndTime = 0;
        private float reverseMirrorTime = 0;
        private void ProcessReverseGimmicks()
        {
            // TODO!!!!! Fix reverses AGAIN omegalul

            if (reverseNoteIndex > Chart.reverseNotes.Count) return;

            if (reverseGimmickIndex < Chart.reverseGimmicks.Count - 1 && Chart.reverseGimmicks[reverseGimmickIndex].TimeMs <= timeManager.VisualTimeMs)
            {
                switch (Chart.reverseGimmicks[reverseGimmickIndex].Type)
                {
                    case Gimmick.GimmickType.ReverseEffectStart:
                        reverseStartTime = Chart.reverseGimmicks[reverseGimmickIndex].ScaledVisualTime;
                        reverseMidTime = Chart.reverseGimmicks[reverseGimmickIndex + 1].ScaledVisualTime;
                        reverseEndTime = Chart.reverseGimmicks[reverseGimmickIndex + 2].ScaledVisualTime;
                        reverseMirrorTime = reverseStartTime + (reverseEndTime - reverseMidTime);
                        reverseActive = true;
                        break;

                    case Gimmick.GimmickType.ReverseEffectEnd:
                        reverseStartTime = 0;
                        reverseMidTime = 0;
                        reverseEndTime = 0;
                        reverseActive = false;
                        break;
                }

                reverseGimmickIndex++;
            }

            while (reverseActive && reverseNoteIndex < Chart.reverseNotes.Count && ScaledVisualTime() + (0.25f * ScrollDuration()) >= Chart.reverseNotes[reverseNoteIndex].ScaledVisualTime)
            {
                Note currentNote = Chart.reverseNotes[reverseNoteIndex];

                GetNote(currentNote, true);

                if (currentNote is SnapNote snapNote)
                    GetSnap(snapNote, true);

                if (currentNote is SwipeNote swipeNote)
                    GetSwipe(swipeNote, true);

                if (currentNote.BonusType is Note.NoteBonusType.R_Note)
                    GetR_Effect(currentNote, true);

                reverseNoteIndex++;
            }

            if (reverseHoldNoteIndex != 0 && reverseHoldNoteIndex > Chart.reverseHoldNotes.Count - 1) return;

            while (reverseHoldNoteIndex < Chart.reverseHoldNotes.Count && ScaledVisualTime() + (0.25f * ScrollDuration()) >= Chart.reverseHoldNotes[reverseHoldNoteIndex].Start.ScaledVisualTime)
            {
                HoldNote currentHold = Chart.reverseHoldNotes[reverseHoldNoteIndex];

                GetNote(currentHold, true);
                GetHoldEnd(currentHold.End, true);
                GetHoldSurface(currentHold, true);

                if (currentHold.BonusType is Note.NoteBonusType.R_Note)
                    GetR_Effect(currentHold, true);

                reverseHoldNoteIndex++;
            }
        }

        private void UpdateObjects()
        {
            foreach (NoteContainer container in notePool.ActiveObjects)
            {
                // Set only reverse containers active during a reverse.
                if (reverseActive)
                    container.gameObject.SetActive(container.reverse);
                else
                    container.gameObject.SetActive(!container.reverse);

                if (!container.reverse)
                    AnimateObject(container, noteGarbage, container.note.ScaledVisualTime, container.note.ScaledVisualTime, 0.25f, container.transform, true);
                else ReverseAnimateObject(container, noteGarbage, container.note.ScaledVisualTime, container.note.ScaledVisualTime, 1.0f, container.transform, true);
            }

            foreach (SnapContainer container in snapPool.ActiveObjects)
            {
                // Set only reverse containers active during a reverse.
                if (reverseActive)
                    container.gameObject.SetActive(container.reverse);
                else
                    container.gameObject.SetActive(!container.reverse);

                if (!container.reverse)
                    AnimateObject(container, snapGarbage, container.note.ScaledVisualTime, container.note.ScaledVisualTime, 0.25f, container.transform, true);
                else ReverseAnimateObject(container, snapGarbage, container.note.ScaledVisualTime, container.note.ScaledVisualTime, 1.0f, container.transform, true);
            }

            foreach (SwipeContainer container in swipePool.ActiveObjects)
            {
                // Set only reverse containers active during a reverse.
                if (reverseActive)
                    container.gameObject.SetActive(container.reverse);
                else
                    container.gameObject.SetActive(!container.reverse);

                if (!container.reverse)
                    AnimateObject(container, swipeGarbage, container.note.ScaledVisualTime, container.note.ScaledVisualTime, 0.25f, container.transform, true);
                else ReverseAnimateObject(container, swipeGarbage, container.note.ScaledVisualTime, container.note.ScaledVisualTime, 1.0f, container.transform, true);
            }

            foreach (GenericContainer container in r_EffectPool.ActiveObjects)
            {
                // Set only reverse containers active during a reverse.
                if (reverseActive)
                    container.gameObject.SetActive(container.reverse);
                else
                    container.gameObject.SetActive(!container.reverse);

                if (!container.reverse)
                    AnimateObject(container, r_EffectGarbage, container.note.ScaledVisualTime, container.note.ScaledVisualTime, 0.25f, container.transform, true);
                else ReverseAnimateObject(container, r_EffectGarbage, container.note.ScaledVisualTime, container.note.ScaledVisualTime, 1.0f, container.transform, true);
            }

            foreach (HoldEndContainer container in holdEndPool.ActiveObjects)
            {
                // Set only reverse containers active during a reverse.
                if (reverseActive)
                    container.gameObject.SetActive(container.reverse);
                else
                    container.gameObject.SetActive(!container.reverse);

                if (!container.reverse)
                    AnimateObject(container, holdEndGarbage, container.holdEnd.ScaledVisualTime, container.holdEnd.ScaledVisualTime, 0.25f, container.transform, true);
                else ReverseAnimateObject(container, holdEndGarbage, container.holdEnd.ScaledVisualTime, container.holdEnd.ScaledVisualTime, 1.0f, container.transform, true);
            }

            foreach (HoldSurfaceRenderer renderer in holdSurfacePool.ActiveObjects)
            {
                // Set only reverse renderers active during a reverse.
                if (reverseActive)
                    renderer.gameObject.SetActive(renderer.reverse);
                else
                    renderer.gameObject.SetActive(!renderer.reverse);

                if (!renderer.reverse)
                    AnimateObject(renderer, holdSurfaceGarbage, renderer.holdNote.Start.ScaledVisualTime, renderer.holdNote.End.ScaledVisualTime, 0.25f, renderer.transform, false);
                else ReverseAnimateObject(renderer, holdSurfaceGarbage, renderer.holdNote.End.ScaledVisualTime, renderer.holdNote.Start.ScaledVisualTime, 1.0f, renderer.transform, false);
            }

            foreach (GenericContainer container in syncPool.ActiveObjects)
                AnimateObject(container, syncGarbage, container.note.ScaledVisualTime, container.note.ScaledVisualTime, 0.25f, container.renderer.transform, true);

            foreach (BarLineContainer container in barLinePool.ActiveObjects)
            {
                container.gameObject.SetActive(!reverseActive);
                AnimateObject(container, barLineGarbage, container.time, container.time, 0, container.transform, true);
            }
        }

        private void ReleaseObjects()
        {
            // Get rid of objects by releasing them back into the pool.
            foreach (NoteContainer container in noteGarbage)
            {
                notePool.ReleaseObject(container);
            }

            foreach (SnapContainer snap in snapGarbage)
            {
                snapPool.ReleaseObject(snap);
            }

            foreach (SwipeContainer swipe in swipeGarbage)
            {
                swipePool.ReleaseObject(swipe);
            }

            foreach (GenericContainer r_Effect in r_EffectGarbage)
            {
                r_EffectPool.ReleaseObject(r_Effect);
            }

            foreach (BarLineContainer barLine in barLineGarbage)
            {
                barLinePool.ReleaseObject(barLine);
            }

            foreach (GenericContainer sync in syncGarbage)
            {
                syncPool.ReleaseObject(sync);
            }

            foreach (HoldEndContainer holdEnd in holdEndGarbage)
            {
                holdEndPool.ReleaseObject(holdEnd);
            }

            foreach (HoldSurfaceRenderer holdSurface in holdSurfaceGarbage)
            {
                holdSurfacePool.ReleaseObject(holdSurface);
            }

            // Clear the garbage lists
            noteGarbage.Clear();
            snapGarbage.Clear();
            swipeGarbage.Clear();
            r_EffectGarbage.Clear();
            barLineGarbage.Clear();
            syncGarbage.Clear();
            holdEndGarbage.Clear();
            holdSurfaceGarbage.Clear();
        }


        private void AnimateObject<T> (T obj, List<T> garbage, float time, float despawnTime, float despawnTimeMultiplier, Transform transform, bool scale)
        {
            float distance = time - ScaledVisualTime();
            float scroll = SaturnMath.InverseLerp(ScrollDuration(), 0, distance);

            transform.position = new Vector3(0, 0, Mathf.LerpUnclamped(-6, 0, scroll));

            if (scale)
            {
                float clampedScroll = Mathf.Max(0, scroll);
                transform.localScale = new Vector3(clampedScroll, clampedScroll, clampedScroll);
            }

            // Collect all objects after passing the judgement line to return them to their pool.
            if (ScaledVisualTime() - ScrollDuration() * despawnTimeMultiplier >= despawnTime)
            {
                garbage.Add(obj);
            }
        }

        private void ReverseAnimateObject<T> (T obj, List<T> garbage, float time, float despawnTime, float despawnTimeMultiplier, Transform transform, bool scale)
        {
            float distance = time - ScaledVisualTime();
            float scroll = SaturnMath.InverseLerp(0.25f * ScrollDuration(), 0, distance);

            transform.position = new Vector3(0, 0, Mathf.LerpUnclamped(1.5f, 0, scroll));

            if (scale)
            {
                float scaledScroll = Mathf.LerpUnclamped(1.25f, 1, scroll);
                transform.localScale = new Vector3(scaledScroll, scaledScroll, scaledScroll);
            }

            // Collect all objects after passing the judgement line to return them to their pool.
            if (ScaledVisualTime() - ScrollDuration() * despawnTimeMultiplier >= despawnTime)
            {
                garbage.Add(obj);
            }
        }


        private NoteContainer GetNote(Note input, bool reverse = false)
        {
            NoteContainer container = notePool.GetObject();

            container.note = input;
            int noteWidth = SettingsManager.Instance.PlayerSettings.DesignSettings.NoteWidth;
            container.renderer.SetRenderer(input, noteWidth);
            container.reverse = reverse;

            container.transform.SetParent(activeObjectsContainer);
            container.gameObject.SetActive(true);

            return container;
        }

        private SnapContainer GetSnap(SnapNote input, bool reverse = false)
        {
            SnapContainer container = snapPool.GetObject();

            container.note = input;
            container.renderer.SetRenderer(input);
            container.reverse = reverse;

            container.transform.SetParent(activeObjectsContainer);
            container.gameObject.SetActive(true);

            return container;
        }

        private SwipeContainer GetSwipe(SwipeNote input, bool reverse = false)
        {
            SwipeContainer container = swipePool.GetObject();

            container.note = input;
            container.renderer.SetRenderer(input);
            container.reverse = reverse;

            container.transform.SetParent(activeObjectsContainer);
            container.gameObject.SetActive(true);

            return container;
        }

        private HoldEndContainer GetHoldEnd(HoldSegment input, bool reverse = false)
        {
            HoldEndContainer container = holdEndPool.GetObject();

            container.holdEnd = input;
            container.renderer.SetRenderer(input);
            container.reverse = reverse;

            container.transform.SetParent(activeObjectsContainer);
            container.gameObject.SetActive(true);

            return container;
        }

        private HoldSurfaceRenderer GetHoldSurface(HoldNote input, bool reverse = false)
        {
            HoldSurfaceRenderer renderer = holdSurfacePool.GetObject();

            renderer.SetRenderer(input);
            renderer.GenerateMesh(ScrollDuration());
            renderer.reverse = reverse;

            renderer.transform.SetParent(activeObjectsContainer);
            renderer.transform.localScale = Vector3.one;
            renderer.gameObject.SetActive(true);

            return renderer;
        }

        private GenericContainer GetR_Effect(Note input, bool reverse = false)
        {
            GenericContainer container = r_EffectPool.GetObject();

            container.note = input;
            container.renderer.SetRenderer(input.Size, input.Position);
            container.reverse = reverse;

            container.transform.SetParent(activeObjectsContainer);
            container.gameObject.SetActive(true);

            return container;
        }

        private BarLineContainer GetBarLine(float timestamp)
        {
            BarLineContainer container = barLinePool.GetObject();

            container.time = timestamp;
            container.transform.SetParent(activeObjectsContainer);
            container.gameObject.SetActive(true);

            return container;
        }

        private GenericContainer GetSync(SyncIndicator input)
        {
            GenericContainer container = syncPool.GetObject();

            container.note = input;
            container.renderer.SetRenderer(input.Size, input.Position);

            container.transform.SetParent(activeObjectsContainer);
            container.gameObject.SetActive(true);

            return container;
        }


        private float GetScaledTime(float input)
        {
            float hiSpeed = lastHiSpeedChange.HiSpeed;
            float hiSpeedTime = lastHiSpeedChange.TimeMs;
            float hiSpeedScaledTime = lastHiSpeedChange.ScaledVisualTime;

            return hiSpeedScaledTime + ((input - hiSpeedTime) * hiSpeed);
        }

        private float ScrollDuration()
        {
            // A Note scrolling from it's spawn point to the judgement line at NoteSpeed 1.0 takes
            // approximately 3266.667 milliseconds. This is 10x that, because
            // NoteSpeed is stored as an integer that's 10x the actual value.

            return 32660.667f / SettingsManager.Instance.PlayerSettings.GameSettings.NoteSpeed;
        }

        private float ScaledVisualTime()
        {
            if (reverseActive)
            {
                float progress = SaturnMath.InverseLerp(reverseStartTime, reverseMidTime, GetScaledTime(timeManager.VisualTimeMs));
                float ease = SaturnMath.Ease.Reverse(progress);
                return Mathf.Lerp(reverseStartTime, reverseMirrorTime, ease);
            }

            return GetScaledTime(timeManager.VisualTimeMs);
        }

        void Update()
        {
            if (!bgmManager.bgmPlayer.isPlaying) return;

            ProcessBgmData();

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
