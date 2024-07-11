using SaturnGame.RhythmGame;
using UnityEngine;

namespace SaturnGame.Rendering
{
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
[AddComponentMenu("SaturnGame/Rendering/Snap Renderer")]
public class SnapRenderer : AbstractPositionedChartElementRenderer<SnapNote>
{
    // ==== MESH ====
    [SerializeField] private Material materialTemplate;
    private Material materialInstance;
    private static readonly int NoteColorPropertyID = Shader.PropertyToID("_NoteColor");
    private static readonly int FlipArrowPropertyID = Shader.PropertyToID("_FlipArrow");

    // ==== NOTE INFO ====
    private Color Color { get; set; }
    private int FlipArrow { get; set; }

    private void Awake()
    {
        materialInstance = new(materialTemplate);
    }

    public override void SetRenderer(SnapNote note)
    {
        Size = note.Size;
        Position = note.Position;

        Color = NoteColors.GetColor(note);
        NoteColors.GetColorID(note);

        FlipArrow = note.Direction is SnapNote.SnapDirection.Forward ? 0 : 1;

        if (materialInstance.HasColor(NoteColorPropertyID))
        {
            materialInstance.SetColor(NoteColorPropertyID, Color);
        }

        if (materialInstance.HasInteger(FlipArrowPropertyID))
        {
            materialInstance.SetInteger(FlipArrowPropertyID, FlipArrow);
        }

        MeshFilter.mesh = Meshes[Size - 1];
        MeshRenderer.material = materialInstance;

        transform.eulerAngles = new(0, 0, Position * -6);
    }
}
}