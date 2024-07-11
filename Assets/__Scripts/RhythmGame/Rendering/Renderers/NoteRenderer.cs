using System;
using SaturnGame.RhythmGame;
using UnityEngine;

namespace SaturnGame.Rendering
{
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
[AddComponentMenu("SaturnGame/Rendering/Note Renderer")]
public class NoteRenderer : AbstractPositionedChartElementRenderer<Note>
{
    // ==== MESH ====
    [SerializeField] private Material materialTemplate;
    private Material materialInstance;
    private static readonly int NoteColorPropertyId = Shader.PropertyToID("_NoteColor");
    private static readonly int NoteWidthPropertyID = Shader.PropertyToID("_NoteWidth");
    private static readonly int SyncPropertyID = Shader.PropertyToID("_Sync");
    private static readonly int BonusPropertyID = Shader.PropertyToID("_Bonus");
    private static readonly int ChainPropertyID = Shader.PropertyToID("_Chain");
    private static readonly int StrengthPropertyID = Shader.PropertyToID("_SubStrength");
    private static readonly int ZOffsetPropertyID = Shader.PropertyToID("_Z_Offset");

    // ==== NOTE INFO ====
    private Color Color { get; set; }
    public int Width { get; set; } = 3;
    private bool IsSync { get; set; }
    private bool IsBonus { get; set; }
    private bool IsChain { get; set; }

    private void Awake()
    {
        materialInstance = new(materialTemplate);
    }

    public override void SetRenderer(Note note)
    {
        Size = note.Size;
        Position = note.Position;
        
        Color = NoteColors.GetColor(note);

        IsSync = note.IsSync;
        IsBonus = note.BonusType is Note.NoteBonusType.Bonus;
        IsChain = note is ChainNote;

        if (materialInstance.HasColor(NoteColorPropertyId))
            materialInstance.SetColor(NoteColorPropertyId, Color);

        if (materialInstance.HasFloat(NoteWidthPropertyID))
            materialInstance.SetFloat(NoteWidthPropertyID, Width);

        if (materialInstance.HasFloat(SyncPropertyID))
            materialInstance.SetFloat(SyncPropertyID, Convert.ToInt32(IsSync));

        if (materialInstance.HasFloat(BonusPropertyID))
            materialInstance.SetFloat(BonusPropertyID, Convert.ToInt32(IsBonus));

        if (materialInstance.HasFloat(ChainPropertyID))
            materialInstance.SetFloat(ChainPropertyID, Convert.ToInt32(IsChain));

        if (materialInstance.HasFloat(ZOffsetPropertyID))
        {
            int state = note is HoldNote ? 1 : 0;
            materialInstance.SetFloat(ZOffsetPropertyID, state);
        }

        MeshFilter.mesh = Meshes[Size - 1];
        MeshRenderer.material = materialInstance;

        transform.eulerAngles = new(0, 0, Position * -6);
    }
}
}