using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace SaturnGame.UI
{
public class OptionPanelRadial : MonoBehaviour
{
    [FormerlySerializedAs("rect")] public RectTransform Rect;

    public string Title
    {
        get => title0TMP == null ? "" : title0TMP.text;
        set
        {
            if (title0TMP != null) title0TMP.text = value;
        }
    }

    public void SetRadialPanelColor([NotNull] UIListItem item)
    {
        radialPanel.color = item.Color;
    }

    [SerializeField] private TextMeshProUGUI title0TMP;
    [SerializeField] private Image radialPanel;
}
}