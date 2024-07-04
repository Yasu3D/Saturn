using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SaturnGame.Settings;
using UnityEngine;
using UnityEngine.Serialization;

namespace SaturnGame.UI
{
[CreateAssetMenu(fileName = "UIScreen", menuName = "UI Logic/UI Screen")]
public class UIScreen : ScriptableObject
{
    public enum UIScreenType
    {
        LinearSimple,
        LinearDetailed,
        Radial,
    }

    public enum RadialScreenSubType
    {
        Text,
        Sprites,
        ChartPreview,
        Offset,
        ConsoleColor,
        NoteColor,
        Volume,
    }

    public string ScreenTitle;

    [TextArea(1, 5)] public string ScreenSubtitle;

    [FormerlySerializedAs("screenType")] public UIScreenType ScreenType;
    public RadialScreenSubType RadialSubType;

    [FormerlySerializedAs("defaultItemIndex")]
    public int DefaultItemIndex;

    [FormerlySerializedAs("listItems")] public List<UIListItem> ListItems;

    public List<UIListItem> VisibleListItems => ListItems.Where(x => x.IsVisible()).ToList();
}

[Serializable]
public class UIListItem
{
    public enum VisibilityTypes
    {
        Always,
        Equals,
    }

    public VisibilityTypes VisibilityType;
    public string ConditionParameter;
    public string ConditionValue; // Note: must be enum

    public enum ItemTypes
    {
        ValueSetter,
        SubMenu,
    }

    [FormerlySerializedAs("itemType")] public ItemTypes ItemType;

    [FormerlySerializedAs("color")] public Color Color;
    [FormerlySerializedAs("title")] public string Title;

    public enum SubtitleTypes
    {
        Static,
        Dynamic, // Only valid for SubMenu
    }

    [FormerlySerializedAs("subtitleType")] public SubtitleTypes SubtitleType;

    // Subtitle is only valid for SubtitleType Static
    [FormerlySerializedAs("subtitle")] public string Subtitle;

    // SettingsBinding is only valid for SubtitleType Dynamic
    [FormerlySerializedAs("settingsBinding")]
    public string SettingsBinding;

    // Sprite, SettingsParameter, and SettingsValue are only valid for ValueSetter
    public Sprite Sprite;

    [FormerlySerializedAs("settingsParameter")]
    public string SettingsParameter;

    public enum ValueType
    {
        Int,
        Enum,
        Float,
    }

    // Default to int since that was the default before multiple types were available.
    // Ideally, this is automatically set by checking the corresponding underlying setting, but that sounds complicated.
    public ValueType SettingsType = ValueType.Int;

    [FormerlySerializedAs("SettingsValue")] [FormerlySerializedAs("settingsValue")]
    public int SettingsValueInt;

    public string SettingsValueEnum;

    public float SettingsValueFloat;
    private const float FloatValueEpsilon = 0.0001f; // In practice, all float settings only use .1 increments.

    // NextScreen is only valid for SubMenu
    [FormerlySerializedAs("nextScreen")] public UIScreen NextScreen;

    public bool IsVisible()
    {
        switch (VisibilityType)
        {
            case VisibilityTypes.Always:
            {
                return true;
            }
            case VisibilityTypes.Equals:
            {
                (Type type, object value) = SettingsManager.Instance.PlayerSettings.GetParameter(ConditionParameter);
                if (type.IsEnum) return value.ToString() == ConditionValue;
                Debug.LogError($"Expected an enum parameter for ConditionParameter, " +
                               $"but got a {type} for {ConditionParameter}");
                return false;
            }
            default:
            {
                throw new ArgumentOutOfRangeException();
            }
        }
    }

    public bool MatchesParameterValue(Type type, object value)
    {
        if (ItemType != ItemTypes.ValueSetter)
            return false;
        return SettingsType switch
        {
            ValueType.Int => type == typeof(int) && (int)value == SettingsValueInt,
            ValueType.Enum => type.IsEnum && (string)value == SettingsValueEnum,
            ValueType.Float => type == typeof(float) && Mathf.Abs((float)value - SettingsValueFloat) < FloatValueEpsilon,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }
}
}
