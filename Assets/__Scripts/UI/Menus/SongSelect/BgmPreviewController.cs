using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;
using SaturnGame.RhythmGame;
using SaturnGame.Loading;

namespace SaturnGame.UI
{
    public class BgmPreviewController : MonoBehaviour
    {
        public AudioSource bgmSource;

        private string bgmPath;
        private string prevBgmPath;
        private float startTime;
        private float durationTime;
        private float StopTime => startTime + durationTime;
        private float FadeTime => StopTime - fadeDuration;
        private const float fadeDuration = 0.25f;
        private const float lingerThreshold = 0.5f;
        private float lingerTimer = 0.0f;
        private bool IsLingering { get => lingerTimer >= lingerThreshold; }

        private PreviewState state = PreviewState.Idle;
        private enum PreviewState
        {
            Idle, // not playing, waiting for auto-start
            Playing, // actively playing audio
            FadingOut, // fading out audio
            InvalidAudio, // audio invalid, won't auto-start but reactivated by song switch.
            Stopped // stopped completely, won't auto-start.
        }


        void Update()
        {
            lingerTimer += Time.deltaTime;

            if (state is PreviewState.Idle)
            {
                if (IsLingering) StartBgmPreview();
            }

            if (state is PreviewState.Playing)
            {
                if (bgmSource.time >= FadeTime) FadeoutBgmPreview();
            }
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

        public async void StartBgmPreview()
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
            bgmSource.DOFade(1, fadeDuration).SetEase(Ease.Linear);
        }

        public async void FadeoutBgmPreview(bool forceStop = false)
        {
            if (state is not PreviewState.Playing) return;

            // a bit nervous about setting the state in an async function (code race maybe?)
            // seems ok from quick testing tho...
            state = PreviewState.FadingOut;
            bgmSource.DOFade(0, fadeDuration).SetEase(Ease.Linear);
            await Awaitable.WaitForSecondsAsync(fadeDuration);
            StopBgmPreview(forceStop);
        }

        public void StopBgmPreview(bool forceStop = false)
        {
            if (!bgmSource.isPlaying) return;

            state = forceStop ? PreviewState.Stopped : PreviewState.Idle;
            bgmSource.Stop();
        }

        private async Task LoadBgm()
        {
            // .ogg makes the game freeze because of decompression.
            // Please just use .wav... I beg you.
            AudioClip bgmClip = await AudioLoader.LoadBgm(bgmPath);

            ChartManager.Instance.BGMClip = bgmClip;
            bgmSource.clip = bgmClip;
        }
    }
}
