using System;
using System.Collections;
using System.Collections.Generic;
using SaturnGame.Rendering;
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
        private List<SnapRenderer> snapGarbage = new();
        private List<SwipeRenderer> swipeGarbage = new();

        private int maskIndex = 0;
        private void ProcessMasks()
        {
            while (maskIndex < chart.masks.Count && timeManager.VisualTime >= chart.masks[maskIndex].Time)
            {
                guideLaneRenderer.SetMask(chart.masks[maskIndex], timeManager.PlaybackSpeed);
                maskIndex++;
            }
        }

        private int noteIndex = 0;
        [SerializeField] private float noteSpeed = 40.0f;

        // 0.02 is the magic constant for how far ahead you can see at notespeed 40. (this is very wrong)
        // TODO FIX THIS SHIT!!! INVERSE SCALING OR SOMETHING!!!
        private float magicConstant = 20.0f;
        private void ProcessNotes()
        {
            if (noteIndex > chart.notes.Count - 1) return;

            // scan through chart line by line for new notes to instantiate
            // there is something HORRIBLY wrong with this right now.
            float spawnTime = noteSpeed * magicConstant;
            while (timeManager.VisualTime + spawnTime >= chart.notes[noteIndex].Time)
            {
                if (noteIndex >= chart.notes.Count - 1) break;

                Note currentNote = chart.notes[noteIndex];

                NoteContainer container = GetNote(currentNote);

                if (currentNote.NoteType is ObjectEnums.NoteType.SnapForward or ObjectEnums.NoteType.SnapBackward)
                    GetSnap(container);
                
                if (currentNote.NoteType is ObjectEnums.NoteType.SwipeClockwise or ObjectEnums.NoteType.SwipeCounterclockwise)
                    GetSwipe(container);

                noteIndex++;
            }
        }

        private void UpdateNotes()
        {
            foreach (NoteContainer container in notePool.ActiveObjects)
            {
                float distance = container.note.Time - timeManager.VisualTime;
                float scroll = Mathf.InverseLerp(noteSpeed * magicConstant, 0, distance);

                container.transform.position = new Vector3(0, 0, Mathf.LerpUnclamped(-6, 0, scroll));
                container.transform.localScale = Vector3.LerpUnclamped(Vector3.zero, Vector3.one, scroll);

                // collect all notes 200ms past judgementLine to dispose
                if (timeManager.VisualTime >= container.note.Time)
                {
                    noteGarbage.Add(container);

                    if (container.snapRenderer != null) snapGarbage.Add(container.snapRenderer);
                    if (container.swipeRenderer != null) swipeGarbage.Add(container.swipeRenderer);
                }
            }

            // get rid of notes by releasing them back into the pool.
            foreach (NoteContainer container in noteGarbage)
            {
                // clear children before release
                if (container.snapRenderer != null)
                    container.snapRenderer.transform.SetParent(activeObjectsContainer);

                if (container.swipeRenderer != null)
                    container.swipeRenderer.transform.SetParent(activeObjectsContainer);

                notePool.ReleaseObject(container);
            }

            foreach (SnapRenderer snap in snapGarbage)
            {
                snapPool.ReleaseObject(snap);
            }

            foreach (SwipeRenderer swipe in swipeGarbage)
            {
                swipePool.ReleaseObject(swipe);
            }

            noteGarbage.Clear();
            snapGarbage.Clear();
            swipeGarbage.Clear();
        }

        private NoteContainer GetNote(Note input)
        {
            NoteContainer note = notePool.GetObject();
            
            note.note = input;
            note.noteRenderer.SetRendererProperties(input, 3);
            note.noteRenderer.UpdateRenderer();

            note.transform.SetParent(activeObjectsContainer);
            note.gameObject.SetActive(true);

            return note;
        }

        private void GetSnap(NoteContainer input)
        {
            SnapRenderer snap = snapPool.GetObject();
            input.snapRenderer = snap;
            snap.SetRendererProperties(input.note);
            snap.UpdateRenderer();

            snap.transform.SetParent(input.transform);
            snap.transform.localScale = Vector3.one;
            snap.gameObject.SetActive(true);
        }

        private void GetSwipe(NoteContainer input)
        {
            SwipeRenderer swipe = swipePool.GetObject();
            input.swipeRenderer = swipe;
            swipe.SetRendererProperties(input.note);
            swipe.UpdateRenderer();

            swipe.transform.SetParent(input.transform);
            swipe.transform.localScale = Vector3.one;
            swipe.gameObject.SetActive(true);
        }

        void Update()
        {
            if (!bgmManager.bgmPlayer.isPlaying) return;

            ProcessMasks();
            ProcessNotes();
            UpdateNotes();
            BeatDebug();
        }





        // IF YOU SEE THIS, DELETE IT! :]
        private float beatTime;
        [SerializeField] private GameObject testObject;
        void BeatDebug()
        {
            if (timeManager.VisualTime >= beatTime)
            {
                beatTime += bgmManager.BeatDuration;
                testObject.SetActive(!testObject.activeSelf);
            }
        }
    }
}
