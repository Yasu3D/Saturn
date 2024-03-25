using System.Linq;
using UnityEngine;

namespace SaturnGame.RhythmGame
{
/// <summary>
/// PositionedChartObject represents a chart object that has a position and size on the ring, such as notes or hold segments.
/// </summary>
[System.Serializable]
public abstract class PositionedChartElement : ChartElement
{
    [Range(0, 59)] private int position;

    // Position of the note or start of hold note.
    public virtual int Position
    {
        get => position;
        set => position = value;
    }

    [Range(1, 60)] private int size;

    // Size of the note or start of hold note.
    public virtual int Size
    {
        get => size;
        set => size = value;
    }

    protected PositionedChartElement(int measure, int tick, int position, int size) : base(measure, tick)
    {
        this.position = position;
        this.size = size;
    }

    protected PositionedChartElement()
    {
    }

    // We can define a note as an interval in mod 60, e.g. [40, 50) for a 10-size note at position 40.
    // If the note crosses 60, still use mod 60 integers, e.g. [55, 5) for a 10-size note at position 55.
    /// <summary>
    /// Left is the beginning or "left side" of the interval represented in mod60.
    /// It is the clockwise-most segment of the note. Note that Left can be greater than Right.
    /// This is a synonym for Position.
    /// </summary>
    public int Left => Position;

    /// <summary>
    /// Right is the end or "right side" of the interval represented in mod60 (not inclusive).
    /// It is the counterclockwise-most segment of the note + 1. Note that Left can be greater than Right.
    /// </summary>
    public int Right => SaturnMath.Modulo(Position + Size, 60);

    public bool Touched(TouchState touchState)
    {
        return Enumerable.Range(Position, Size)
            .Any(offset => touchState.AnglePosPressedAtAnyDepth(SaturnMath.Modulo(offset, 60)));
    }
}
}