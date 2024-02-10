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
            Text,
            Binding
        }

        public SubtitleTypes SubtitleType;
        public string Title;
        public string Subtitle;
        public string Binding;

        public Color Color;
        public ItemTypes ItemType;

        // These should only be visible when Type is set to SubMenu.
        public UIScreen NextScreen;

        // These should only be visible when Type is set to ValueSetter.
        public string Paramter;
        public int Value;
    }
}
