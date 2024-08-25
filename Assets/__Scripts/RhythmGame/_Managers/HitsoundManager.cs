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
    [SerializeField] private ChartManager chartManager;
    [SerializeField] private TimeManager timeManager;

    [SerializeField] private AudioClip guideSound;
    [SerializeField] private AudioClip touchSound;
    [SerializeField] private AudioClip swipeSound;
    [SerializeField] private AudioClip bonusSound;
    [SerializeField] private AudioClip rNoteSound;

    public int PoolSize;
    [SerializeField] private AudioSource hitsoundSourcePrefab;
    [SerializeField] private Transform availableHitsoundSourcesParent;
    [SerializeField] private Transform activeHitsoundSourcesParent;
    public List<AudioSource> AvailableHitsoundSources = new();
    public List<AudioSource> ActiveHitsoundSources = new();

    private const float HitsoundLevelDbOffset = -10f;

    private static SoundSettings SoundSettings => SettingsManager.Instance.PlayerSettings.SoundSettings;

    // Start is called before the first frame update
    private void Start()
    {
        EnsurePoolSize();

        SetHitsoundMixerLevel();
    }

    private void SetHitsoundMixerLevel()
    {
        float settingsHitsoundVolume = SoundSettings.HitsoundOverallVolume;
        float newDbLevel = SaturnMath.FractionToDecibel(settingsHitsoundVolume / 100f) + HitsoundLevelDbOffset;
        hitsoundSourcePrefab.outputAudioMixerGroup.audioMixer.SetFloat("HitsoundAttenuationLevel", newDbLevel);
        Debug.Log($"set hitsound level to {newDbLevel}dB ({settingsHitsoundVolume})");

        // TODO: also set bgm level
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

    private void PlayHitsound(AudioClip clip, int volume = 100)
    {
        AudioSource source = GetHitsoundSourceFromPool();
        source.clip = clip;
        source.volume = volume / 100f;
        source.Play();
    }

    public void PlayNoteHitsound([NotNull] Note note)
    {
        switch (note)
        {
            case TouchNote:
                PlayHitsound(touchSound, SoundSettings.TouchNoteVolume);
                break;
            case ChainNote:
                PlayHitsound(touchSound, SoundSettings.ChainNoteVolume);
                break;
            case HoldNote:
                PlayHitsound(touchSound, SoundSettings.HoldNoteVolume);
                break;
            case SnapNote:
                PlayHitsound(touchSound, SoundSettings.SnapNoteVolume);
                PlayHitsound(swipeSound, SoundSettings.SnapNoteVolume);
                break;
            case SwipeNote:
                PlayHitsound(touchSound, SoundSettings.SlideNoteVolume);
                PlayHitsound(swipeSound, SoundSettings.SlideNoteVolume);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(note));
        }

        switch (note.BonusType)
        {
            case Note.NoteBonusType.None:
                break;
            case Note.NoteBonusType.Bonus:
                PlayHitsound(bonusSound, SoundSettings.BonusEffectVolume);
                break;
            case Note.NoteBonusType.RNote:
                PlayHitsound(rNoteSound, SoundSettings.RNoteEffectVolume);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private bool ShouldPlayGuideSound()
    {
        Chart chart = chartManager.Chart;

        foreach (Note note in chart.Notes)
        {
            if (timeManager.LastFrameGameplayTimeMs < note.TimeMs && note.TimeMs <= timeManager.GameplayTimeMs)
                return true;
        }

        foreach (HoldNote note in chart.HoldNotes)
        {
            if (timeManager.LastFrameGameplayTimeMs < note.TimeMs && note.TimeMs <= timeManager.GameplayTimeMs)
                return true;
            if (timeManager.LastFrameGameplayTimeMs < note.End.TimeMs && note.End.TimeMs <= timeManager.GameplayTimeMs)
                return true;
        }

        return false;
    }

    private float time;

    private void Update()
    {
        EnsurePoolSize();
        ReturnFinishedHitsoundsToPool();

        if (chartManager.Chart != null)
        {
            if (ShouldPlayGuideSound())
            {
                // Note: ideally we would play the guide sound at the exact time the note is supposed to be hit,
                // using PlayScheduled, but I'm too lazy to do this now. Given a decent enough framerate, this is
                // pretty good.
                PlayHitsound(guideSound, SoundSettings.GuideVolume);
            }
        }
    }
}
}
