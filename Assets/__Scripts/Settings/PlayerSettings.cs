using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using SaturnGame.JetBrains.Annotations;
using Tomlyn;
using Tomlyn.Model;
using Tomlyn.Syntax;
using UnityEngine;

namespace SaturnGame.Settings
{
[Serializable]
public class PlayerSettings : SettingsWithTomlMetadata
{
    public GameSettings GameSettings { get; set; } = new();
    public UiSettings UiSettings { get; set; } = new();
    public DesignSettings DesignSettings { get; set; } = new();
    public SoundSettings SoundSettings { get; set; } = new();

    private static readonly string SettingsPath = Path.Join(Application.persistentDataPath, "settings.toml");

    [NotNull]
    public static PlayerSettings Load()
    {
        PlayerSettings loadedSettings = null;

        if (File.Exists(SettingsPath))
        {
            try
            {
                FileInfo info = new(SettingsPath);
                if (info.Length > 1_000_000)
                    // A normal settings file should be around 1KB, not 1MB.
                    // Something is seriously wrong, don't try to parse this.
                    throw new($"Not trying to load {SettingsPath} - size is too large ({info.Length} bytes)");

                string tomlString = File.ReadAllText(SettingsPath);
                loadedSettings = Toml.ToModel<PlayerSettings>(tomlString, SettingsPath);
            } catch (Exception e)
            {
                Debug.LogError($"Failed to load settings from {SettingsPath}: {e}");

                // Move current settings file to a backup if possible, otherwise we will overwrite it.
                long unixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                File.Move(SettingsPath, SettingsPath + $"-{unixTime}.bak");
            }
        }

        if (loadedSettings == null)
        {
            loadedSettings = new();

            // When creating new settings, add a comment and some newlines to the TOML so it looks nicer.
            loadedSettings.GameSettings.SetLeadingTrivia("game_settings", new()
            {
                new(TokenKind.Comment,
                    "# Please don't edit this file while the game is running, your changes may be lost."),
                new(TokenKind.NewLine, "\n"),
                new(TokenKind.NewLine, "\n"),
            });
            loadedSettings.UiSettings.SetLeadingTrivia("ui_settings", new() { new(TokenKind.NewLine, "\n") });
            loadedSettings.DesignSettings.SetLeadingTrivia("design_settings", new() { new(TokenKind.NewLine, "\n") });
            loadedSettings.SoundSettings.SetLeadingTrivia("sound_settings", new() { new(TokenKind.NewLine, "\n") });

            loadedSettings.SaveToFile();
        }

        return loadedSettings;
    }

    private void SaveToFile()
    {
        // Note: Investigated rounding floats when writing:
        // - There is no way to change the internal serialization to string.
        // - TomlModelOptions.ConvertToToml can do arbitrary conversions during serialization, but at the end of the day
        //   the value still needs to be one of the supported primitives. Float and double will both have some
        //   imprecision you will see. String will be quoted. Decimal is not supported.
        // Concluded that there is no nice way to do this in TOML without using e.g. a string value.
        // Not worth the hassle.
        string tomlString = Toml.FromModel(this);
        File.WriteAllText(SettingsPath, tomlString);
    }

    // Given a parameter string, searches the children settings objects for the parameter.
    // Returns a tuple of (corresponding child settings object, propertyInfo of the parameter)
    private (object, PropertyInfo) GetParameterProperty(string parameter)
    {
        object[] objectsToSearch = { GameSettings, UiSettings, DesignSettings, SoundSettings };

        foreach (object settingsObject in objectsToSearch)
        {
            PropertyInfo possibleProperty = settingsObject.GetType().GetProperty(parameter);
            if (possibleProperty != null) return (settingsObject, possibleProperty);
        }

        return (null, null);
    }

