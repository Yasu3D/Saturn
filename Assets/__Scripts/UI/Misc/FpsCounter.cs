using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace SaturnGame {
[RequireComponent(typeof(TMP_Text))]
public class FpsCounter : MonoBehaviour
{
    public float FpsMeasurePeriodSec = 0.5f;
    [SerializeField] private TMP_Text text;

    private int frameCount;
    private float fpsNextPeriodStartSec;

    private void Start() {
        fpsNextPeriodStartSec = Time.realtimeSinceStartup + FpsMeasurePeriodSec;
    }

    private void Update() {
        // measure average frames per second
        frameCount++;
        if (!(Time.realtimeSinceStartup > fpsNextPeriodStartSec)) return;

        text.text = Mathf.RoundToInt(frameCount / FpsMeasurePeriodSec).ToString();
        frameCount = 0;
        fpsNextPeriodStartSec += FpsMeasurePeriodSec;
    }
}
}
