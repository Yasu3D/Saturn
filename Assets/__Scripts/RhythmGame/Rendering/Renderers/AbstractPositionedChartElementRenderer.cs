using JetBrains.Annotations;
using SaturnGame.RhythmGame;
using UnityEngine;

namespace SaturnGame.Rendering
{
    public abstract class AbstractPositionedChartElementRenderer<T> : ObjectRenderer where T : PositionedChartElement
    {
        protected int Size { get; set; }
        protected int Position { get; set; }

        public virtual void SetRenderer([NotNull] T chartElement)
        {
            Size = chartElement.Size;
            Position = chartElement.Position;
            MeshFilter.mesh = Meshes[Size - 1];
            transform.eulerAngles = new Vector3 (0, 0, Position * -6);
        }
    }
}
