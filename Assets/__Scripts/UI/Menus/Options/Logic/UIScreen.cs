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

        [FormerlySerializedAs("screenName")] public string ScreenName;
        [FormerlySerializedAs("screenType")] public UIScreenType ScreenType;
        [FormerlySerializedAs("defaultItemIndex")] public int DefaultItemIndex;
        [FormerlySerializedAs("listItems")] public List<UIListItem> ListItems;
    }

    [Serializable]
    public class UIListItem
    {
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

        [FormerlySerializedAs("subtitleType")] public SubtitleTypes SubtitleType;
        [FormerlySerializedAs("itemType")] public ItemTypes ItemType;

        [FormerlySerializedAs("color")] public Color Color;
        [FormerlySerializedAs("title")] public string Title;
        [FormerlySerializedAs("subtitle")] public string Subtitle;

        [FormerlySerializedAs("settingsBinding")] public string SettingsBinding;

        [FormerlySerializedAs("nextScreen")] public UIScreen NextScreen;

        [FormerlySerializedAs("settingsParameter")] public string SettingsParameter;
        [FormerlySerializedAs("settingsValue")] public int SettingsValue;
    }
}
