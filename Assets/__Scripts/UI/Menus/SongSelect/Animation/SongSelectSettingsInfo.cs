using System;
using JetBrains.Annotations;
using SaturnGame.Settings;
using UnityEngine;
using TMPro;
using static SaturnGame.Settings.GameSettings.GiveUpOptions;

public class SongSelectSettingsInfo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI maskText;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private TextMeshProUGUI offsetText;
    [SerializeField] private TextMeshProUGUI giveUpText;

    private static GameSettings Settings => SettingsManager.Instance.PlayerSettings.GameSettings;

    [NotNull]
    private string GiveUpText(GameSettings.GiveUpOptions giveUpSetting) => giveUpSetting switch
    {
        Off => "OFF",
        NoTouch => "NO TOUCH",
        SBorder => "S RANK BORDER",
        SSBorder => "SS RANK BORDER",
        SSSBorder => "SSS RANK BORDER",
        MasterBorder => "MASTER RANK BORDER",
        PersonalBestBorder => "PERSONAL BEST",
        _ => throw new ArgumentOutOfRangeException(nameof(giveUpSetting), giveUpSetting, "Unknown GiveUpOptions"),
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
        giveUpText.text = GiveUpText(Settings.GiveUpSetting);
    }
}
