using SaturnGame.RhythmGame;
using UnityEngine;
using UnityEngine.UI;

public class BgmProgressBar : MonoBehaviour
{
    [SerializeField] private AudioSource bgmPlayer;
    [SerializeField] private TimeManager timeManager;
    [SerializeField] private Image image;
    [SerializeField] private Material material;
    private Material instance;

    void Awake()
    {
        instance = new Material(material);
        image.material = instance;
    }

    void Update()
    {
        if (bgmPlayer.clip == null) return;
        float length = bgmPlayer.clip.length * 1000;
        float time = timeManager.VisualTimeMs;

        float progress = Mathf.InverseLerp(0, length, time);

        instance.SetFloat("_Progress", progress);
    }
}
