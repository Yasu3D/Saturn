using UnityEngine;

namespace SaturnGame.UI
{
public class RingDebugManager : MonoBehaviour
{
    [SerializeField] private SegmentGrid grid;
    [SerializeField] private SegmentIndicator indicatorPrefab;
    [SerializeField] private SegmentIndicator[,] indicators;
    [SerializeField] private float segmentOpacity;

    private float InnerRadius => grid.OverallInnerRadius;
    private float OuterRadius => grid.OverallOuterRadius;

    private void Awake()
    {
        indicators = new SegmentIndicator[60, 8];

        Color color = new(0, 0, 0, segmentOpacity);

        for (int anglePos = 0; anglePos < 60; anglePos++)
        for (int depthPos = 0; depthPos < 8; depthPos++)
        {
            SegmentIndicator indicator = Instantiate(indicatorPrefab, grid.transform);
            indicator.StartRadians = anglePos * 6 * Mathf.Deg2Rad;
            indicator.SizeRadians = 6 * Mathf.Deg2Rad;
            indicator.InnerRadius = InnerRadius + (7 - depthPos) * (OuterRadius - InnerRadius) / 8;
            indicator.OuterRadius = InnerRadius + (8 - depthPos) * (OuterRadius - InnerRadius) / 8;
            indicator.color = color;

            indicators[anglePos, depthPos] = indicator;
        }
    }

    public void UpdateColors(Color32[,] colors)
    {
        for (int i = 0; i < 8; i++)
        for (int j = 0; j < 60; j++)
        {
            Color newColor = colors[i, j];
            newColor.a *= segmentOpacity;
            indicators[j, i].color = newColor;
        }
    }

    public void ToggleVisibility() => grid.gameObject.SetActive(!grid.gameObject.activeSelf);
}
}
