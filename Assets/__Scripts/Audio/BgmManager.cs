using UnityEngine;

namespace SaturnGame.RhythmGame
{
    [System.Serializable] public class TimeSignature
    {
        public TimeSignature(int upper, int lower)
        {
            Upper = upper;
            Lower = lower;
            Ratio = (float) upper / (float) lower;
        }

        public int Upper = 4;
        public int Lower = 4;
        public float Ratio = 1;
    }

    public class BgmManager : MonoBehaviour
    {
        // ==== Music Info ====
        [Header("MUSIC INFO")]
        public float BeatDuration;
        public float BeatsPerMinute;
        public TimeSignature TimeSig;

        // ==== BGM ====
        [Header("BGM")]
        public AudioSource bgmPlayer;
        public AudioClip bgmClip;

        // ==== Timing ====
        [Header("TIMING")]
        public float StaticAudioOffset = -60;
        public float UserAudioOffset = 0;
        [Space(10)]
        [SerializeField] private float timeWarpMultiplier = 1.0f;
        [SerializeField] private float forceSyncDiscrepancy = 50f;
        public float VisualTimeScale { get; private set; } = 1.0f;

        /// <summary>
        /// Returns Song Position in ms.
        /// </summary>
        /// <returns></returns>
        public float BgmTime()
        {
            if (bgmPlayer.clip == null) return -1;
            return Mathf.Max(0, 1000 * bgmPlayer.time);
        }

        public float VisualTime { get; private set; }
        public void UpdateVisualTime()
        {
            if (bgmPlayer.isPlaying)
            {
                VisualTime += Time.deltaTime * VisualTimeScale * 1000;
            }
        }

        /// <summary>
        /// Synchronises VisualTime with BgmTime if they drift apart too much. <br />
        /// Huge thanks to AllPoland for sharing this code from ArcViewer with me.
        /// </summary>
        public void ReSync()
        {
            if (!bgmPlayer.isPlaying) return;
        
            float discrepancy = VisualTime - BgmTime();
            float absDiscrepancy = Mathf.Abs(discrepancy);

            if (absDiscrepancy >= forceSyncDiscrepancy || bgmPlayer.pitch == 0)
            {
                // Snap directly to BgmTime if discrepancy gets too high.
                Debug.Log($"Force correcting by {discrepancy}");
                VisualTime = BgmTime();
            }

            // Warp VisualTime to re-align with audio
            // * 0.001f because SaturnGame uses milliseconds for all timing.
            float timeWarp = discrepancy * timeWarpMultiplier * 0.001f;
            VisualTimeScale = bgmPlayer.pitch - timeWarp;
        }

        /// <summary>
        /// Updates current BGM's Beats Per Minute, Time Signature <br />
        /// and recalculates a new BeatDuration from those values.
        /// </summary>
        /// <param name="bpm">New Beats Per Minute</param>
        /// <param name="sig">New Time Signature</param>
        public void UpdateBgmData(float bpm, TimeSignature sig)
        {
            BeatsPerMinute = bpm;
            TimeSig = sig;
            BeatDuration = 60 / BeatsPerMinute * TimeSig.Ratio * 1000;
        }

        /// <summary>
        /// Updates current BGM's Beats Per Minute <br />
        /// and recalculates a new BeatDuration from those values.
        /// </summary>
        /// <param name="bpm">New Beats Per Minute</param>
        public void UpdateBgmData(float bpm)
        {
            BeatsPerMinute = bpm;
            BeatDuration = 60 / BeatsPerMinute * TimeSig.Ratio * 1000;
        }

        /// <summary>
        /// Updates current BGM's Time Signature and<br />
        /// recalculates a new BeatDuration from those values.
        /// </summary>
        /// <param name="sig">New Time Signature</param>
        public void UpdateBgmData(TimeSignature sig)
        {
            TimeSig = sig;
            BeatDuration = 60 / BeatsPerMinute * TimeSig.Ratio * 1000;
        }

        void Update()
        {
            UpdateVisualTime();
            ReSync();
        }
    }
}
