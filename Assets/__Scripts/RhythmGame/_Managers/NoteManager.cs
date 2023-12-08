using System;
using System.Collections;
using System.Collections.Generic;
using SaturnGame.Rendering;
using SaturnGame.Settings;
using UnityEngine;

namespace SaturnGame.RhythmGame
{
    public class NoteManager : MonoBehaviour
    {
        [Header("MANAGERS")]
        [SerializeField] private ChartManager chart;
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

        private List<NoteContainer> noteGarbage = new();
        private List<SnapContainer> snapGarbage = new();
        private List<SwipeContainer> swipeGarbage = new();
        private List<GenericContainer> r_EffectGarbage = new();
        private List<BarLineContainer> barLineGarbage = new();
        private List<GenericContainer> syncGarbage = new();

        private int maskIndex = 0;
        private void ProcessMasks()
        {
            if (maskIndex > chart.masks.Count - 1) return;

            while (maskIndex < chart.masks.Count && timeManager.VisualTime >= chart.masks[maskIndex].Time)
            {
                guideLaneRenderer.SetMask(chart.masks[maskIndex], timeManager.PlaybackSpeed);
                maskIndex++;
            }
        }

        private int noteIndex = 0;
        private void ProcessNotes()
        {
            if (noteIndex > chart.notes.Count - 1) return;

            // Scans through the chart note by note.
            while (noteIndex < chart.notes.Count && GetScaledTime(timeManager.VisualTime) + ScrollDuration() >= chart.notes[noteIndex].ScaledVisualTime)
            {
                Note currentNote = chart.notes[noteIndex];

                GetNote(currentNote);

                if (currentNote.NoteType is ObjectEnums.NoteType.SnapForward or ObjectEnums.NoteType.SnapBackward)
                    GetSnap(currentNote);
                
                if (currentNote.NoteType is ObjectEnums.NoteType.SwipeClockwise or ObjectEnums.NoteType.SwipeCounterclockwise)
                    GetSwipe(currentNote);

                if (currentNote.BonusType is ObjectEnums.BonusType.R_Note)
                    GetR_Effect(currentNote);

                noteIndex++;
            }
        }

        private int barLineIndex = 0;
        private void ProcessBarLines()
        {
            if (barLineIndex > chart.barLines.Count - 1) return;

            while (barLineIndex < chart.barLines.Count && GetScaledTime(timeManager.VisualTime) + ScrollDuration() >= chart.barLines[barLineIndex].ScaledVisualTime)
            {
                GetBarLine(chart.barLines[barLineIndex].ScaledVisualTime);
                barLineIndex++;
            }
        }

        private int syncIndex = 0;
        private void ProcessSync()
        {
            if (syncIndex > chart.syncs.Count - 1) return;

            while (syncIndex < chart.syncs.Count && GetScaledTime(timeManager.VisualTime) + ScrollDuration() >= chart.syncs[syncIndex].ScaledVisualTime)
            {
                GetSync(chart.syncs[syncIndex]);
                syncIndex++;
            }
        }

        private Gimmick bgmData;
        private int bgmDataIndex = 0;
        private void ProcessBgmData()
        {
            if (bgmDataIndex > chart.notes.Count - 1) return;

            while (bgmDataIndex < chart.bgmDataGimmicks.Count && chart.bgmDataGimmicks[bgmDataIndex].Time <= timeManager.VisualTime)
            {
                bgmData = chart.bgmDataGimmicks[bgmDataIndex];

                if (bgmData != null) bgmManager.UpdateBgmData(bgmData.BeatsPerMinute, bgmData.TimeSig);
                bgmDataIndex++;
            }
        }

        private Gimmick lastHiSpeedChange = new(0, 0, ObjectEnums.GimmickType.HiSpeed, 1, null);
        private int hiSpeedIndex = 0;
        private void ProcessHiSpeed()
        {
            if (hiSpeedIndex > chart.notes.Count - 1) return;

            while (hiSpeedIndex < chart.hiSpeedGimmicks.Count && chart.hiSpeedGimmicks[hiSpeedIndex].Time <= timeManager.VisualTime)
            {
                lastHiSpeedChange = chart.hiSpeedGimmicks[hiSpeedIndex];
                hiSpeedIndex++;
            }
        }

