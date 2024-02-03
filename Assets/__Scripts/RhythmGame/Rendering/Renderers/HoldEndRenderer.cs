using System.Collections.Generic;
using SaturnGame.RhythmGame;
using UnityEngine;

namespace SaturnGame.Rendering
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    [AddComponentMenu("SaturnGame/Rendering/Hold End Renderer")]
    public class HoldEndRenderer : IObjectRenderer
    {
        [SerializeField] private Material materialTemplate;
        private Material materialInstance;

        // ==== NOTE INFO ====
        public int Size { get; private set; }
        public int Position { get; private set; }

        public int ColorID { get; private set; }

        void Awake()
        {
            materialInstance = new(materialTemplate);
        }

        public void SetRenderer(HoldSegment note)
        {
            Size = note.Size;
            Position = note.Position;

            ColorID = NoteColors.GetColorID(note);

            if (materialInstance.HasFloat("_ColorID"))
                materialInstance.SetFloat("_ColorID", ColorID);

            meshFilter.mesh = meshes[Size - 1];
            meshRenderer.material = materialInstance;

            transform.eulerAngles = new Vector3 (0, 0, Position * -6);
        }
    }
}
