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
        private bool isPlaying = false; // can't use bgmSource.isPlaying because of async. -> race condition.

        void Update()
        {
            lingerTimer += Time.deltaTime;

            if (isPlaying)
            {
                if (bgmSource.time >= FadeTime) bgmSource.DOFade(0, fadeDuration).SetEase(Ease.Linear);
                if (bgmSource.time >= StopTime) StopBgmPreview();
                if (!bgmSource.isPlaying) isPlaying = false;
            }
            else
            {
                if (IsLingering) StartBgmPreview();
            }
        }

        public void SetBgmValues(string path, float start, float duration)
        {
            bgmPath = path;
            startTime = start;
            durationTime = duration;
        }

        public void ResetLingerTimer()
        {
            lingerTimer = 0;
        }

        public async void StartBgmPreview()
        {
            if (durationTime <= 0 || bgmSource.isPlaying) return; 

            isPlaying = true;

            if (bgmPath != prevBgmPath)
            {
                prevBgmPath = bgmPath;
                if (!await LoadBgm())
                {
                    isPlaying = false;
                    return;
                }
            }

            bgmSource.volume = 0;
            bgmSource.time = startTime;
            bgmSource.Play();
            bgmSource.DOFade(1, fadeDuration).SetEase(Ease.Linear);
        }

        public void StopBgmPreview()
        {
            if (!bgmSource.isPlaying) return;

            isPlaying = false;
            bgmSource.Stop();
        }

        private async Task<bool> LoadBgm()
        {
            // .ogg makes the game freeze because of decompression.
            // Please just use .wav... I beg you.
            AudioClip bgmClip = await AudioLoader.LoadBgm(bgmPath);

            ChartManager.Instance.bgmClip = bgmClip;
            bgmSource.clip = bgmClip;

            return bgmClip != null;
        }
    }
}
