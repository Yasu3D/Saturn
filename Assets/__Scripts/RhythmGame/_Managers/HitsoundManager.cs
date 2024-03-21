using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

namespace SaturnGame.RhythmGame
{
    public class HitsoundManager : MonoBehaviour
    {
        [SerializeField] private TimeManager timeManager;
        [SerializeField] private ScoringManager scoringManager;
        [SerializeField] private TMPro.TextMeshProUGUI debugText;
        
        [SerializeField] private AudioClip guideSound;
        [SerializeField] private AudioClip touchSound;
        [SerializeField] private AudioClip swipeSound;
        //[SerializeField] private AudioSource audioSource;

        public int PoolSize;
        [SerializeField] private AudioSource hitsoundSourcePrefab;
        [SerializeField] private Transform availableHitsoundSourcesParent;
        [SerializeField] private Transform activeHitsoundSourcesParent;
        public List<AudioSource> AvailableHitsoundSources = new List<AudioSource>();
        public List<AudioSource> ActiveHitsoundSources = new List<AudioSource>();
        
        // Start is called before the first frame update
        void Start()
        {
            EnsurePoolSize();
        }

        void EnsurePoolSize()
        {
            int currentSize = AvailableHitsoundSources.Count + ActiveHitsoundSources.Count;
            int delta = PoolSize - currentSize;
            if (delta > 0)
            {
                for (int i = 0; i < delta; i++)
                {
                    AddHitsoundSourceToPool();
                }
            }
        }

        private AudioSource AddHitsoundSourceToPool()
        {
            AudioSource newSource = Instantiate(hitsoundSourcePrefab, availableHitsoundSourcesParent);
            newSource.gameObject.SetActive(false);
            AvailableHitsoundSources.Add(newSource);
            return newSource;
        }

        public AudioSource GetHitsoundSourceFromPool()
        {
            AudioSource source;
            if (AvailableHitsoundSources.Count > 0)
            {
                source = AvailableHitsoundSources.First();
            }
            else
            {
                source = AddHitsoundSourceToPool();
            }
            AvailableHitsoundSources.Remove(source);
            ActiveHitsoundSources.Add(source);
            source.transform.SetParent(activeHitsoundSourcesParent);
            source.gameObject.SetActive(true);
            return source;
        }

        public void ReturnHitsoundSourceToPool(AudioSource source)
        {
            if (!ActiveHitsoundSources.Contains(source))
            {
                throw new Exception("Tried to return a hitsound source to the pool that was not in the active list");
            }
            source.gameObject.SetActive(false);
            source.transform.SetParent(availableHitsoundSourcesParent);
            ActiveHitsoundSources.Remove(source);
            AvailableHitsoundSources.Add(source);
        }
        
        void ReturnFinishedHitsoundsToPool()
        {
            for (int i = ActiveHitsoundSources.Count - 1; i >= 0; i--)
            {
                AudioSource source = ActiveHitsoundSources[i];
                if (!source.isPlaying)
                {
                    ReturnHitsoundSourceToPool(source);
                }
            }
        }

        void playHitsound(AudioClip clip, float volume = 0.3f)
        {
            var source = GetHitsoundSourceFromPool();
            source.clip = clip;
            source.volume = volume;
            source.Play();
        }

        bool ShouldPlayGuideSound()
        {
            var chart = ChartManager.Instance.Chart;
            
            foreach (var note in chart.notes)
            {
                if (timeManager.LastFrameVisualTimeMs < note.TimeMs && note.TimeMs <= timeManager.VisualTimeMs)
                {
                    return true;
                }
            }

            foreach (var note in chart.holdNotes)
            {
                if (timeManager.LastFrameVisualTimeMs < note.TimeMs && note.TimeMs <= timeManager.VisualTimeMs)
                {
                    return true;
                }
                if (timeManager.LastFrameVisualTimeMs < note.End.TimeMs && note.End.TimeMs <= timeManager.VisualTimeMs)
                {
                    return true;
                }
            }

            return false;
        }

        private float time;
        void Update()
        {
            EnsurePoolSize();
            ReturnFinishedHitsoundsToPool();

            if (ChartManager.Instance.LoadedChart != null)
            {
                if (ShouldPlayGuideSound())
                {
                    // Note: ideally we would play the guide sound at the exact time the note is supposed to be hit,
                    // using PlayScheduled, but I'm too lazy to do this now. Given a decent enough framerate, this is
                    // pretty good.
                    playHitsound(guideSound);
                }
            }

            if (scoringManager.NeedTouchHitsound)
            {
                playHitsound(touchSound);
                scoringManager.NeedTouchHitsound = false;
            }
            if (scoringManager.NeedSwipeSnapHitsound)
            {
                playHitsound(swipeSound);
                scoringManager.NeedSwipeSnapHitsound = false;
            }
        }
    }
}