using System;
using System.Collections.Generic;
using UnityEngine;

namespace SaturnGame.UI
{
    [CreateAssetMenu(fileName = "UIScreen", menuName = "UI Logic/UI Screen")]
    public class UIScreen : ScriptableObject
    {
        public string Name;
        public List<UIListItem> ListItems;
    }

    [Serializable]
    public class UIListItem
    {
        public string Title;
        public string Subtitle;
        public UIScreen NextScreen;
    }
}
