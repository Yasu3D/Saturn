using SaturnGame.Settings;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundRenderer : MonoBehaviour
{
    [SerializeField] private RawImage image;
    private readonly float[] densityList = {1, 0.75f, 0.5f, 0.25f, 0.1f};
    
    private void Awake()
    {
        SetMaskDensity();
    }

    public void SetMaskDensity()
    {
        float density = densityList[SettingsManager.Instance.PlayerSettings.GameSettings.MaskDensity];
        image.color = new(density, density, density, 1);
    }
}