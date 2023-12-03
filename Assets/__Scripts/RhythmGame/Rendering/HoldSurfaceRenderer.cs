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
        private const float tunnelRadius = 1.75f;
        private const float tunnelLength = 6f;
        // ==== MESH ====
        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private Mesh holdMesh;
        private Vector3[] vertices;
        private Vector2[] uv;
        private int[] triangles;

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

        void GenerateMesh(HoldNote holdNote)
        {
            int holdDivisions = GetMaxDivisions(10);
            vertices = new Vector3[(holdNote.MaxSize + 1) * holdDivisions];
            uv = new Vector2[(holdNote.MaxSize + 1) * holdDivisions];

            int vertexID = 0;
            for (int x = 0; x <= holdNote.MaxSize; x++)
            {
                for (int y = 0; y < holdDivisions; y++)
                {
                    float stepSize = holdNote.Distance[y] / holdDivisions;

                    float sizeMultiplier = GetAngleInterval(holdNote.Notes[y].Size, holdNote.MaxSize);
                    float currentAngle = (sizeMultiplier * x + holdNote.Notes[y].Position) * 6;
                    
                    vertices[vertexID] = GetPointOnCone(Vector2.zero, tunnelRadius, tunnelLength, currentAngle, y);
                    uv[vertexID] = GetUV(x, holdNote.MaxSize, y, holdDivisions);
                    vertexID++;
                }
            }
        }

        private int GetMaxDivisions(float distance)
        {
            int divisor = 1;
            while (distance / divisor > 0.12f)
            {
                divisor++;
            }

            return divisor;
        }

        Vector3 GetPointOnCone(Vector2 centerPoint, float radius, float length, float angle, float depth)
        {
            float scaledDepth = depth * 0.12f;
            float scale = Mathf.InverseLerp(length, 0, scaledDepth);

            angle = 180 - angle;

            float x = radius * scale * Mathf.Cos(Mathf.Deg2Rad * angle) + centerPoint.x;
            float y = radius * scale * Mathf.Sin(Mathf.Deg2Rad * angle) + centerPoint.y;
            float z = -scaledDepth;

            return new Vector3 (x, y, z);
        }

        Vector2 GetUV(float x, float noteSize, float y, float depth)
        {
            float u = Mathf.InverseLerp(0, noteSize, x);
            float v = Mathf.InverseLerp(0, depth, y);
            return new Vector2 (u,v);
        }

        float GetAngleInterval(int currentNoteSize, int maxNoteSize)
        {
            return (float) currentNoteSize / (float) maxNoteSize;
        }
    }
}
