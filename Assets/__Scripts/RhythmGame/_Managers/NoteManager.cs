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
            while (noteIndex < chart.notes.Count && timeManager.VisualTime + ScrollDuration() >= chart.notes[noteIndex].Time)
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

        private float nextMeasureTime = 0;
        private void ProcessBarLines()
        {
            if (timeManager.VisualTime + ScrollDuration() >= nextMeasureTime)
            {
                GetBarLine(nextMeasureTime);
                nextMeasureTime += bgmManager.BeatDuration * 4;
            }
        }


        private void UpdateObjects()
        {
            foreach (NoteContainer container in notePool.ActiveObjects)
                AnimateObject(container, noteGarbage, container.note.Time, container.renderer.transform);

            foreach (SnapContainer container in snapPool.ActiveObjects)
                AnimateObject(container, snapGarbage, container.note.Time, container.transform);

            foreach (SwipeContainer container in swipePool.ActiveObjects)
                AnimateObject(container, swipeGarbage, container.note.Time, container.transform);

            foreach (GenericContainer container in r_EffectPool.ActiveObjects)
                AnimateObject(container, r_EffectGarbage, container.note.Time, container.transform);

            foreach (BarLineContainer container in barLinePool.ActiveObjects)
                AnimateObject(container, barLineGarbage, container.time, container.transform);
        }

        private void ReleaseObjects()
        {
            // get rid of objects by releasing them back into the pool.
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

            noteGarbage.Clear();
            snapGarbage.Clear();
            swipeGarbage.Clear();
            r_EffectGarbage.Clear();
            barLineGarbage.Clear();
        }


        private float ScrollDuration()
        {
            // A note scrolling from it's spawnpoint to the judgement line takes
            // approximately 3266.667 milliseconds. This is 10x that, because
            // NoteSpeed is stored as an integer that's 10x the actual value.
            return 32660.667f / SettingsManager.Instance.PlayerSettings.GameSettings.NoteSpeed;
        }

        private void AnimateObject<T> (T obj, List<T> garbage, float time, Transform transform)
        {
            float distance = time - timeManager.VisualTime;
            float scroll = SaturnMath.InverseLerp(ScrollDuration(), 0, distance);

            transform.position = new Vector3(0, 0, Mathf.LerpUnclamped(-6, 0, scroll));
            transform.localScale = new Vector3(scroll, scroll, scroll);

            // Collect all objects after passing the judgement line. To return them to their pool.
            // Using 1/4 of the ScrollDuration to keep the distance they're collected at consistent.
            if (timeManager.VisualTime - ScrollDuration() * 0.25f >= time)
            {
                garbage.Add(obj);
            }
        }

        private NoteContainer GetNote(Note input)
        {
            NoteContainer container = notePool.GetObject();
            
            container.note = input;
            container.renderer.SetRenderer(input, 3);

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

        private GenericContainer GetR_Effect(Note input)
        {
            GenericContainer container = r_EffectPool.GetObject();

            container.note = input;
            container.renderer.SetRenderer(input.Size, input.Position);

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

        void Update()
        {
            if (!bgmManager.bgmPlayer.isPlaying) return;

            ProcessMasks();
            ProcessNotes();
            ProcessBarLines();
            UpdateObjects();
            ReleaseObjects();
        }
    }
}
