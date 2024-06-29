using SaturnGame.Settings;
using UnityEngine;
using TMPro;

public class SongSelectSettingsInfo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI maskText;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private TextMeshProUGUI offsetText;
    [SerializeField] private TextMeshProUGUI giveUpText;

    private static GameSettings Settings => SettingsManager.Instance.PlayerSettings.GameSettings;

    private readonly string[] giveUpSettings =
    {
        "OFF", "NO TOUCH", "S RANK BORDER", "SS RANK BORDER", "SSS RANK BORDER", "MASTER RANK BORDER", "PERSONAL BEST",
    };

    private void Start()
    {
        SetInfo();
    }

    private void SetInfo()
    {
        maskText.text = Settings.MaskDensity == 0 ? "NO MASK" : $"MASK +{Settings.MaskDensity}";
        speedText.text = (Settings.NoteSpeed * 0.1f).ToString("0.0");
        offsetText.text = (Settings.AudioOffset * 0.1f).ToString("0.0");
        // clamp just in case
        giveUpText.text = giveUpSettings[Mathf.Clamp(Settings.GiveUpSetting, 0, giveUpSettings.Length - 1)];
    }
}