using UnityEngine;

namespace SaturnGame.RhythmGame
{
    [System.Serializable]
    public class Mask : PositionedChartObject
    {
        public MaskDirection Direction;
        // Add is true if this the mask is being added, and false if it's being removed.
        public bool Add;

        public Mask(int measure, int tick, int position, int size, MaskDirection direction, bool add) : base(measure, tick, position, size)
        {
            Direction = direction;
            Add = add;
        }

        public enum MaskDirection
        {
            // Note: Weird values are a result of .mer format.
            None = 3,
            Counterclockwise = 0,
            Clockwise = 1,
            Center = 2
        }
    }
}
