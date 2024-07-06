using SaturnGame.RhythmGame;
using UnityEngine;

namespace SaturnGame.Rendering
{
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
[AddComponentMenu("SaturnGame/Rendering/Hold End Renderer")]
public class HoldEndRenderer : AbstractPositionedChartElementRenderer<HoldSegment>
{
    [SerializeField] private Material materialTemplate;
    private Material materialInstance;

    private static readonly int ColorPropertyID = Shader.PropertyToID("_ColorIndex");

    // ==== NOTE INFO ====

    private int ColorID { get; set; }

    private void Awake()
    {
        materialInstance = new Material(materialTemplate);
    }

    public override void SetRenderer(HoldSegment note)
    {
        Size = note.Size;
        Position = note.Position;

        ColorID = NoteColors.GetColorID(note);

        if (materialInstance.HasFloat(ColorPropertyID))
            materialInstance.SetFloat(ColorPropertyID, ColorID);

        MeshFilter.mesh = Meshes[Size - 1];
        MeshRenderer.material = materialInstance;

        transform.eulerAngles = new Vector3(0, 0, Position * -6);
    }
}
}