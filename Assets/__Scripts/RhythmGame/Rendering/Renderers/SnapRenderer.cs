using System;
using System.Collections.Generic;
using SaturnGame.RhythmGame;
using UnityEngine;

namespace SaturnGame.Rendering
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    [AddComponentMenu("SaturnGame/Rendering/Snap Renderer")]
    public class SnapRenderer : IObjectRenderer
    {
        // ==== MESH ====
        [SerializeField] private Material materialTemplate;
        private Material materialInstance;

        // ==== NOTE INFO ====
        public int Size { get; private set; }
        public int Position { get; private set; }

        public Color Color { get; private set; }
        public int ColorID { get; private set; }
        public string Direction { get; private set; } = "_FORWARD";

        void Awake()
        {
            materialInstance = new(materialTemplate);
        }

        public void SetRenderer(SnapNote note)
        {
            Size = note.Size;
            Position = note.Position;

            Color = NoteColors.GetColor(note).color;
            ColorID = NoteColors.GetColorID(note);

            Direction = note.Direction is SnapNote.SnapDirection.Forward ? "_FORWARD" : "_BACKWARD";

            if (materialInstance.HasColor("_NoteColor"))
                materialInstance.SetColor("_NoteColor", Color);

            materialInstance.DisableKeyword("_DIRECTION_FORWARD");
            materialInstance.EnableKeyword("_DIRECTION" + Direction);

            meshFilter.mesh = meshes[Size - 1];
            meshRenderer.material = materialInstance;

            transform.eulerAngles = new Vector3 (0, 0, Position * -6);
        }
    }
}