    /// <summary>
    /// Set a given settings parameter to the given value.
    /// If the parameter is an enum, the value should be a string.
    /// </summary>
    /// <param name="parameter">The property name of the enum. Should not include the settings object name
    /// (e.g. "NoteSpeed", NOT "GameSettings.NoteSpeed").</param>
    /// <param name="value">The value of the setting. The type should match the parameter type, except for enum
    /// parameters, which should be represented as a string.</param>
    /// <exception cref="ArgumentException">thrown if the parameter doesn't exist, or the value type is wrong</exception>
    public void SetParameter(string parameter, [NotNull] object value)
    {
        (object settingsObject, PropertyInfo possibleProperty) = GetParameterProperty(parameter);

        if (possibleProperty == null)
            throw new ArgumentException($"Parameter {parameter} not found in any settings object");

        if (possibleProperty.PropertyType.IsEnum)
        {
            switch (value)
            {
                case string valueString:
                {
                    object enumValue = Enum.Parse(possibleProperty.PropertyType, valueString);
                    value = enumValue;
                    break;
                }
                default:
                {
                    throw new ArgumentException($"Incorrect type for setting {parameter} - parameter is an enum, " +
                                                $"so expected a string, but got a {value.GetType()}");
                }
            }
        }
        else if (value.GetType() != possibleProperty.PropertyType)
        {
            throw new ArgumentException($"Incorrect type for setting {parameter} - " +
                                        $"expected {possibleProperty.PropertyType}, but got {value.GetType()}");
        }

        possibleProperty.SetValue(settingsObject, value);

        // Any time we change a property, immediately save the TOML file.
        // Warning: no guarantee that the change is saved if someone changes a setting value directly
        // (which shouldn't happen).
        SaveToFile();
    }

    // Get the type and value of a parameter.
    // Enum types will accurately return the underlying type, but the value will be converted to a string.
    public (Type, object) GetParameter(string parameter)
    {
        (object settingsObject, PropertyInfo possibleProperty) = GetParameterProperty(parameter);

        if (possibleProperty == null)
            throw new ArgumentException($"Parameter {parameter} not found in any settings object");

        object value = possibleProperty.GetValue(settingsObject);

        if (possibleProperty.PropertyType.IsEnum)
            value = ((Enum)value).ToString();

        return (possibleProperty.PropertyType, value);
    }
}

// ReSharper disable RedundantDefaultMemberInitializer
[Serializable]
public class GameSettings : SettingsWithTomlMetadata
{
    /// <summary>
    /// Note Speed from 10 [1.0] to 60 [6.0]
    /// </summary>
    public int NoteSpeed { get; set; } = 25;

    public enum OffsetModeOptions
    {
        Standard, // Low Latency
        Classic, // Original Latency
        Advanced,
    }

    public OffsetModeOptions OffsetMode { get; set; } = OffsetModeOptions.Standard;

    /// <summary>
    /// Audio offset in classic mode, from -10 to +10. 8.333~ms per unit (0.833~ms per 0.1 unit).
    /// </summary>
    public float ClassicOffset { get; set; } = 0;

    private const float ClassicOffsetUnitMs = 1000f / 12f; // (half a frame)

    /// <summary>
    /// Audio offset in ms. Ignored in Classic mode.
    /// </summary>
    public int AudioOffsetMs { get; set; } = 0;

    public float CalculatedAudioOffsetMs => OffsetMode switch
    {
        OffsetModeOptions.Standard => AudioOffsetMs,
        OffsetModeOptions.Classic => ClassicOffset * ClassicOffsetUnitMs,
        OffsetModeOptions.Advanced => AudioOffsetMs,
        _ => throw new ArgumentOutOfRangeException(),
    };

    /// <summary>
    /// Visual offset in ms. Only used in Advanced mode.
    /// </summary>
    public int VisualOffsetMs { get; set; } = 0;

    public float CalculatedVisualOffsetMs => OffsetMode switch
    {
        OffsetModeOptions.Standard => 0,
        OffsetModeOptions.Classic => 0,
        OffsetModeOptions.Advanced => VisualOffsetMs,
        _ => throw new ArgumentOutOfRangeException(),
    };

    /// <summary>
    /// Input latency in ms. Only used in Advanced mode.
    /// </summary>
    public int InputLatencyMs { get; set; } = 0;

    public float CalculatedInputLatencyMs => OffsetMode switch
    {
        OffsetModeOptions.Standard => 0,
        OffsetModeOptions.Classic => 30f, // Chosen by testing.
        OffsetModeOptions.Advanced => InputLatencyMs,
        _ => throw new ArgumentOutOfRangeException(),
    };

