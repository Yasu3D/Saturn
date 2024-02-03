using UnityEngine;

namespace SaturnGame.RhythmGame
{
    [System.Serializable]
    public class EndOfChart : TimedChartElement
    {
        public EndOfChart(int measure, int tick) : base(measure, tick)
        {
        }
    }
}
