using TMPro;
using UnityEngine;
using SaturnGame.Settings;

public class ScreenPosToText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;

    private void Update()
    {
        text.text = SettingsManager.Instance.DeviceSettings.DisplaySettings.ViewRectPosition.ToString();
    }
}