    /// <summary>
    /// Mask Density from 0 to +4
    /// </summary>
    public int MaskDensity { get; set; } = 2;

    public enum BackgroundVideoOptions
    {
        Ask,
        Off,
        On,
    }

    public BackgroundVideoOptions BackgroundVideoSetting { get; set; } = BackgroundVideoOptions.Ask;

    public enum BonusEffectOptions
    {
        Off,
        On,
    }

    public BonusEffectOptions BonusEffectSetting { get; set; } = BonusEffectOptions.Off;

    public enum MirrorNotesOptions
    {
        Off,
        On,
    }

    public MirrorNotesOptions MirrorNotes { get; set; } = MirrorNotesOptions.Off;

    public enum GiveUpOptions
    {
        Off,
        NoTouch,
        SBorder,
        SsBorder,
        SssBorder,
        MasterBorder,
        PersonalBestBorder,
    }

    public GiveUpOptions GiveUpSetting { get; set; } = GiveUpOptions.Off;
}

[Serializable]
public class UiSettings : SettingsWithTomlMetadata
{
    public enum JudgementDisplayPositions
    {
        Off,
        Top,
        Center,
        Bottom,
    }

    public JudgementDisplayPositions JudgementDisplayPosition { get; set; } = JudgementDisplayPositions.Center;

    public enum ShowJudgementDetailsOptions
    {
        Off,
        On,
    }

    public ShowJudgementDetailsOptions ShowJudgementDetails { get; set; } = ShowJudgementDetailsOptions.On;

    public enum GuideLaneTypes
    {
        Off,
        A,
        B,
        C,
        D,
        E,
        F,
        G,
    }

    public GuideLaneTypes GuideLaneType { get; set; } = GuideLaneTypes.A;

    /// <summary>
    /// 0 >   0%<br/>
    /// 1 >  20%<br/>
    /// 2 >  40%<br/>
    /// 3 >  60%<br/>
    /// 4 >  80%<br/>
    /// 5 > 100%<br/>
    /// </summary>
    public int GuideLaneOpacity { get; set; } = 5;

    /// <summary>
    /// 0 >   0%<br/>
    /// 1 >  20%<br/>
    /// 2 >  40%<br/>
    /// 3 >  60%<br/>
    /// 4 >  80%<br/>
    /// 5 > 100%<br/>
    /// </summary>
    public int DisplayOpacity { get; set; } = 5;

    public enum ShowBarLinesOptions
    {
        Off,
        On,
    }

    public ShowBarLinesOptions ShowBarLines { get; set; } = ShowBarLinesOptions.On;

    public enum CenterDisplayInfoOptions
    {
        Off,
        Combo,
        PlusMethod,
        MinusMethod,
        AverageMethod,
        SBorder,
        SsBorder,
        SssBorder,
        PersonalBestBorder,
    }

    public CenterDisplayInfoOptions CenterDisplayInfo { get; set; } = CenterDisplayInfoOptions.Combo;

    public enum ScoreDisplayMethods
    {
        PlusMethod,
        MinusMethod,
        AverageMethod,
    }

    public ScoreDisplayMethods ScoreDisplayMethod { get; set; } = ScoreDisplayMethods.PlusMethod;
}

[Serializable]
public class DesignSettings : SettingsWithTomlMetadata
{
    /// <summary>
    /// WIP
    /// </summary>
    public int RingColor { get; set; } = 0;

    public enum JudgeLineColors
    {
        Original,
        Lily,
        Reverse,
    }

    public JudgeLineColors JudgeLineColor { get; set; } = JudgeLineColors.Reverse;

    /// <summary>
    /// 1 > 1
    /// 2 > 2
    /// 3 > 3
    /// 4 > 4
    /// 5 > 5
    /// </summary>
    public int NoteWidth { get; set; } = 3;

    // TODO: Color enums
    [DataMember(Name="note_color_id_touch")] public int NoteColorIDTouch { get; set; } = 0;
    [DataMember(Name="note_color_id_chain")] public int NoteColorIDChain { get; set; } = 1;
    [DataMember(Name="note_color_id_swipe_clockwise")] public int NoteColorIDSwipeClockwise { get; set; } = 2;
    [DataMember(Name="note_color_id_swipe_counterclockwise")] public int NoteColorIDSwipeCounterclockwise { get; set; } = 3;
    [DataMember(Name="note_color_id_snap_forward")] public int NoteColorIDSnapForward { get; set; } = 4;
    [DataMember(Name="note_color_id_snap_backward")] public int NoteColorIDSnapBackward { get; set; } = 5;
    [DataMember(Name="note_color_id_hold")] public int NoteColorIDHold { get; set; } = 6;

