using UnityEngine;

namespace SaturnGame.RhythmGame
{
    public class BgmManager : MonoBehaviour
    {
        [Header("MUSIC INFO")]
        public float BeatDuration;
        public float BeatsPerMinute;
        public TimeSignature TimeSig;

        [Header("BGM")]
        public AudioSource bgmPlayer;
        public AudioClip bgmClip;

        /// <summary>
        /// Start playback in [delay] milliseconds.
        /// </summary>
        /// <param name="delay">delay in ms</param>
        public void PlayScheduled(double delay)
        {
            bgmPlayer.PlayScheduled(AudioSettings.dspTime + delay * 0.001f);
        }

        /// <summary>
        /// Start playback
        /// </summary>
        public void Play()
        {
            bgmPlayer.Play();
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
    }
}
