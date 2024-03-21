using System.Collections;
using System.Collections.Generic;
using SaturnGame.RhythmGame;
using UnityEngine;

public class DebugChartPlayer : MonoBehaviour
{
    [SerializeField] private string path = "SongPacks/DONOTSHIP/";
    [SerializeField] private AudioClip bgm;
    [SerializeField] private AudioSource bgmPlayer;

    // i know update shouldn't be async but this is for temporary testing so i dont care
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            ChartManager.Instance.BGMClip = bgm;
            bgmPlayer.clip = bgm;
            ChartManager.Instance.LoadChart(System.IO.Path.Combine(Application.streamingAssetsPath, path));
        }
    }
}
