using System.Collections.Generic;
using System.Linq;
using SaturnGame.RhythmGame;
using UnityEngine;

namespace SaturnGame.Rendering
{

// TODO: Rewrite this entire thing. Eventually...
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
[AddComponentMenu("SaturnGame/Rendering/Hold Surface Renderer")]
public class HoldSurfaceRenderer : MonoBehaviour
{
    private const float TunnelRadius = 1.72f; // 1.75 by default.
    private const float TunnelLength = -6f;

    // ==== MESH ====
    [SerializeField] private Material materialTemplate;
    private Material materialInstance;

    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private MeshRenderer meshRenderer;

    [SerializeField] private Mesh holdMesh;
    private readonly List<Vector3> vertList = new();
    private readonly List<Vector2> uvList = new();
    private int[] triangles;

    private int colorID;
    public HoldNote HoldNote;
    private static readonly int ColorPropertyId = Shader.PropertyToID("_ColorIndex");
    private static readonly int StatePropertyId = Shader.PropertyToID("_State");

    public bool Reverse;

    public TimeManager TimeManager;
    
    private enum HoldState
    {
        Neutral = 0,
        Held = 1,
        Dropped = 2,
    }
    private HoldState state;

    private void Awake()
    {
        materialInstance = new(materialTemplate);
        holdMesh = new();
        meshFilter.mesh = holdMesh;
    }

    private void Update()
    {
        HoldState newState;

        bool dropped = !HoldNote.CurrentlyHeld && HoldNote.LastHeldTimeMs.HasValue && HoldNote.LastHeldTimeMs < TimeManager.GameplayTimeMs - HoldNote.LeniencyMs;
        bool missed = !HoldNote.Held && HoldNote.TimeMs < TimeManager.GameplayTimeMs - 100; // Missed holds turn dark after 6 frames at 60fps, or 100ms.

        if (dropped || missed) newState = HoldState.Dropped;
        else if (HoldNote.Held) newState = HoldState.Held;
        else newState = HoldState.Neutral;
        
        if (state != newState)
        {
            state = newState;
            SetState((int)newState);
        }
    }
    
    public void SetRenderer(HoldNote hold)
    {
        colorID = NoteColors.GetColorID(hold);
        HoldNote = hold;

        if (materialInstance.HasInteger(ColorPropertyId))
            materialInstance.SetInteger(ColorPropertyId, colorID);

        SetState(0);

        meshRenderer.material = materialInstance;
        meshFilter.mesh = holdMesh;
    }

    public void SetState(int index)
    {
        if (!materialInstance.HasInteger(StatePropertyId)) return;
        materialInstance.SetInteger(StatePropertyId, index);
    }

    /// <summary>
    /// Generates a Hold Note Mesh.
    /// This still needs A LOT of optimizing/cleanup!!!
    /// </summary>
    public void GenerateMesh(float scrollDuration)
    {
        float holdStartTime = HoldNote.Start.ScaledVisualTime;
        float holdEndTime = HoldNote.End.ScaledVisualTime;

        int holdWidth = HoldNote.MaxSize;
        int holdLength = HoldNote.RenderedNotes.Length;
        int trueLength = 0; // To keep track of all sub-segments as well

        vertList.Clear();
        uvList.Clear();

        // Sort so that this still works when the hold is reversed.
        HoldSegment[] sortedRenderedNotes = HoldNote.RenderedNotes.OrderBy(seg => seg.ScaledVisualTime).ToArray();

        // For every RenderedNote
        for (int y = 0; y < holdLength; y++)
        {
            HoldSegment startNote = sortedRenderedNotes[y];
            int startNoteSize = startNote.Size;
            int startNotePos = startNote.Position;

            int endNoteSize = startNoteSize;
            int endNotePos = startNotePos;

            float startTime = startNote.ScaledVisualTime;
            float endTime = startTime + 1;
            float interval = 20;

            if (y != holdLength - 1)
            {
                HoldSegment endNote = sortedRenderedNotes[y + 1];
                endNoteSize = endNote.Size;
                endNotePos = endNote.Position;
                endTime = endNote.ScaledVisualTime;

                // Hold is straight and doesn't need sub-segments.
                // Set interval to the whole distance from start to end.
                if (startNoteSize == endNoteSize && startNotePos == endNotePos)
                    interval = endNote.ScaledVisualTime - startNote.ScaledVisualTime;
            }

            // For every sub-segment between RenderedNotes.
            for (float i = startTime; i < endTime; i += interval)
            {
                float localProgress = Mathf.InverseLerp(startTime, endTime, i);
                float globalProgress = Mathf.InverseLerp(holdStartTime, holdEndTime, i);

                float noteSize = Mathf.Lerp(startNoteSize, endNoteSize, localProgress);
                float notePos = SaturnMath.LerpRound(startNotePos, endNotePos, localProgress, 60);

                // Shrink hold sizes to fit note mesh
                if (noteSize < 60)
                {
                    noteSize -= 1.4f;
                    notePos += 0.7f;
                }

                float angleInterval = GetAngleInterval(noteSize, holdWidth);
                float depth = SaturnMath.InverseLerp(0, scrollDuration, i - holdStartTime);

                // Generate an arc of verts
                for (int x = 0; x <= holdWidth; x++)
                {
                    float currentAngle = (angleInterval * x + notePos) * 6;

                    vertList.Add(GetPointOnCylinder(Vector2.zero, TunnelRadius, TunnelLength, currentAngle, depth));
                    uvList.Add(GetUV(x, holdWidth, globalProgress));
                }

                // We've generated one segment. Increment for triangle gen.
                trueLength++;
            }
        }

        triangles = new int[holdWidth * trueLength * 6];

        int vert = 0;
        int tris = 0;

        for (int y = 0; y < trueLength - 1; y++)
        {
            for (int x = 0; x < holdWidth; x++)
            {
                // Draw triangles in this order to control normals
                triangles[tris + 2] = vert;
                triangles[tris + 1] = vert + 1;
                triangles[tris + 0] = vert + holdWidth + 1;

                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + holdWidth + 1;
                triangles[tris + 5] = vert + holdWidth + 2;

                vert++;
                tris += 6;
            }

            vert++;
        }

        holdMesh.Clear();

        holdMesh.vertices = vertList.ToArray();
        holdMesh.uv = uvList.ToArray();
        holdMesh.triangles = triangles;
    }

    private static Vector3 GetPointOnCylinder(Vector2 centerPoint, float coneRadius, float coneLength, float angle,
        float depth)
    {
        angle = 180 - angle;

        //if (reverse) depth *= -1;

        float x = coneRadius * Mathf.Cos(Mathf.Deg2Rad * angle) + centerPoint.x;
        float y = coneRadius * Mathf.Sin(Mathf.Deg2Rad * angle) + centerPoint.y;
        float z = coneLength * depth;

        return new(x, y, z);
    }

    private static Vector2 GetUV(float x, float noteSize, float y) => new(Mathf.InverseLerp(0, noteSize, x), y);

    private static float GetAngleInterval(float currentNoteSize, int maxNoteSize) => currentNoteSize / maxNoteSize;


    [SerializeField] private int debugGizmos;

    private void OnDrawGizmos()
    {
        for (int i = 0; i < debugGizmos; i++)
            Gizmos.DrawSphere(holdMesh.vertices[i], 0.1f);
    }
}
}