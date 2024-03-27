using System;
using SaturnGame.Settings;
using UnityEngine;

namespace SaturnGame.RhythmGame
{
public class TimeManager : MonoBehaviour
{
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
    /// Returns Song Position in ms.
    /// </summary>
    /// <returns></returns>
    private float BgmTime()
    {
        if (bgmPlayer.clip == null)
            return -1;

        return Mathf.Max(0, 1000 * (bgmPlayer.time + ChartManager.Instance.Chart.AudioOffset));
    }

    /// <summary>
    /// <b>Use VisualTime! This does NOT include any offsets!</b><br />
    /// Time synchronized with BgmTime, but properly updated every frame for smooth visuals beyond 60fps.
    /// </summary>
    public float RawVisualTimeMs { get; private set; }

    private float LastFrameRawVisualTimeMs { get; set; }

    private float TotalOffsetMs =>
        StaticAudioOffset + SettingsManager.Instance.PlayerSettings.GameSettings.JudgementOffset * 10 /* temp */;

    /// <summary>
    /// <b>Includes sync calibration and user offsets!</b> <br />
    /// Time synchronized with BgmTime, but properly updated every frame for smooth visuals beyond 60fps.
    /// </summary>
    public float VisualTimeMs => RawVisualTimeMs + TotalOffsetMs;

    public float LastFrameVisualTimeMs => LastFrameRawVisualTimeMs + TotalOffsetMs;

    private void UpdateVisualTime()
    {
        if (State != SongState.Playing) return;

        LastFrameRawVisualTimeMs = RawVisualTimeMs;
        RawVisualTimeMs += Time.deltaTime * visualTimeScale * 1000;
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

        float discrepancy = RawVisualTimeMs - BgmTime();
        float absDiscrepancy = Mathf.Abs(discrepancy);

        if (absDiscrepancy >= forceSyncDiscrepancy || PlaybackSpeed == 0)
        {
            // Snap directly to BgmTime if discrepancy gets too high.
            Debug.Log($"Force correcting by {discrepancy}");
            RawVisualTimeMs = BgmTime();
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
                if (VisualTimeMs > ChartManager.Instance.Chart.EndOfChart.TimeMs)
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

    private void Update()
    {
        UpdateVisualTime();
        ReSync();
        UpdateState();

        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log($"offset {SettingsManager.Instance.PlayerSettings.GameSettings.JudgementOffset}");
            AudioClip bgm = ChartManager.Instance.BGMClip;
            bgmPlayer.clip = bgm;
            State = SongState.Playing;
            bgmPlayer.Play();
        }

        if (Input.GetKey(KeyCode.M))
            SetPlaybackSpeed(1);

        if (Input.GetKeyDown(KeyCode.I))
            SetPlaybackSpeed(2 * PlaybackSpeed, false);

        if (Input.GetKeyDown(KeyCode.J))
            SetPlaybackSpeed(0.5f * PlaybackSpeed, false);

        if (Input.GetKeyDown(KeyCode.Escape))
            SceneSwitcher.Instance.LoadScene("_SongSelect");
    }
}
}