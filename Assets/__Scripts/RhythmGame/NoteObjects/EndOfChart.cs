using UnityEngine;

namespace SaturnGame.RhythmGame
{
    [System.Serializable]
    public class EndOfChart : ChartObject
    {
        public EndOfChart(int measure, int tick) : base(measure, tick)
        {
        }
    }
}
