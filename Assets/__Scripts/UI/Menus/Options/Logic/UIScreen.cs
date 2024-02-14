using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SaturnGame.UI
{
    [CreateAssetMenu(fileName = "UIScreen", menuName = "UI Logic/UI Screen")]
    public class UIScreen : ScriptableObject
    {
        public enum UIScreenType
        {
            LinearSimple,
            LinearDetailed,
            Radial
        }

        public string Name;
        public UIScreenType ScreenType;
        public List<UIListItem> ListItems;
    }

    [Serializable]
    public class UIListItem
    {
        public enum ItemTypes
        {
            ValueSetter,
            SubMenu
        }

        public enum SubtitleTypes
        {
            Static,
            Dynamic
        }

        public SubtitleTypes SubtitleType;
        public ItemTypes ItemType;

        public Color Color;
        public string Title;
        public string Subtitle;

        public string SettingsBinding;

        public UIScreen NextScreen;

        public string SettingsParameter;
        public int SettingsValue;
    }
}
