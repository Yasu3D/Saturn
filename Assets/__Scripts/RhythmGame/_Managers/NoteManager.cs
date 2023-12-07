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


        private void UpdateObjects()
        {
            foreach (NoteContainer container in notePool.ActiveObjects)
                AnimateObject(container, noteGarbage, container.note.ScaledVisualTime, container.renderer.transform, ScrollDuration());

            foreach (SnapContainer container in snapPool.ActiveObjects)
                AnimateObject(container, snapGarbage, container.note.ScaledVisualTime, container.transform, ScrollDuration());

            foreach (SwipeContainer container in swipePool.ActiveObjects)
                AnimateObject(container, swipeGarbage, container.note.ScaledVisualTime, container.transform, ScrollDuration());

            foreach (GenericContainer container in r_EffectPool.ActiveObjects)
                AnimateObject(container, r_EffectGarbage, container.note.ScaledVisualTime, container.transform, ScrollDuration());

            foreach (BarLineContainer container in barLinePool.ActiveObjects)
                AnimateObject(container, barLineGarbage, container.time, container.transform, 0);

            foreach (GenericContainer container in syncPool.ActiveObjects)
                AnimateObject(container, syncGarbage, container.note.ScaledVisualTime, container.renderer.transform, ScrollDuration());
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

            // Clear the garbage lists
            noteGarbage.Clear();
            snapGarbage.Clear();
            swipeGarbage.Clear();
            r_EffectGarbage.Clear();
            barLineGarbage.Clear();
        }


        private void AnimateObject<T> (T obj, List<T> garbage, float time, Transform transform, float despawnTime)
        {
            float distance = time - GetScaledTime(timeManager.VisualTime);
            float scroll = SaturnMath.InverseLerp(ScrollDuration(), 0, distance);

            transform.position = new Vector3(0, 0, Mathf.LerpUnclamped(-6, 0, scroll));
            transform.localScale = new Vector3(scroll, scroll, scroll);

            // Collect all objects after passing the judgement line to return them to their pool.
            if (GetScaledTime(timeManager.VisualTime) - despawnTime * 0.25f >= time)
            {
                garbage.Add(obj);
            }
        }


        private NoteContainer GetNote(Note input)
        {
            NoteContainer container = notePool.GetObject();
            
            container.note = input;
            int noteWidth = SettingsManager.Instance.PlayerSettings.DesignSettings.NoteWidth;
            container.renderer.SetRenderer(input, noteWidth);

            container.transform.SetParent(activeObjectsContainer);
            container.gameObject.SetActive(true);

            return container;
        }

        private SnapContainer GetSnap(Note input)
        {
            SnapContainer container = snapPool.GetObject();

            container.note = input;
            container.renderer.SetRenderer(input);

            container.transform.SetParent(activeObjectsContainer);
            container.gameObject.SetActive(true);

            return container;
        }

        private SwipeContainer GetSwipe(Note input)
        {
            SwipeContainer container = swipePool.GetObject();

            container.note = input;
            container.renderer.SetRenderer(input);

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

        private GenericContainer GetR_Effect(Note input)
        {
            GenericContainer container = r_EffectPool.GetObject();

            container.note = input;
            container.renderer.SetRenderer(input.Size, input.Position);

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
            ProcessMasks();
            ProcessNotes();
            ProcessSync();
            ProcessBarLines();
            
            UpdateObjects();
            ReleaseObjects();
        }
    }
}
