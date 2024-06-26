using System;
using System.Reflection;
using SaturnGame.JetBrains.Annotations;
using UnityEngine;

namespace SaturnGame.Settings
{
[Serializable]
public class PlayerSettings
{
    public GameSettings GameSettings = new();
    public UISettings UISettings = new();
    public DesignSettings DesignSettings = new();
    public SoundSettings SoundSettings = new();

    // Given a parameter string, searches the children settings objects for the parameter.
    // Returns a tuple of (corresponding child settings object, FieldInfo of the parameter)
    private (object, FieldInfo) GetParameterField(string parameter)
    {
        object[] objectsToSearch = { GameSettings, UISettings, DesignSettings, SoundSettings };

        foreach (object settingsObject in objectsToSearch)
        {
            FieldInfo possibleField = settingsObject.GetType().GetField(parameter);
            if (possibleField != null) return (settingsObject, possibleField);
        }

        return (null, null);
    }

    /// <summary>
    /// Set a given settings parameter to the given value.
    /// If the parameter is an enum, the value should be a string.
    /// </summary>
    /// <param name="parameter">The field name of the enum. Should not include the settings object name
    /// (e.g. "NoteSpeed", NOT "GameSettings.NoteSpeed").</param>
    /// <param name="value">The value of the setting. The type should match the parameter type, except for enum
    /// parameters, which should be represented as a string.</param>
    /// <exception cref="ArgumentException">thrown if the parameter doesn't exist, or the value type is wrong</exception>
    public void SetParameter(string parameter, [NotNull] object value)
    {
        (object settingsObject, FieldInfo possibleField) = GetParameterField(parameter);

        if (possibleField == null)
            throw new ArgumentException($"Parameter {parameter} not found in any settings object");

        if (possibleField.FieldType.IsEnum)
        {
            switch (value)
            {
                case string valueString:
                {
                    object enumValue = Enum.Parse(possibleField.FieldType, valueString);
                    possibleField.SetValue(settingsObject, enumValue);
                    return;
                }
                default:
                {
                    throw new ArgumentException($"Incorrect type for setting {parameter} - parameter is an enum, " +
                                                $"so expected a string, but got a {value.GetType()}");
                }
            }
        }

        if (value.GetType() != possibleField.FieldType)
        {
            throw new ArgumentException($"Incorrect type for setting {parameter} - " +
                                        $"expected {possibleField.FieldType}, but got {value.GetType()}");
        }

        possibleField.SetValue(settingsObject, value);
    }

    // Get the type and value of a parameter.
    // Enum types will accurately return the underlying type, but the value will be converted to a string.
    public (Type, object) GetParameter(string parameter)
    {
        (object settingsObject, FieldInfo possibleField) = GetParameterField(parameter);

        if (possibleField == null)
            throw new ArgumentException($"Parameter {parameter} not found in any settings object");

        object value = possibleField.GetValue(settingsObject);

        if (possibleField.FieldType.IsEnum) value = ((Enum)value).ToString();

        return (possibleField.FieldType, value);
    }
}

// ReSharper disable RedundantDefaultMemberInitializer
[Serializable]
public class GameSettings
{
    /// <summary>
    /// Note Speed from 10 [1.0] to 60 [6.0]
    /// </summary>
    public int NoteSpeed = 25;

    public enum OffsetModeOptions
    {
        Standard, // Low Latency
        Classic, // Original Latency
        Advanced,
    }

    public OffsetModeOptions OffsetMode = OffsetModeOptions.Standard;

    /// <summary>
    /// Audio Offset from +100 [10] to -100 [-10]
    /// TODO: Expand to +200 -200
    /// </summary>
    public int AudioOffset = 0;

    /// <summary>
    /// Visual Offset from +100 [10] to -100 [-10]
    /// TODO: Expand to +200 -200
    /// </summary>
    public int VisualOffset = 0;

    /// <summary>
    /// Input latency. No clue about range yet.
    /// TODO: @cg505 define range for this.
    /// </summary>
    public int InputLatency = 0;

    /// <summary>
    /// Mask Density from 0 to +4
    /// </summary>
    public int MaskDensity = 2;

    /// <summary>
    /// 0 > Ask<br/>
    /// 1 > Off<br/>
    /// 2 > On<br/>
    /// </summary>
    public int BackgroundVideoSetting = 0;

    /// <summary>
    /// 0 > Off<br/>
    /// 1 > On<br/>
    /// </summary>
    public int BonusEffectSetting = 1;

    /// <summary>
    /// 0 > Off<br/>
    /// 1 > On<br/>
    /// </summary>
    public int MirrorNotes = 0;

