using UnityEngine;

namespace SaturnGame.RhythmGame
{
    /// <summary>
    /// SyncIndicator represents the line that connects two notes at the same time.
    /// </summary>
    [System.Serializable]
    public class SyncIndicator : PositionedChartObject
    {
        public SyncIndicator(int measure, int tick, int position, int size) : base(measure, tick, position, size)
        {
        }
    }
}
