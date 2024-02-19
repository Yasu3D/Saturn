using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
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
            Radial
        }

        public string screenName;
        public UIScreenType screenType;
        public List<UIListItem> listItems;
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

        public SubtitleTypes subtitleType;
        public ItemTypes itemType;

        public Color color;
        public string title;
        public string subtitle;

        public string settingsBinding;

        public UIScreen nextScreen;

        public string settingsParameter;
        public int settingsValue;
    }
}
