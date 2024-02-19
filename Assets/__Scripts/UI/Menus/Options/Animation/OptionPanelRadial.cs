using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SaturnGame.UI
{
    public class OptionPanelRadial : MonoBehaviour
    {
        public RectTransform rect;
        public string Title
        {
            get => title0TMP == null ? "" : title0TMP.text;
            set
            {
                if (title0TMP != null) title0TMP.text = value;
            }
        }

        public void SetRadialPanelColor(UIListItem item)
        {
            radialPanel.color = item.color;
        }

        [SerializeField] private TextMeshProUGUI title0TMP;
        [SerializeField] private Image radialPanel;
    }
}
