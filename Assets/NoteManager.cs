using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SaturnGame.Rendering;
using UnityEngine;

namespace SaturnGame.RhythmGame
{
    public class NoteManager : MonoBehaviour
    {
        [Header("MANAGERS")]
        [SerializeField] private Chart chart;
        [SerializeField] private TimeManager timeManager;
        [SerializeField] private BgmManager bgmManager;

        [Header("POOLS")]
        [SerializeField] private BarLineObjectPool barLinePool;
        [SerializeField] private HoldEndObjectPool holdEndPool;
        [SerializeField] private HoldSurfaceObjectPool holdSurfacePool;
        [SerializeField] private NoteObjectPool notePool;
        [SerializeField] private R_EffectObjectPool r_EffectPool;
        [SerializeField] private SnapObjectPool snapPool;
        [SerializeField] private SwipeObjectPool swipePool;
        [SerializeField] private SyncObjectPool syncPool;
        [Header("RENDERERS")]
        [SerializeField] private GuideLaneRenderer guideLaneRenderer;

        private int maskIndex = 0;
        private void ProcessMasks()
        {
            if (maskIndex > chart.masks.Count - 1) return;

            while (timeManager.VisualTime >= chart.masks[maskIndex].Time)
            {
                guideLaneRenderer.SetMask(chart.masks[maskIndex]);
                maskIndex++;
            }
        }

        private float beatTime;
        [SerializeField] private GameObject testObject;
        private bool toggle;
        void Update()
        {
            ProcessMasks();

            if (timeManager.VisualTime >= beatTime)
            {
                beatTime += bgmManager.BeatDuration;
                toggle = !toggle;
                testObject.SetActive(toggle);
            }
        }
    }
}
