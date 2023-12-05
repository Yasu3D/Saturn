using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaturnGame.Rendering
{
    public abstract class IObjectRenderer : MonoBehaviour
    {
        public List<Mesh> meshes;
        public MeshFilter meshFilter;
        public MeshRenderer meshRenderer;
    }
}