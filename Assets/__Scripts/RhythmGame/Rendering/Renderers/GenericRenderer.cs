using System;
using System.Collections.Generic;
using UnityEngine;

namespace SaturnGame.Rendering
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    [AddComponentMenu("SaturnGame/Rendering/Generic Renderer")]
    public class GenericRenderer : IObjectRenderer
    {
        public int Size { get; private set; }
        public int Position { get; private set; }

        public void SetRenderer(int size, int position)
        {
            Size = size;
            Position = position;
            meshFilter.mesh = meshes[Size - 1];
            transform.eulerAngles = new Vector3 (0, 0, Position * -6);
        }
    }
}