        private int reverseNoteIndex = 0;
        private int reverseHoldNoteIndex = 0;
        private int reverseGimmickIndex = 0;
        private bool reverseActive = false;
        private void ProcessReverseGimmicks()
        {
            if (reverseNoteIndex > chart.reverseNotes.Count) return;

            if (reverseGimmickIndex < chart.reverseGimmicks.Count && chart.reverseGimmicks[reverseGimmickIndex].Time <= timeManager.VisualTime)
            {
                switch (chart.reverseGimmicks[reverseGimmickIndex].GimmickType)
                {
                    case ObjectEnums.GimmickType.ReverseEffectStart:
                        reverseActive = true;
                        break;
                    
                    case ObjectEnums.GimmickType.ReverseEffectEnd:
                        reverseActive = false;
                        break;
                }

                reverseGimmickIndex++;
            }

            if (reverseHoldNoteIndex > chart.reverseHoldNotes.Count - 1) return;

            while (reverseActive && reverseNoteIndex < chart.reverseNotes.Count && GetScaledTime(timeManager.VisualTime) - (spawnOffset * ScrollDuration()) >= chart.reverseNotes[reverseNoteIndex].ScaledVisualTime)
            {
                Note currentNote = chart.reverseNotes[reverseNoteIndex];

                GetNote(currentNote, true);

                if (currentNote.NoteType is ObjectEnums.NoteType.SnapForward or ObjectEnums.NoteType.SnapBackward)
                    GetSnap(currentNote, true);
                
                if (currentNote.NoteType is ObjectEnums.NoteType.SwipeClockwise or ObjectEnums.NoteType.SwipeCounterclockwise)
                    GetSwipe(currentNote, true);

                if (currentNote.BonusType is ObjectEnums.BonusType.R_Note)
                    GetR_Effect(currentNote, true);

                reverseNoteIndex++;
            }

            if (reverseGimmickIndex > chart.reverseGimmicks.Count - 1) return;

            /*while (reverseActive && reverseHoldNoteIndex < chart.reverseHoldNotes.Count && GetScaledTime(timeManager.VisualTime) + ScrollDuration() >= chart.reverseHoldNotes[reverseHoldNoteIndex].Start.ScaledVisualTime)
            {
                // hold note stuff here
            }*/
        }


        [SerializeField] private float spawnOffset = 0.25f;
        [SerializeField] private float scrollOffset = 2.0f;

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
                    AnimateObject(container, noteGarbage, container.note.ScaledVisualTime, container.renderer.transform, 0.25f);
                else ReverseAnimateObject(container, noteGarbage, container.note.ScaledVisualTime, container.renderer.transform, 1f);
            }

            foreach (SnapContainer container in snapPool.ActiveObjects)
            {
                // Set only reverse containers active during a reverse.
                if (reverseActive)
                    container.gameObject.SetActive(container.reverse);
                else
                    container.gameObject.SetActive(!container.reverse);

                if (!container.reverse)
                    AnimateObject(container, snapGarbage, container.note.ScaledVisualTime, container.transform, 0.25f);
                else ReverseAnimateObject(container, snapGarbage, container.note.ScaledVisualTime, container.transform, 1f);
            }

            foreach (SwipeContainer container in swipePool.ActiveObjects)
            {
                // Set only reverse containers active during a reverse.
                if (reverseActive)
                    container.gameObject.SetActive(container.reverse);
                else
                    container.gameObject.SetActive(!container.reverse);

                if (!container.reverse)
                    AnimateObject(container, swipeGarbage, container.note.ScaledVisualTime, container.transform, 0.25f);
                else ReverseAnimateObject(container, swipeGarbage, container.note.ScaledVisualTime, container.transform, 1f);
            }

