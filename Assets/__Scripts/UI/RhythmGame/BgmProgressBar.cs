using SaturnGame.RhythmGame;
using UnityEngine;
using UnityEngine.UI;

public class BgmProgressBar : MonoBehaviour
{
    
    [SerializeField] private Image image;
    [SerializeField] private Material material;
    [SerializeField] private TimeManager timeManager;
    [SerializeField] private ChartManager chartManager;
    private Material instance;
    private static readonly int ProgressPropertyID = Shader.PropertyToID("_Progress");

    private void Awake()
    {
        instance = new(material);
        image.material = instance;
    }

    private void Update()
    {
        if (chartManager.Chart.EndOfChart == null || chartManager.Chart == null) return;
        float length = chartManager.Chart.EndOfChart.TimeMs;
        float time = timeManager.VisualTimeMs;

        float progress = Mathf.InverseLerp(0, length, time);

        instance.SetFloat(ProgressPropertyID, progress);
    }
}