    /// <summary>
    /// 0 > Off<br/>
    /// 1 > No Touch<br/>
    /// 2 > S Border<br/>
    /// 3 > SS Border<br/>
    /// 4 > SSS Border<br/>
    /// 5 > Personal Best Border<br/>
    /// </summary>
    public int GiveUpSetting = 0;
}

[Serializable]
public class UISettings
{
    /// <summary>
    /// 0 > Off<br/>
    /// 1 > Top<br/>
    /// 2 > Center<br/>
    /// 3 > Bottom<br/>
    /// </summary>
    public int JudgementDisplayPosition = 2;

    /// <summary>
    /// 0 > Off<br/>
    /// 1 > On<br/>
    /// </summary>
    public int ShowJudgementDetails = 1;

    /// <summary>
    /// 0 > Off<br/>
    /// 1 > A<br/>
    /// 2 > B<br/>
    /// 3 > C<br/>
    /// 4 > D<br/>
    /// 5 > E<br/>
    /// 6 > F<br/>
    /// 7 > G<br/>
    /// </summary>
    public int GuideLaneType = 1;

    /// <summary>
    /// 0 >   0%<br/>
    /// 1 >  20%<br/>
    /// 2 >  40%<br/>
    /// 3 >  60%<br/>
    /// 4 >  80%<br/>
    /// 5 > 100%<br/>
    /// </summary>
    public int GuideLaneOpacity = 5;

    /// <summary>
    /// 0 >   0%<br/>
    /// 1 >  20%<br/>
    /// 2 >  40%<br/>
    /// 3 >  60%<br/>
    /// 4 >  80%<br/>
    /// 5 > 100%<br/>
    /// </summary>
    public int DisplayOpacity = 5;

    /// <summary>
    /// 0 > Off<br/>
    /// 1 > On<br/>
    /// </summary>
    public int ShowBarLines = 1;

    /// <summary>
    /// 0 > Off<br/>
    /// 1 > Combo<br/>
    /// 2 > Plus Method<br/>
    /// 3 > Minus Method<br/>
    /// 4 > Average Method<br/>
    /// 5 > S Border<br/>
    /// 6 > SS Border<br/>
    /// 7 > SSS Border<br/>
    /// 8 > Personal Best Border<br/>
    /// </summary>
    public int CenterDisplayInfo = 1;

    /// <summary>
    /// 0 > Plus Method<br/>
    /// 1 > Minus Method<br/>
    /// 2 > Average Method<br/>
    /// </summary>
    public int ScoreDisplayMethod = 0;
}

[Serializable]
public class DesignSettings
{
    /// <summary>
    /// WIP
    /// </summary>
    public int RingColor = 0;

    /// <summary>
    /// 0 > Original<br/>
    /// 1 > Lily<br/>
    /// 2 > Reverse<br/>
    /// </summary>
    public int JudgeLineColor = 2;

    /// <summary>
    /// 1 > 1
    /// 2 > 2
    /// 3 > 3
    /// 4 > 4
    /// 5 > 5
    /// </summary>
    public int NoteWidth = 3;


    public int NoteColorIDTouch = 0;
    public int NoteColorIDChain = 1;
    public int NoteColorIDSwipeClockwise = 2;
    public int NoteColorIDSwipeCounterclockwise = 3;
    public int NoteColorIDSnapForward = 4;
    public int NoteColorIDSnapBackward = 5;
    public int NoteColorIDHold = 6;

    /// <summary>
    /// 0 > OFF
    /// 1 > ON
    /// </summary>
    public int InvertSlideColor = 0;

    /// <summary>
    /// WIP
    /// </summary>
    public int TouchEffect = 1;

    /// <summary>
    /// 0 > OFF
    /// 1 > ON
    /// </summary>
    public int ShowShootEffect = 1;

    /// <summary>
    /// 0 > OFF
    /// 1 > ON
    /// </summary>
    public int ShowKeyBeams = 1;

    /// <summary>
    /// 0 > OFF
    /// 1 > ON
    /// </summary>
    public int ShowRNoteEffect = 1;
}

[Serializable]
public class SoundSettings
{
    public int TouchSE = 0;
    public int BGMVolume = 100;
    public int HitsoundOverallVolume = 70;
    public int GuideVolume = 30;
    public int TouchNoteVolume = 80;
    public int HoldNoteVolume = 80;
    public int SlideNoteVolume = 80;
    public int SnapNoteVolume = 80;
    public int ChainNoteVolume = 80;
    public int BonusEffectVolume = 80;
    public int RNoteEffectVolume = 80;
}
// ReSharper restore RedundantDefaultMemberInitializer
}
