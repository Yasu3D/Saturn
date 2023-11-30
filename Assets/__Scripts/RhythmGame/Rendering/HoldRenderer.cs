using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HoldRenderer : MonoBehaviour
{
    private const float tunnelRadius = 1.75f;
    private const float tunnelLength = 6f;

    // ================ Test variables
    [Range(1,60)] [SerializeField] private int selectedSize;
    [Range(1,60)] [SerializeField] private int selectedDepth;
    [SerializeField] private float selectedSpin;
    [SerializeField] private float selectedScale = 1;
    // ===============================


    [SerializeField] private HoldNote hold;

    private Mesh mesh;
    [SerializeField] private MeshFilter meshFilter;
    private Vector3[] vertices;
    private Vector2[] uv;
    private int[] triangles;

    void Start()
    {
        mesh = new Mesh();
        meshFilter.mesh = mesh;
    }

    void TestGenerate(int noteSize, int depth)
    {
        vertices = new Vector3[(noteSize + 1) * (depth + 1)];
        uv = new Vector2[(noteSize + 1) * (depth + 1)];

        int vertID = 0;
        for (int x = 0; x <= noteSize; x++)
        {
            for (int y = 0; y <= depth; y++)
            {
                vertices[vertID] = GetPointOnCone(Vector2.zero, tunnelRadius, tunnelLength, x * 6, y);
                uv[vertID] = GetUV(x, noteSize, y, depth);
                vertID++;
            }
        }

        triangles = new int[noteSize * depth * 6];

        int vert = 0;
        int tris = 0;

        for (int y = 0; y < noteSize; y++)
        {
            for (int x = 0; x < depth; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + depth + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + depth + 1;
                triangles[tris + 5] = vert + depth + 2;

                vert ++;
                tris += 6;
            }

            vert ++;
        }
    }

    void Generate(HoldNote holdNote)
    {
        // this code doesnt work yet and is very stupid.
        vertices = new Vector3[(holdNote.MaxSize + 1) * holdNote.TotalLength];
        uv = new Vector2[(holdNote.MaxSize + 1) * holdNote.TotalLength];

        int vertID = 0;
        for (int x = 0; x <= holdNote.MaxSize; x++)
        {
            for (int y = 0; y < holdNote.TotalLength; y++)
            {
                float sizeMultiplier = GetAngleInterval(holdNote.Notes[y].Size, holdNote.MaxSize);
                float currentAngle = (sizeMultiplier * x + holdNote.Notes[y].Position) * 6;
                
                vertices[vertID] = GetPointOnCone(Vector2.zero, tunnelRadius, tunnelLength, currentAngle, y);
                uv[vertID] = GetUV(x, holdNote.MaxSize, y, holdNote.TotalLength);
                vertID++;
            }
        }

        triangles = new int[holdNote.MaxSize * (holdNote.TotalLength - 1) * 6];

        int vert = 0;
        int tris = 0;

        for (int y = 0; y < holdNote.MaxSize; y++)
        {
            for (int x = 0; x < (holdNote.TotalLength - 1); x++)
            {
                // draw triangles counterclockwise because normals are being stupid
                triangles[tris + 2] = vert + 0;
                triangles[tris + 1] = vert + (holdNote.TotalLength - 1) + 1;
                triangles[tris + 0] = vert + 1;
                
                triangles[tris + 5] = vert + 1;
                triangles[tris + 4] = vert + (holdNote.TotalLength - 1) + 1;
                triangles[tris + 3] = vert + (holdNote.TotalLength - 1) + 2;

                vert ++;
                tris += 6;
            }

            vert ++;
        }
    }


    void SetMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
    }

    Vector3 GetPointOnCone(Vector2 centerPoint, float radius, float length, float angle, float depth)
    {
        float scaledDepth = depth * selectedScale;
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

    void OnDrawGizmos()
    {
        if (vertices == null) return;
        // debug
        foreach (Vector3 vert in vertices)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(vert, 0.025f);
        }
    }

    void Update()
    {
        Generate(hold);
        SetMesh();
    }
}
