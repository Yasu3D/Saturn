using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace SaturnGame.UI
{
    public class OptionPanelLinear : MonoBehaviour
    {
        [FormerlySerializedAs("rect")] public RectTransform Rect;
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
            if (simple != null) simple.SetActive(type is UIScreen.UIScreenType.LinearSimple || (type is UIScreen.UIScreenType.LinearDetailed && detailed == null));
            if (detailed != null) detailed.SetActive(type is UIScreen.UIScreenType.LinearDetailed);
        }

        [SerializeField] private TextMeshProUGUI title0TMP;
        [SerializeField] private TextMeshProUGUI title1TMP;

        [SerializeField] private TextMeshProUGUI subtitleTMP;

        [SerializeField] private GameObject simple;
        [SerializeField] private GameObject detailed;
    }
}
