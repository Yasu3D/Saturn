using SaturnGame.RhythmGame;
using UnityEngine;

namespace SaturnGame.Rendering
{
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
[AddComponentMenu("SaturnGame/Rendering/Swipe Renderer")]
public class SwipeRenderer : AbstractPositionedChartElementRenderer<SwipeNote>
{
    [SerializeField] private Material materialTemplate;
    private Material materialInstance;
    private static readonly int NoteColorPropertyID = Shader.PropertyToID("_NoteColor");

    private Color Color { get; set; }
    private string Direction { get; set; } = "_COUNTERCLOCKWISE";

    private void Awake()
    {
        materialInstance = new Material(materialTemplate);
    }

    public override void SetRenderer(SwipeNote note)
    {
        Size = note.Size;
        Position = note.Position;

        int colorID = NoteColors.GetColorID(note);
        Color = NoteColors.GetSwipeColor(colorID);

        Direction = note.Direction is SwipeNote.SwipeDirection.Counterclockwise ? "_COUNTERCLOCKWISE" : "_CLOCKWISE";

        if (materialInstance.HasColor(NoteColorPropertyID))
            materialInstance.SetColor(NoteColorPropertyID, Color);

        materialInstance.DisableKeyword("_DIRECTION_COUNTERCLOCKWISE");
        materialInstance.EnableKeyword("_DIRECTION" + Direction);

        MeshFilter.mesh = Meshes[Size - 1];
        MeshRenderer.material = materialInstance;

        transform.eulerAngles = new Vector3(0, 0, Position * -6);
    }
}
}