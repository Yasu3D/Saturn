using System.Collections.Generic;
using SaturnGame.RhythmGame;
using Unity.VisualScripting;
using UnityEngine;

namespace SaturnGame.Rendering
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    [AddComponentMenu("SaturnGame/Rendering/Hold Surface Renderer")]
    public class HoldSurfaceRenderer : MonoBehaviour
    {
        // ==== MESH ====
        [SerializeField] private Mesh holdMesh;
        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private Material materialTemplate;
        private Material materialInstance;

        // ==== NOTE INFO ====
        public int ColorID { get; private set; }

        void Awake()
        {
            materialInstance = new(materialTemplate);
        }

        void SetRendererProperties(Note note)
        {
            ColorID = NoteColors.GetColorID(note.NoteType);
        }

        void UpdateRenderer()
        {
            if (materialInstance.HasFloat("_ColorID"))
                materialInstance.SetFloat("_ColorID", ColorID);

            meshRenderer.material = materialInstance;
        }

        void Update()
        {
            meshFilter.mesh = holdMesh;
        }

        void GenerateMesh()
        {
            // imagine a lot of cool math here to make those spinny yellow thingies.
        }
    }
}
