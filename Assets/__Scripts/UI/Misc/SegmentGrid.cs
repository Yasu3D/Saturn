using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace SaturnGame.UI
{
[RequireComponent(typeof(CanvasRenderer))]
public class SegmentGrid : MaskableGraphic
{
    // https://gist.github.com/yasirkula/d09bbc1e16dc96354b2e7162b351f964

    // Note: we actually draw outside these radii by half an edge width, since the edge centered on the radius.
    public float OverallOuterRadius;
    public float OverallInnerRadius;

    // There will be AngleSplits * DepthSplit segments.
    public int AngleSplits;
    public int DepthSplits;

    public float EdgeThickness;

    public int Detail = 64;

    protected override void OnPopulateMesh([NotNull] VertexHelper vertexHelper)
    {
        Rect r = GetPixelAdjustedRect();

        vertexHelper.Clear();

        Vector2 pivot = rectTransform.pivot;
        float centerWidth = r.width * (0.5f - pivot.x);
        float centerHeight = r.height * (0.5f - pivot.y);

        float deltaRadians = 2 * Mathf.PI / Detail;

        // Draw circles.
        for (int depthIndex = 0; depthIndex <= DepthSplits; depthIndex++)
        {
            float depthRadiusCenter = depthIndex * (OverallOuterRadius - OverallInnerRadius) / DepthSplits +
                                      OverallInnerRadius;

            float outerWidthRadius = depthRadiusCenter * r.width * 0.5f + EdgeThickness * 0.5f;
            float outerHeightRadius = depthRadiusCenter * r.height * 0.5f + EdgeThickness * 0.5f;
            float innerWidthRadius = depthRadiusCenter * r.width * 0.5f - EdgeThickness * 0.5f;
            float innerHeightRadius = depthRadiusCenter * r.height * 0.5f - EdgeThickness * 0.5f;

            int depthVertexIndexOffset = vertexHelper.currentVertCount;

            for (int i = 0; i <= Detail; i++)
            {
                float radians = i * deltaRadians;
                float cos = Mathf.Cos(radians);
                float sin = Mathf.Sin(radians);

                // adds vertex i * 2 (outer)
                vertexHelper.AddVert(
                    new Vector3(cos * outerWidthRadius + centerWidth, sin * outerHeightRadius + centerHeight, 0f),
                    color, Vector2.zero);
                // adds vertex i * 2 + 1 (inner)
                vertexHelper.AddVert(
                    new Vector3(cos * innerWidthRadius + centerWidth, sin * innerHeightRadius + centerHeight, 0f),
                    color, Vector2.zero);

                if (i <= 0) continue;

                // add the quad from i - 1 and i vertices
                AddQuadAtOffset(depthVertexIndexOffset, i * 2 - 2, i * 2 - 1, i * 2, i * 2 + 1, vertexHelper);
            }
        }

        // Draw lines. Note: Doesn't round the edges of the lines, so if the lines are thick enough and detail is high
        // enough, it could look distorted.
        float overallOuterWidthRadius = OverallOuterRadius * r.width * 0.5f;
        float overallOuterHeightRadius = OverallOuterRadius * r.height * 0.5f;
        float overallInnerWidthRadius = OverallInnerRadius * r.width * 0.5f;
        float overallInnerHeightRadius = OverallInnerRadius * r.height * 0.5f;
        for (int angleIndex = 0; angleIndex < AngleSplits; angleIndex++)
        {
            float radians = angleIndex * 2 * Mathf.PI / AngleSplits;

            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);

            Vector2 innerPoint = new(cos * overallInnerWidthRadius + centerWidth,
                sin * overallInnerHeightRadius + centerHeight);
            Vector2 outerPoint = new(cos * overallOuterWidthRadius + centerWidth,
                sin * overallOuterHeightRadius + centerHeight);

            // Get the actual perpendicular normalized vector since we can't assume the actual angle is the same as
            // the radians value after adjusting for width/height.
            Vector2 normalizedVector = (outerPoint - innerPoint).normalized;
            Vector2 edgeThicknessOffset = new(-normalizedVector.y * EdgeThickness * 0.5f,
                normalizedVector.x * EdgeThickness * 0.5f);

            int angleVertexIndexOffset = vertexHelper.currentVertCount;
            vertexHelper.AddVert(innerPoint + edgeThicknessOffset, color, Vector2.zero);
            vertexHelper.AddVert(innerPoint - edgeThicknessOffset, color, Vector2.zero);
            vertexHelper.AddVert(outerPoint + edgeThicknessOffset, color, Vector2.zero);
            vertexHelper.AddVert(outerPoint - edgeThicknessOffset, color, Vector2.zero);

            AddQuadAtOffset(angleVertexIndexOffset, 0, 1, 2, 3, vertexHelper);
        }
    }

    private static void AddQuadAtOffset(int indexOffset, int i1, int i2, int i3, int i4,
        [NotNull] VertexHelper vertexHelper)
    {
        vertexHelper.AddTriangle(i1 + indexOffset, i2 + indexOffset, i3 + indexOffset);
        vertexHelper.AddTriangle(i2 + indexOffset, i3 + indexOffset, i4 + indexOffset);
    }
}
}
