using System;
using System.Collections.Generic;
using SaturnGame.RhythmGame;
using UnityEngine;

namespace SaturnGame.Rendering
{
    //[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    //[AddComponentMenu("SaturnGame/Rendering/Generic Renderer")]
    public abstract class AbstractPositionedChartElementRenderer<T> : IObjectRenderer where T : PositionedChartElement
    {
        public int Size { get; protected set; }
        public int Position { get; protected set; }

        public virtual void SetRenderer(T chartElement)
        {
            Size = chartElement.Size;
            Position = chartElement.Position;
            meshFilter.mesh = meshes[Size - 1];
            transform.eulerAngles = new Vector3 (0, 0, Position * -6);
        }
    }
}
