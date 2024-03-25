using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace SaturnGame.Rendering
{
public abstract class ObjectRenderer : MonoBehaviour
{
    [FormerlySerializedAs("meshes")] public List<Mesh> Meshes;
    [FormerlySerializedAs("meshFilter")] public MeshFilter MeshFilter;
    [FormerlySerializedAs("meshRenderer")] public MeshRenderer MeshRenderer;
}
}