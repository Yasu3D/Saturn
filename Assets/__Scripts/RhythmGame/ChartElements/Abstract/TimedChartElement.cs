using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaturnGame.RhythmGame
{
    [System.Serializable]
    public abstract class TimedChartElement
    {
        public TimedChartElement(int measure, int tick)
        {
            Measure = measure;
            Tick = tick;
        }

        public TimedChartElement Clone() {
            return (TimedChartElement)MemberwiseClone();
        }

        public int Measure;
        [Range(0, 1919)] public int Tick;
        public float TimeMs;
        public float ScaledVisualTime;
    }
}