            foreach (GenericContainer container in r_EffectPool.ActiveObjects)
            {
                // Set only reverse containers active during a reverse.
                if (reverseActive)
                    container.gameObject.SetActive(container.reverse);
                else
                    container.gameObject.SetActive(!container.reverse);

                if (!container.reverse)
                    AnimateObject(container, r_EffectGarbage, container.note.ScaledVisualTime, container.transform, 0.25f);
                else ReverseAnimateObject(container, r_EffectGarbage, container.note.ScaledVisualTime, container.transform, 1f);
            }

            foreach (GenericContainer container in syncPool.ActiveObjects)
                AnimateObject(container, syncGarbage, container.note.ScaledVisualTime, container.renderer.transform, 0.25f);

            foreach (BarLineContainer container in barLinePool.ActiveObjects)
                AnimateObject(container, barLineGarbage, container.time, container.transform, 0);
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

            // Clear the garbage lists
            noteGarbage.Clear();
            snapGarbage.Clear();
            swipeGarbage.Clear();
            r_EffectGarbage.Clear();
            barLineGarbage.Clear();
            syncGarbage.Clear();
        }


        private void AnimateObject<T> (T obj, List<T> garbage, float time, Transform transform, float despawnTime)
        {
            float distance = time - GetScaledTime(timeManager.VisualTime);
            float scroll = SaturnMath.InverseLerp(ScrollDuration(), 0, distance);

            transform.position = new Vector3(0, 0, Mathf.LerpUnclamped(-6, 0, scroll));
            transform.localScale = new Vector3(scroll, scroll, scroll);

            // Collect all objects after passing the judgement line to return them to their pool.
            if (GetScaledTime(timeManager.VisualTime) - ScrollDuration() * despawnTime >= time)
            {
                garbage.Add(obj);
            }
        }

        private void ReverseAnimateObject<T> (T obj, List<T> garbage, float time, Transform transform, float despawnTime)
        {
            float distance = time - GetScaledTime(timeManager.VisualTime);
            float scroll = scrollOffset - SaturnMath.InverseLerp(ScrollDuration(), 0, distance);

            transform.position = new Vector3(0, 0, Mathf.LerpUnclamped(-6, 0, scroll));
            transform.localScale = new Vector3(scroll, scroll, scroll);

            // Collect all objects after passing the judgement line to return them to their pool.
            if (GetScaledTime(timeManager.VisualTime) - ScrollDuration() * despawnTime >= time)
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

        private SnapContainer GetSnap(Note input, bool reverse = false)
        {
            SnapContainer container = snapPool.GetObject();

            container.note = input;
            container.renderer.SetRenderer(input);
            container.reverse = reverse;

            container.transform.SetParent(activeObjectsContainer);
            container.gameObject.SetActive(true);

            return container;
        }

        private SwipeContainer GetSwipe(Note input, bool reverse = false)
        {
            SwipeContainer container = swipePool.GetObject();

            container.note = input;
            container.renderer.SetRenderer(input);
            container.reverse = reverse;

            container.transform.SetParent(activeObjectsContainer);
            container.gameObject.SetActive(true);

            return container;
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

        private GenericContainer GetSync(Note input)
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
            float hiSpeedTime = lastHiSpeedChange.Time;
            float hiSpeedScaledTime = lastHiSpeedChange.ScaledVisualTime;
            
            return hiSpeedScaledTime + ((input - hiSpeedTime) * hiSpeed);
        }

        private float ScrollDuration()
        {
            // A Note scrolling from it's spawn point to the judgement line takes
            // approximately 3266.667 milliseconds. This is 10x that, because
            // NoteSpeed is stored as an integer that's 10x the actual value.
            return 32660.667f / SettingsManager.Instance.PlayerSettings.GameSettings.NoteSpeed;
        }


        void Update()
        {
            if (!bgmManager.bgmPlayer.isPlaying) return;

            ProcessBgmData();

            ProcessHiSpeed();
            ProcessReverseGimmicks();

            ProcessMasks();
            ProcessNotes();

            ProcessSync();
            ProcessBarLines();
            
            UpdateObjects();
            ReleaseObjects();
        }
    }
}
