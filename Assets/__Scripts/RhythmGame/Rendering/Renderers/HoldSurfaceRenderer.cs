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
        private const float tunnelRadius = 1.72f; // 1.75 by default.
        private const float tunnelLength = -6f;

        // ==== MESH ====
        [SerializeField] private Material materialTemplate;
        private Material materialInstance;

        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private MeshRenderer meshRenderer;

        [SerializeField] private Mesh holdMesh;
        private Vector3[] vertices;
        private Vector2[] uv;
        private int[] triangles;

        private int ColorID;
        public HoldNote holdNote;

        public bool reverse;

        void Awake()
        {
            materialInstance = new(materialTemplate);
            holdMesh = new();
            meshFilter.mesh = holdMesh;
        }

        public void SetRenderer(HoldNote hold)
        {
            ColorID = NoteColors.GetColorID(hold.Start.NoteType);
            holdNote = hold;

            if (materialInstance.HasFloat("_ColorID"))
                materialInstance.SetFloat("_ColorID", ColorID);

            meshRenderer.material = materialInstance;
            meshFilter.mesh = holdMesh;
        }

        public void GenerateMesh(float visualTime, float scrollDuration)
        {
            int maxWidth = holdNote.MaxSize;
            int maxLength = holdNote.RenderedNotes.Length;

            vertices = new Vector3[(maxWidth + 1) * maxLength];
            uv = new Vector2[(maxWidth + 1) * maxLength];

            int vertexID = 0;
            for (int x = 0; x <= maxWidth; x++)
            {
                for (int y = 0; y < maxLength; y++)
                {
                    float noteSize = holdNote.RenderedNotes[y].Size;
                    float notePosition = holdNote.RenderedNotes[y].Position;

                    if (noteSize < 60)
                    {
                        noteSize -= 1.6f;
                        notePosition += 0.8f;
                    }

                    float sizeMultiplier = GetAngleInterval(noteSize, maxWidth);
                    float currentAngle = (sizeMultiplier * x + notePosition) * 6;

                    float time = holdNote.RenderedNotes[y].ScaledVisualTime;
                    float distance = time - visualTime;
                    float depth = SaturnMath.InverseLerp(0, scrollDuration, distance);

                    vertices[vertexID] = GetPointOnCone(Vector2.zero, tunnelRadius, tunnelLength, currentAngle, depth);
                    uv[vertexID] = GetUV(x, maxWidth, y, maxLength);
                    vertexID++;
                }
            }

            triangles = new int[maxWidth * maxLength * 6]; 

            int vert = 0;
            int tris = 0;

            for (int y = 0; y < maxWidth; y++)
            {
                for (int x = 0; x < (maxLength - 1); x++)
                {
                    // Draw triangles counterclockwise to flip normals
                    triangles[tris + 2] = vert + 0;
                    triangles[tris + 1] = vert + (maxLength - 1) + 1;
                    triangles[tris + 0] = vert + 1;
                    
                    triangles[tris + 5] = vert + 1;
                    triangles[tris + 4] = vert + (maxLength - 1) + 1;
                    triangles[tris + 3] = vert + (maxLength - 1) + 2;

                    vert ++;
                    tris += 6;
                }

                vert++;
            }

            holdMesh.Clear();

            holdMesh.vertices = vertices;
            holdMesh.triangles = triangles;
            holdMesh.uv = uv;
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

        Vector3 GetPointOnCone(Vector2 centerPoint, float coneRadius, float coneLength, float angle, float depth, bool clamp = true)
        {
            angle = 180 - angle;

            float x = coneRadius * (1 - depth) * Mathf.Cos(Mathf.Deg2Rad * angle) + centerPoint.x;
            float y = coneRadius * (1 - depth) * Mathf.Sin(Mathf.Deg2Rad * angle) + centerPoint.y;
            float z = coneLength * depth;

            if (clamp && z <= coneLength)
            {
                x = 0;
                y = 0;
                z = coneLength;
            }

            return new Vector3 (x, y, z);
        }

        Vector2 GetUV(float x, float noteSize, float y, float depth)
        {
            float u = Mathf.InverseLerp(0, noteSize, x);
            float v = Mathf.InverseLerp(0, depth, y);
            return new Vector2 (u,v);
        }

        float GetAngleInterval(float currentNoteSize, int maxNoteSize)
        {
            return (float) currentNoteSize / maxNoteSize;
        }
    
        void OnDrawGizmos()
        {
            foreach (Vector3 vert in vertices)
            {
                Gizmos.DrawSphere(vert, 0.1f);
            }
        }
    }
}
