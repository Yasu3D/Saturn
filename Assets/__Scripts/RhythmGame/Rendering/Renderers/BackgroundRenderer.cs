using SaturnGame.Settings;
using UnityEngine;

public class BackgroundRenderer : MonoBehaviour
{
    [SerializeField] private MeshRenderer meshRenderer;
    private static readonly int MaskAddPropertyID = Shader.PropertyToID("_MaskAdd");

    private void Awake()
    {
        GameSettings settings = SettingsManager.Instance.PlayerSettings.GameSettings;
        meshRenderer.material.SetFloat(MaskAddPropertyID, settings.MaskDensity);
    }
}