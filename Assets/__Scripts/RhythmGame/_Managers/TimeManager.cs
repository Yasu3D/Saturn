using System;
using SaturnGame.Settings;
using UnityEngine;
using UnityEngine.Scripting;

namespace SaturnGame.RhythmGame
{
public class TimeManager : MonoBehaviour
{
    [SerializeField] private ChartManager chartManager;
    [SerializeField] private AudioSource bgmPlayer;

    //public float StaticAudioOffset {get; private set; } = -20;
    public float StaticAudioOffset = -10;
    public float PlaybackSpeed { get; private set; } = 1.0f;

    [Space(20)] [SerializeField] private float visualTimeScale = 1.0f;
    [SerializeField] private float timeWarpMultiplier = 1.0f;
    [SerializeField] private float forceSyncDiscrepancy = 50f;

    public enum SongState
    {
        NotYetStarted,
        Playing,
        Finished,
    }

    public SongState State { get; private set; } = SongState.NotYetStarted;

    private void SetPlaybackSpeed(float speed, bool clamp = true)
    {
        float clampedSpeed = clamp ? Mathf.Clamp01(speed) : speed;
        PlaybackSpeed = clampedSpeed;
        bgmPlayer.pitch = clampedSpeed;
    }

    /// <summary>
    /// Returns Song Position in ms, accounting for the "audio offset" defined in the chart.
    /// Warning: this is based on <see cref="AudioSource.time"/>, which does not update continuously. It's possible for
    /// this value to stay the same over multiple frames. See docs for <see cref="AudioSource.time"/>.
    /// </summary>
    /// <returns></returns>
    private float BgmTime()
    {
        if (bgmPlayer.clip == null)
            return -1;

        return Mathf.Max(0, 1000 * (bgmPlayer.time + chartManager.Chart.AudioOffset));
    }

    /// <summary>
    /// Time synchronized with BgmTime, but smoothed to handle multiple frames between BgmTime updates.
    /// </summary>
    private float AudioTimeMs { get; set; }

    private float LastFrameAudioTimeMs { get; set; }

    /// <summary>
    /// Total audio offset in ms. This affects the sync between the gameplay timer (used for judgements) and the audio
    /// timer which is synced to the audio track. It does not affect the sync between the gameplay and visuals.
    /// Positive means that the audio plays later, negative means that the audio plays earlier (compared to the gameplay
    /// timer).
    /// </summary>
    private float AudioOffsetMs =>
        StaticAudioOffset + SettingsManager.Instance.PlayerSettings.GameSettings.JudgementOffset * 10 /* temp */;

    /// <summary>
    /// Core gameplay time in ms, used for judgements. Do not use this for audio or visuals, use
    /// <see cref="AudioTimeMs"/> or <see cref="VisualTimeMs"/> instead, which are correctly offset.
    /// Currently, this is derived from <see cref="AudioOffsetMs"/> with the audio offset applied.
    /// </summary>
    public float GameplayTimeMs => AudioTimeMs + AudioOffsetMs;

    public float LastFrameGameplayTimeMs => LastFrameAudioTimeMs + AudioOffsetMs;

    /// <summary>
    /// Total visual offset in ms. This affects the sync between the gameplay timer (used for judgements) and the visual
    /// time used by the rendering engine. It does not affect the sync between the gameplay and audio.
    /// Positive means that the visuals show later, negative means that the visuals show earlier (compared to the
    /// gameplay timer).
    /// </summary>
    private float VisualOffsetMs = 0; // todo: tweak this value

    /// <summary>
    /// Visual time in ms. This must be used for all visuals instead of GameplayTimeMs.
    /// </summary>
    public float VisualTimeMs => GameplayTimeMs - VisualOffsetMs;

    private void UpdateTime()
    {
        if (State != SongState.Playing) return;

        LastFrameAudioTimeMs = AudioTimeMs;
        AudioTimeMs += Time.deltaTime * visualTimeScale * 1000;
    }

    /// <summary>
    /// Synchronises VisualTime with BgmTime if they drift apart too much. <br />
    /// Huge thanks to AllPoland for sharing this code from ArcViewer with me.
    /// </summary>
    private void ReSync()
    {
        if (!bgmPlayer.isPlaying)
        {
            // If the song is not playing, just use the gameplay clock (Time.deltaTime).
            // So VisualTimeScale should just be exactly the PlaybackSpeed.
            visualTimeScale = PlaybackSpeed;
            return;
        }

        float discrepancy = AudioTimeMs - BgmTime();
        float absDiscrepancy = Mathf.Abs(discrepancy);

        if (absDiscrepancy >= forceSyncDiscrepancy || PlaybackSpeed == 0)
        {
            // Snap directly to BgmTime if discrepancy gets too high.
            Debug.Log($"Force correcting by {discrepancy}");
            AudioTimeMs = BgmTime();
        }

        // Warp VisualTime to re-align with audio
        // * 0.001f because SaturnGame uses milliseconds for all timing.
        float timeWarp = discrepancy * timeWarpMultiplier * 0.001f;
        visualTimeScale = PlaybackSpeed - timeWarp;
    }

    private void UpdateState()
    {
        switch (State)
        {
            case SongState.NotYetStarted:
            {
                // Wait for playback to start, don't update here.
                break;
            }
            case SongState.Playing:
            {
                if (GameplayTimeMs > chartManager.Chart.EndOfChart.TimeMs)
                    State = SongState.Finished;
                break;
            }
            case SongState.Finished:
            {
                // Terminal.
                break;
            }
            default:
            {
                throw new ArgumentOutOfRangeException();
            }
        }
    }

    public void StartSong()
    {
        bgmPlayer.clip = chartManager.BGM;
        State = SongState.Playing;
        bgmPlayer.Play();
    }

    private void Update()
    {
        UpdateTime();
        ReSync();
        UpdateState();

        if (Input.GetKeyDown(KeyCode.P))
        {
            if (State == SongState.NotYetStarted)
            {
                Debug.Log($"offset {SettingsManager.Instance.PlayerSettings.GameSettings.JudgementOffset}");
                StartSong();
            }
            else
                Debug.LogWarning("Tried to start playback, but already started.");
        }

        if (Input.GetKey(KeyCode.M))
            SetPlaybackSpeed(1);

        if (Input.GetKeyDown(KeyCode.I))
            SetPlaybackSpeed(2 * PlaybackSpeed, false);

        if (Input.GetKeyDown(KeyCode.J))
            SetPlaybackSpeed(0.5f * PlaybackSpeed, false);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!Application.isEditor)
                GarbageCollector.GCMode = GarbageCollector.Mode.Enabled;
            SceneSwitcher.Instance.LoadScene("_SongSelect");
        }
    }
}
}
