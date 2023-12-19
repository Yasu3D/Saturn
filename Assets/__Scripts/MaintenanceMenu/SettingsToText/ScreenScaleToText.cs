using TMPro;
using UnityEngine;
using SaturnGame.Settings;

public class ScreenScaleToText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    void Update()
    {
        text.text = SettingsManager.Instance.DeviceSettings.DisplaySettings.ViewRectScale.ToString();
    }
}