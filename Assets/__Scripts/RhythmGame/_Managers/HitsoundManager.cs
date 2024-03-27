using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SaturnGame.Settings;
using UnityEngine;

namespace SaturnGame.RhythmGame
{
public class HitsoundManager : MonoBehaviour
{
    [SerializeField] private TimeManager timeManager;
    [SerializeField] private ScoringManager scoringManager;

    [SerializeField] private AudioClip guideSound;
    [SerializeField] private AudioClip touchSound;
    [SerializeField] private AudioClip swipeSound;

    public int PoolSize;
    [SerializeField] private AudioSource hitsoundSourcePrefab;
    [SerializeField] private Transform availableHitsoundSourcesParent;
    [SerializeField] private Transform activeHitsoundSourcesParent;
    public List<AudioSource> AvailableHitsoundSources = new();
    public List<AudioSource> ActiveHitsoundSources = new();

    // Start is called before the first frame update
    private void Start()
    {
        EnsurePoolSize();

        SetHitsoundMixerLevel();
    }

    private void SetHitsoundMixerLevel()
    {
        float settingsHitsoundVolume = SettingsManager.Instance.PlayerSettings.SoundSettings.HitsoundOverallVolume;
        float newDbLevel = SaturnMath.FractionToDecibel(settingsHitsoundVolume / 100f);
        hitsoundSourcePrefab.outputAudioMixerGroup.audioMixer.SetFloat("HitsoundAttenuationLevel", newDbLevel);
        Debug.Log($"set hitsound level to {newDbLevel}dB ({settingsHitsoundVolume})");
    }

    private void EnsurePoolSize()
    {
        int currentSize = AvailableHitsoundSources.Count + ActiveHitsoundSources.Count;
        int delta = PoolSize - currentSize;
        if (delta <= 0) return;

        for (int i = 0; i < delta; i++)
            AddHitsoundSourceToPool();
    }

    [NotNull]
    private AudioSource AddHitsoundSourceToPool()
    {
        AudioSource newSource = Instantiate(hitsoundSourcePrefab, availableHitsoundSourcesParent);
        newSource.gameObject.SetActive(false);
        AvailableHitsoundSources.Add(newSource);
        return newSource;
    }

    [NotNull]
    private AudioSource GetHitsoundSourceFromPool()
    {
        AudioSource source = AvailableHitsoundSources.FirstOrDefault() ?? AddHitsoundSourceToPool();
        AvailableHitsoundSources.Remove(source);
        ActiveHitsoundSources.Add(source);
        source.transform.SetParent(activeHitsoundSourcesParent);
        source.gameObject.SetActive(true);
        return source;
    }

    private void ReturnHitsoundSourceToPool([NotNull] AudioSource source)
    {
        if (!ActiveHitsoundSources.Contains(source))
            throw new Exception("Tried to return a hitsound source to the pool that was not in the active list");
        source.gameObject.SetActive(false);
        source.transform.SetParent(availableHitsoundSourcesParent);
        ActiveHitsoundSources.Remove(source);
        AvailableHitsoundSources.Add(source);
    }

    private void ReturnFinishedHitsoundsToPool()
    {
        for (int i = ActiveHitsoundSources.Count - 1; i >= 0; i--)
        {
            AudioSource source = ActiveHitsoundSources[i];
            if (!source.isPlaying) ReturnHitsoundSourceToPool(source);
        }
    }

    private void PlayHitsound(AudioClip clip, float volume = 0.3f)
    {
        AudioSource source = GetHitsoundSourceFromPool();
        source.clip = clip;
        source.volume = volume;
        source.Play();
    }

    private bool ShouldPlayGuideSound()
    {
        Chart chart = ChartManager.Instance.Chart;

        foreach (Note note in chart.Notes)
        {
            if (timeManager.LastFrameVisualTimeMs < note.TimeMs && note.TimeMs <= timeManager.VisualTimeMs)
                return true;
        }

        foreach (HoldNote note in chart.HoldNotes)
        {
            if (timeManager.LastFrameVisualTimeMs < note.TimeMs && note.TimeMs <= timeManager.VisualTimeMs)
                return true;
            if (timeManager.LastFrameVisualTimeMs < note.End.TimeMs && note.End.TimeMs <= timeManager.VisualTimeMs)
                return true;
        }

        return false;
    }

    private float time;

    private void Update()
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
                PlayHitsound(guideSound);
            }
        }

        if (scoringManager.NeedTouchHitsound)
        {
            PlayHitsound(touchSound);
            scoringManager.NeedTouchHitsound = false;
        }

        if (scoringManager.NeedSwipeSnapHitsound)
        {
            PlayHitsound(swipeSound);
            scoringManager.NeedSwipeSnapHitsound = false;
        }
    }
}
}