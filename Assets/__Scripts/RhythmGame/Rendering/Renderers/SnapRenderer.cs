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

    // ==== NOTE INFO ====
    private Color Color { get; set; }
    private string Direction { get; set; } = "_FORWARD";

    private void Awake()
    {
        materialInstance = new Material(materialTemplate);
    }

    public override void SetRenderer(SnapNote note)
    {
        Size = note.Size;
        Position = note.Position;

        Color = NoteColors.GetColor(note).color;
        NoteColors.GetColorID(note);

        Direction = note.Direction is SnapNote.SnapDirection.Forward ? "_FORWARD" : "_BACKWARD";

        if (materialInstance.HasColor(NoteColorPropertyID))
            materialInstance.SetColor(NoteColorPropertyID, Color);

        materialInstance.DisableKeyword("_DIRECTION_FORWARD");
        materialInstance.EnableKeyword("_DIRECTION" + Direction);

        MeshFilter.mesh = Meshes[Size - 1];
        MeshRenderer.material = materialInstance;

        transform.eulerAngles = new Vector3(0, 0, Position * -6);
    }
}
}