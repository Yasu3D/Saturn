using System.Collections;
using System.Collections.Generic;
using SaturnGame.Settings;
using UnityEngine;

public class BackgroundRenderer : MonoBehaviour
{
    [SerializeField] private MeshRenderer meshRenderer;
    void Awake()
    {
        GameSettings settings = SettingsManager.Instance.PlayerSettings.GameSettings;
        meshRenderer.material.SetFloat("_MaskAdd", settings.MaskDensity);
    }
}
