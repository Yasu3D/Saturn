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
    private static readonly int FlipArrowPropertyID = Shader.PropertyToID("_FlipArrow");
    private static readonly int NoteSizePropertyID = Shader.PropertyToID("_NoteSize");
    
    private void Awake()
    {
        materialInstance = new(materialTemplate);
    }

    public override void SetRenderer(SwipeNote note)
    {
        Size = note.Size;
        Position = note.Position;

        int colorID = NoteColors.GetColorID(note);

        if (materialInstance.HasColor(NoteColorPropertyID))
        {
            materialInstance.SetColor(NoteColorPropertyID, NoteColors.GetColor(colorID));
        }

        if (materialInstance.HasInteger(FlipArrowPropertyID))
        {
            materialInstance.SetInteger(FlipArrowPropertyID, note.Direction is SwipeNote.SwipeDirection.Counterclockwise ? 1 : 0);
        }
        
        if (materialInstance.HasFloat(NoteSizePropertyID))
        {
            materialInstance.SetFloat(NoteSizePropertyID, Size);
        }

        MeshFilter.mesh = Meshes[Size - 1];
        MeshRenderer.material = materialInstance;

        transform.eulerAngles = new(0, 0, Position * -6);
    }
}
}