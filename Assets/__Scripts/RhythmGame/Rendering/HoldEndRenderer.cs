using System.Collections.Generic;
using SaturnGame.RhythmGame;
using UnityEngine;

namespace SaturnGame.Rendering
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    [AddComponentMenu("SaturnGame/Rendering/Hold End Renderer")]
    public class HoldEndRenderer : MonoBehaviour
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

        public int ColorID { get; private set; }

        void Awake()
        {
            materialInstance = new(materialTemplate);
        }

        void SetRendererProperties(Note note)
        {
            Size = note.Size;
            Position = note.Position;

            ColorID = NoteColors.GetColorID(note.NoteType);
        }

        void UpdateRenderer()
        {
            if (materialInstance.HasFloat("_ColorID"))
                materialInstance.SetFloat("_ColorID", ColorID);

            meshFilter.mesh = meshes[Size - 1];
            meshRenderer.material = materialInstance;

            transform.eulerAngles = new Vector3 (0, 0, Position * -6);
        }
    }
}
