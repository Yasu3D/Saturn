using UnityEngine;

namespace SaturnGame.RhythmGame
{
    /// <summary>
    /// PositionedChartObject represents a chart object that has a position and size on the ring, such as notes or hold segments.
    /// </summary>
    [System.Serializable]
    public abstract class PositionedChartElement : TimedChartElement
    {
        // Position of the note or start of hold note.
        [Range(0, 59)] public int Position;
        // Size of the note or start of hold note.
        [Range(1, 60)] public int Size;
        public PositionedChartElement(
            int measure,
            int tick,
            int position,
            int size) : base(measure, tick)
        {
            Position = position;
            Size = size;
        }
    }
}
