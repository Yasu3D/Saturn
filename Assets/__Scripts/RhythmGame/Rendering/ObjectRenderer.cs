using System;
using System.Collections.Generic;
using UnityEngine;

namespace SaturnGame.Rendering
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    [AddComponentMenu("SaturnGame/Rendering/Object Renderer")]
    public class ObjectRenderer : MonoBehaviour
    {
        // ==== MESH ====
        public List<Mesh> meshes;
        public MeshFilter meshFilter;
        public MeshRenderer meshRenderer;

        // ==== INFO ====
        public int Size { get; private set; }
        public int Position { get; private set; }

        void SetRendererProperties(int size, int position)
        {
            Size = size;
            Position = position;
        }

        void UpdateRenderer()
        {
            meshFilter.mesh = meshes[Size - 1];
            transform.eulerAngles = new Vector3 (0, 0, Position * -6);
        }
    }
}
