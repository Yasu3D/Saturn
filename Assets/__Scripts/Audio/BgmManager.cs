using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using Unity.VisualScripting;
using System;

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
        public float staticAudioOffset = -60;
        public float userAudioOffset = 0;
        [Space(10)]
        [SerializeField] private float timeWarpMultiplier = 1.0f;
        [SerializeField] private float forceSyncDiscrepancy = 50f;
        public float visualTimeScale { get; private set; } = 1.0f;

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
                VisualTime += Time.deltaTime * visualTimeScale * 1000;
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
            visualTimeScale = bgmPlayer.pitch - timeWarp;
        }

        /// <summary>
        /// Updates current BGM's Beats Per Minute, Time Signature <br />
        /// and recalculates new BeatDuration from those values.
        /// </summary>
        /// <param name="bpm">New Beats Per Minute</param>
        /// <param name="sig">New Time Signature</param>
        public void UpdateBgmData(float bpm = -1, TimeSignature sig = null)
        {
            if (bpm > 0)
                BeatsPerMinute = bpm;

            if (sig != null)
                TimeSig = sig;

            BeatDuration = 60 / BeatsPerMinute * TimeSig.Ratio * 1000;
        }




        // EVERYTHING PAST HERE IS DEBUG AND SHOULD BE DELETED ON SIGHT
        void Start()
        {
            UpdateBgmData(BeatsPerMinute, TimeSig);

            bgmPlayer.clip = bgmClip;

            testNote.position = new Vector3 (0, 0, -6);
            testNote.localScale = Vector3.zero;
        }

        [Header("DEBUG")]
        [SerializeField] private TMPro.TextMeshProUGUI text;
        [SerializeField] private GameObject testObject;
        [SerializeField] private Transform testNote;
        private bool toggle;
        private float lastBeat;
        void Update()
        {
            UpdateVisualTime();
            ReSync();

            if (Input.GetKeyDown(KeyCode.P))
            {
                bgmPlayer.Play();
            }

            text.text = $"BgmTime {BgmTime()} \n VisualTime {VisualTime}";

            if (VisualTime > lastBeat + BeatDuration)
            {
                testObject.SetActive(toggle);
                toggle = !toggle;
                lastBeat += BeatDuration;
            }

            float scrollTime = VisualTime % (BeatDuration * 4) / (BeatDuration * 4);

            testNote.position = new Vector3 (0, 0, Mathf.Lerp(-6, 0, scrollTime));
            testNote.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, scrollTime);
        }
    }
}
