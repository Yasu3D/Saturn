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
}
