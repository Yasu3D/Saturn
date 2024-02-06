using UnityEngine;

namespace SaturnGame.RhythmGame
{
    /// <summary>
    /// PositionedChartObject represents a chart object that has a position and size on the ring, such as notes or hold segments.
    /// </summary>
    [System.Serializable]
    public abstract class PositionedChartElement : ChartElement
    {
        [Range(0, 59)] private int _position;
        // Position of the note or start of hold note.
        public virtual int Position { get => _position; set => _position = value; }

        [Range(1, 60)] private int _size;
        // Size of the note or start of hold note.
        public virtual int Size { get => _size; set => _size = value; }

        public PositionedChartElement(int measure, int tick, int position, int size) : base(measure, tick)
        {
            _position = position;
            _size = size;
        }

        protected PositionedChartElement()
        {
        }
    }
}
