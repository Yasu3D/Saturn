using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SaturnGame.UI
{
    public class OptionPanelPrimary : MonoBehaviour
    {
        public string Title
        {
            get => title0TMP == null ? "" : title0TMP.text;
            set
            {
                if (title0TMP != null) title0TMP.text = value;
                if (title1TMP != null) title1TMP.text = value;
            }
        }

        public string Subtitle
        {
            get => subtitleTMP == null ? "" : subtitleTMP.text;
            set
            {
                if (subtitleTMP != null) subtitleTMP.text = value;
            }
        }

        public void SetType(UIScreen.UIScreenType type)
        {
            if (linear != null) linear.SetActive(type is UIScreen.UIScreenType.LinearSimple or UIScreen.UIScreenType.LinearDetailed);
            if (radial != null) radial.SetActive(type is UIScreen.UIScreenType.Radial);
        }

        public void SetRadialPanelColor(UIListItem item)
        {
            radialPanel.color = item.color;
        }

        [SerializeField] private TextMeshProUGUI title0TMP;
        [SerializeField] private TextMeshProUGUI title1TMP;

        [SerializeField] private TextMeshProUGUI subtitleTMP;

        [SerializeField] private GameObject linear;
        [SerializeField] private GameObject radial;

        [SerializeField] private Image radialPanel;
    }
}