    public enum InvertSlideColorOptions
    {
        Off,
        On,
    }

    public InvertSlideColorOptions InvertSlideColor { get; set; } = InvertSlideColorOptions.Off;

    public enum ShowShootEffectOptions
    {
        Off,
        On,
    }

    public ShowShootEffectOptions ShowShootEffect { get; set; } = ShowShootEffectOptions.On;

    public enum ShowKeyBeamsOptions
    {
        Off,
        On,
    }

    public ShowKeyBeamsOptions ShowKeyBeams { get; set; } = ShowKeyBeamsOptions.On;

    public enum ShowRNoteEffectOptions
    {
        Off,
        On,
    }

    public ShowRNoteEffectOptions ShowRNoteEffect { get; set; } = ShowRNoteEffectOptions.On;
}

[Serializable]
public class SoundSettings : SettingsWithTomlMetadata
{
    [DataMember(Name="touch_se")] public int TouchSE { get; set; } = 0;
    public int BgmVolume { get; set; } = 100;
    public int HitsoundOverallVolume { get; set; } = 70;
    public int GuideVolume { get; set; } = 30;
    public int TouchNoteVolume { get; set; } = 80;
    public int HoldNoteVolume { get; set; } = 80;
    public int SlideNoteVolume { get; set; } = 80;
    public int SnapNoteVolume { get; set; } = 80;
    public int ChainNoteVolume { get; set; } = 80;
    public int BonusEffectVolume { get; set; } = 80;
    [DataMember(Name="r_note_effect_volume")] public int RNoteEffectVolume { get; set; } = 80;
}
// ReSharper restore RedundantDefaultMemberInitializer

public abstract class SettingsWithTomlMetadata : ITomlMetadataProvider {
    // TOML metadata information (comments, etc.) needed for ITomlMetadataProvider
    //TomlPropertiesMetadata ITomlMetadataProvider.PropertiesMetadata { get; set; }
    TomlPropertiesMetadata ITomlMetadataProvider.PropertiesMetadata { get; set; }

    public void SetLeadingTrivia(string propertyName, List<TomlSyntaxTriviaMetadata> trivia)
    {
        ITomlMetadataProvider metadataProvider = this;
        metadataProvider.PropertiesMetadata ??= new();

        metadataProvider.PropertiesMetadata.TryGetProperty(propertyName, out TomlPropertyMetadata metadata);
        metadata ??= new();

        if (metadata.LeadingTrivia != null)
            metadata.LeadingTrivia.AddRange(trivia);
        else
            metadata.LeadingTrivia = trivia;

        metadataProvider.PropertiesMetadata.SetProperty(propertyName, metadata);
    }

    public void DebugTrivia([NotNull] string propertyName)
    {
        TomlPropertiesMetadata propertiesMetadata = ((ITomlMetadataProvider)this).PropertiesMetadata;
        if (propertiesMetadata == null)
        {
            Debug.Log($"{propertyName}: No properties metadata");
            return;
        }

        propertiesMetadata.TryGetProperty(propertyName, out TomlPropertyMetadata metadata);

        if (metadata == null)
        {
            Debug.Log($"{propertyName}: No metadata");
            return;
        }

        Debug.Log($"{propertyName}: {metadata.LeadingTrivia?.Count} leading trivia");
        foreach (TomlSyntaxTriviaMetadata trivia in metadata.LeadingTrivia ?? new List<TomlSyntaxTriviaMetadata>())
            Debug.Log(trivia);

        Debug.Log($"{propertyName}: {metadata.TrailingTriviaAfterEndOfLine?.Count} trailing trivia");
        foreach (TomlSyntaxTriviaMetadata trivia in metadata.TrailingTriviaAfterEndOfLine ?? new List<TomlSyntaxTriviaMetadata>())
            Debug.Log(trivia);

    }
}
}
