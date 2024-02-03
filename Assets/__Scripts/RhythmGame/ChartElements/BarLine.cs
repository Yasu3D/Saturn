using UnityEngine;

namespace SaturnGame.RhythmGame
{
    [System.Serializable]
    public class BarLine : TimedChartElement
    {
        public BarLine(int measure, int tick) : base(measure, tick)
        {
        }
    }
}
