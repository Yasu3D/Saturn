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

    private void Start()
    {
        indicators = new SegmentIndicator[60, 8];

        Color color = new(0, 0, 0, segmentOpacity);

        for (int anglePos = 0; anglePos < 60; anglePos++)
        {
            for (int depthPos = 0; depthPos < 8; depthPos++)
            {
                SegmentIndicator indicator = Instantiate(indicatorPrefab, transform);
                indicator.StartRadians = anglePos * 6 * Mathf.Deg2Rad;
                indicator.SizeRadians = 6 * Mathf.Deg2Rad;
                indicator.InnerRadius = InnerRadius + (7 - depthPos) * (OuterRadius - InnerRadius) / 8;
                indicator.OuterRadius = InnerRadius + (8 - depthPos) * (OuterRadius - InnerRadius) / 8;
                indicator.color = color;

                indicators[anglePos, depthPos] = indicator;
            }
        }
    }

    public void UpdateColors(Color32[] colors)
    {
        for (int anglePos = 0; anglePos < 60; anglePos++)
        {
            for (int depthPos = 0; depthPos < 8; depthPos++)
            {
                Color newColor = colors[anglePos * 8 + depthPos];
                newColor.a *= segmentOpacity;
                indicators[anglePos, depthPos].color = newColor;
            }
        }
    }
}
}
