using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaturnGame.RhythmGame
{
    public class TimeManager : MonoBehaviour
    {
        [SerializeField] private BgmManager bgmManager;

        //public float StaticAudioOffset {get; private set; } = -20;
        public float StaticAudioOffset = -20;
        public float PlaybackSpeed {get; private set; } = 1.0f;

        [Space(20)]
        [SerializeField] private  float VisualTimeScale = 1.0f;
        [SerializeField] private float timeWarpMultiplier = 1.0f;
        [SerializeField] private float forceSyncDiscrepancy = 50f;
        
        public void SetPlaybackSpeed(float speed)
        {
            float clampedSpeed = Mathf.Clamp01(speed);
            PlaybackSpeed = clampedSpeed;
            bgmManager.bgmPlayer.pitch = clampedSpeed;
        }

        /// <summary>
        /// Returns Song Position in ms.
        /// </summary>
        /// <returns></returns>
        public float BgmTime()
        {
            if (bgmManager.bgmPlayer.clip == null)
                return -1;

            return Mathf.Max(0, 1000 * bgmManager.bgmPlayer.time);
        }

        /// <summary>
        /// <b>Use VisualTime! This does NOT include any offsets!</b><br />
        /// Time synchronized with BgmTime, but properly updated every frame for smooth visuals beyond 60fps. 
        /// </summary>
        public float RawVisualTime { get; private set; }
        /// <summary>
        /// <b>Includes sync calibration and user offsets!</b> <br />
        /// Time synchronized with BgmTime, but properly updated every frame for smooth visuals beyond 60fps.
        /// </summary>
        public float VisualTime { get; private set; }
        public void UpdateVisualTime()
        {
            if (!bgmManager.bgmPlayer.isPlaying) return;

            RawVisualTime += Time.deltaTime * VisualTimeScale * 1000;
            VisualTime = RawVisualTime + StaticAudioOffset;
        }

        /// <summary>
        /// Synchronises VisualTime with BgmTime if they drift apart too much. <br />
        /// Huge thanks to AllPoland for sharing this code from ArcViewer with me.
        /// </summary>
        public void ReSync()
        {
            if (!bgmManager.bgmPlayer.isPlaying) return;
        
            float discrepancy = RawVisualTime - BgmTime();
            float absDiscrepancy = Mathf.Abs(discrepancy);

            if (absDiscrepancy >= forceSyncDiscrepancy || PlaybackSpeed == 0)
            {
                // Snap directly to BgmTime if discrepancy gets too high.
                Debug.Log($"Force correcting by {discrepancy}");
                RawVisualTime = BgmTime();
            }

            // Warp VisualTime to re-align with audio
            // * 0.001f because SaturnGame uses milliseconds for all timing.
            float timeWarp = discrepancy * timeWarpMultiplier * 0.001f;
            VisualTimeScale = PlaybackSpeed - timeWarp;
        }

        void Update()
        {
            UpdateVisualTime();
            ReSync();
            
            if (Input.GetKeyDown(KeyCode.P))
            {
                bgmManager.UpdateBgmData(181, TimeSignature.Default);
                bgmManager.bgmPlayer.clip = bgmManager.bgmClip;
                bgmManager.Play();
            }

            if (Input.GetKeyDown(KeyCode.I))
                SetPlaybackSpeed(0.5f);
        }
    }
}
