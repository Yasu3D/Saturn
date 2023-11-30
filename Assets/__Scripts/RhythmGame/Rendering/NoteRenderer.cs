using System.Collections;
using System.Collections.Generic;
using SaturnGame.RhythmGame;
using UnityEngine;

public class NoteRenderer : MonoBehaviour
{
    // ==== PROPERTIES ====
    [Header("PROPERTIES")]
    public List<Mesh> meshes;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    public Material material;
    public Note note;

    void SetNote(Note newNote)
    {
        note = newNote;

        meshFilter.mesh = meshes[note.Size - 1];
        material.SetColor("NoteColor", new Color (1,0,1,1));
        meshRenderer.material = material;

        float angle = note.Position * -6;
        transform.eulerAngles = new Vector3 (0, 0, angle);
    }
}
