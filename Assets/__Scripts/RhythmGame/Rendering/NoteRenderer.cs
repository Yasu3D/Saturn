using System;
using System.Collections.Generic;
using SaturnGame.RhythmGame;
using UnityEngine;

namespace SaturnGame.Rendering
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    [AddComponentMenu("SaturnGame/Rendering/Note Renderer")]
    public class NoteRenderer : MonoBehaviour
    {
        // ==== MESH ====
        [SerializeField] private List<Mesh> meshes;
        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private Material materialTemplate;
        private Material materialInstance;

        // ==== NOTE INFO ====
        public int Size { get; private set; }
        public int Position { get; private set; }

        public Color Color { get; private set; }
        public int Width { get; private set; } = 3;
        public bool IsSync { get; private set; } = false;
        public bool IsBonus { get; private set; } = false;
        public bool IsChain { get; private set; } = false;

        void Awake()
        {
            materialInstance = new(materialTemplate);
        }

        public void SetRendererProperties(Note note, int width)
        {
            Size = note.Size;
            Position = note.Position;

            Color = NoteColors.GetColor(note.NoteType);
            Width = width;

            IsSync = note.IsSync;
            IsBonus = note.BonusType is ObjectEnums.BonusType.Bonus; 
            IsChain = note.NoteType is ObjectEnums.NoteType.Chain;
        }

        public void UpdateRenderer()
        {
            if (materialInstance.HasColor("_NoteColor"))
                materialInstance.SetColor("_NoteColor", Color);

            if (materialInstance.HasFloat("_NoteWidth"))
                materialInstance.SetFloat("_NoteWidth", Width);
            
            if (materialInstance.HasFloat("_Sync"))
                materialInstance.SetFloat("_Sync", Convert.ToInt32(IsSync));

            if (materialInstance.HasFloat("_Bonus"))
                materialInstance.SetFloat("_Bonus", Convert.ToInt32(IsBonus));

            if (materialInstance.HasFloat("_Chain"))
                materialInstance.SetFloat("_Chain", Convert.ToInt32(IsChain));

            meshFilter.mesh = meshes[Size - 1];
            meshRenderer.material = materialInstance;

            transform.eulerAngles = new Vector3 (0, 0, Position * -6);
        }
    }
}
