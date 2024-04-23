using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace SaturnGame.UI
{
[RequireComponent(typeof(CanvasRenderer))]
public class SegmentIndicator : MaskableGraphic
{
    // reference: https://gist.github.com/yasirkula/d09bbc1e16dc96354b2e7162b351f964

    public float StartRadians;
    public float SizeRadians;

    public float OuterRadius;
    public float InnerRadius;

    public int Detail = 64;

    protected override void OnPopulateMesh([NotNull] VertexHelper vertexHelper)
    {
        Rect r = GetPixelAdjustedRect();

        vertexHelper.Clear();

        Vector2 pivot = rectTransform.pivot;
        float centerWidth = r.width * (0.5f - pivot.x);
        float centerHeight = r.height * (0.5f - pivot.y);

        float outerWidthRadius = OuterRadius * r.width * 0.5f;
        float outerHeightRadius = OuterRadius * r.height * 0.5f;
        float innerWidthRadius = InnerRadius * r.width * 0.5f;
        float innerHeightRadius = InnerRadius * r.height * 0.5f;

        float deltaRadians = SizeRadians / Detail;

        for (int i = 0; i <= Detail; i++)
        {
            float radians = StartRadians + i * deltaRadians;
            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);

            // adds vertex i * 2 (outer)
            vertexHelper.AddVert(
                new Vector3(cos * outerWidthRadius + centerWidth, sin * outerHeightRadius + centerHeight, 0f), color,
                Vector2.zero);
            // adds vertex i * 2 + 1 (inner)
            vertexHelper.AddVert(
                new Vector3(cos * innerWidthRadius + centerWidth, sin * innerHeightRadius + centerHeight, 0f), color,
                Vector2.zero);

            if (i <= 0) continue;

            // add the quad from i - 1 and i vertices
            vertexHelper.AddTriangle(i * 2 - 2, i * 2 - 1, i * 2);
            vertexHelper.AddTriangle(i * 2 - 1, i * 2, i * 2 + 1);
        }
    }
}
}
