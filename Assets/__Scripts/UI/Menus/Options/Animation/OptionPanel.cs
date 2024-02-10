using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SaturnGame.UI
{
    public class OptionPanel : MonoBehaviour
    {
        public RectTransform rect;
        public string Title
        {
            get => title0TMP == null ? "" : title0TMP.text;
            set
            {
                if (title0TMP != null) title0TMP.text = value;
                if (title1TMP != null) title1TMP.text = value;
                if (title2TMP != null) title2TMP.text = value;
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
            if (simple != null) simple.SetActive(type is UIScreen.UIScreenType.LinearSimple || (type is UIScreen.UIScreenType.LinearDetailed && detailed == null));
            if (detailed != null) detailed.SetActive(type is UIScreen.UIScreenType.LinearDetailed);
            if (radial != null) radial.SetActive(type is UIScreen.UIScreenType.Radial);
        }

        public void SetRadialPanelColor(UIListItem item)
        {
            radialPanel.color = item.Color;
        }

        [SerializeField] private TextMeshProUGUI title0TMP;
        [SerializeField] private TextMeshProUGUI title1TMP;
        [SerializeField] private TextMeshProUGUI title2TMP;

        [SerializeField] private TextMeshProUGUI subtitleTMP;

        [SerializeField] private GameObject simple;
        [SerializeField] private GameObject detailed;
        [SerializeField] private GameObject radial;

        [SerializeField] private Image radialPanel;
    }
}
