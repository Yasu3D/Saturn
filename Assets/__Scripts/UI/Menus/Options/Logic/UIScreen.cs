using System;
using System.Collections.Generic;
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
    
    [TextArea(1, 5)]
    public string ScreenSubtitle;
    
    [FormerlySerializedAs("screenType")] public UIScreenType ScreenType;
    public RadialScreenSubType RadialSubType;

    [FormerlySerializedAs("defaultItemIndex")]
    public int DefaultItemIndex;

    [FormerlySerializedAs("listItems")] public List<UIListItem> ListItems;
}

[Serializable]
public class UIListItem
{
    public enum VisibilityTypes
    {
        Always,
        Equals,
    }
        
    public enum ItemTypes
    {
        ValueSetter,
        SubMenu,
    }

    public enum SubtitleTypes
    {
        Static,
        Dynamic,
    }

    public VisibilityTypes VisibilityType;
    public string ConditionParameter;
    public int ConditionValue;
    
    [FormerlySerializedAs("subtitleType")] public SubtitleTypes SubtitleType;
    [FormerlySerializedAs("itemType")] public ItemTypes ItemType;

    [FormerlySerializedAs("color")] public Color Color;
    [FormerlySerializedAs("title")] public string Title;
    [FormerlySerializedAs("subtitle")] public string Subtitle;
    public Sprite Sprite;

    [FormerlySerializedAs("settingsBinding")]
    public string SettingsBinding;

    [FormerlySerializedAs("nextScreen")] public UIScreen NextScreen;

    [FormerlySerializedAs("settingsParameter")]
    public string SettingsParameter;

    [FormerlySerializedAs("settingsValue")]
    public int SettingsValue;
}
}