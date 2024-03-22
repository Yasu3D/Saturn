using DG.Tweening;
using SaturnGame.Loading;
using SaturnGame.RhythmGame;
using UnityEngine;

namespace SaturnGame.UI
{
    public class BgmPreviewController : MonoBehaviour
    {
        [SerializeField] private AudioSource bgmSource;

        private string bgmPath;
        private string prevBgmPath;
        private float startTime;
        private float durationTime;
        private float StopTime => startTime + durationTime;
        private float FadeTime => StopTime - FadeDuration;
        private const float FadeDuration = 0.25f;
        private const float LingerThreshold = 0.5f;
        private float lingerTimer;
        private bool IsLingering => lingerTimer >= LingerThreshold;

        private PreviewState state = PreviewState.Idle;
        private enum PreviewState
        {
            Idle, // not playing, waiting for auto-start
            Playing, // actively playing audio
            FadingOut, // fading out audio
            InvalidAudio, // audio invalid, won't auto-start but reactivated by song switch.
            Stopped, // stopped completely, won't auto-start.
        }


        private void Update()
        {
            lingerTimer += Time.deltaTime;

            if (state is PreviewState.Idle && IsLingering)
                StartBgmPreview();

            if (state is PreviewState.Playing && bgmSource.time >= FadeTime)
                FadeoutBgmPreview();
        }

        public void SetBgmValues(string path, float start, float duration)
        {
            bgmPath = path;
            startTime = start;
            durationTime = duration;
            if (state is PreviewState.InvalidAudio)
                state = PreviewState.Idle;
        }

        public void ResetLingerTimer()
        {
            lingerTimer = 0;
        }

        private async void StartBgmPreview()
        {
            if (durationTime <= 0 || bgmSource.isPlaying) return;

            state = PreviewState.Playing;

            if (bgmPath != prevBgmPath)
            {
                prevBgmPath = bgmPath;
                await LoadBgm();
            }

            if (bgmSource.clip == null || startTime >= bgmSource.clip.length)
            {
                state = PreviewState.InvalidAudio;
                return;
            }

            bgmSource.volume = 0;
            bgmSource.time = startTime;
            bgmSource.Play();
            bgmSource.DOFade(1, FadeDuration).SetEase(Ease.Linear);
        }

        public async void FadeoutBgmPreview(bool forceStop = false)
        {
            if (state is not PreviewState.Playing) return;

            // a bit nervous about setting the state in an async function (code race maybe?)
            // seems ok from quick testing tho...
            state = PreviewState.FadingOut;
            bgmSource.DOFade(0, FadeDuration).SetEase(Ease.Linear);
            await Awaitable.WaitForSecondsAsync(FadeDuration);
            StopBgmPreview(forceStop);
        }

        private void StopBgmPreview(bool forceStop = false)
        {
            if (!bgmSource.isPlaying) return;

            state = forceStop ? PreviewState.Stopped : PreviewState.Idle;
            bgmSource.Stop();
        }

        private async Awaitable LoadBgm()
        {
            AudioClip bgmClip = await AudioLoader.LoadBgm(bgmPath);

            ChartManager.Instance.BGMClip = bgmClip;
            bgmSource.clip = bgmClip;
        }
    }
}
