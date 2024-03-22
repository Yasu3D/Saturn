using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace SaturnGame.RhythmGame
{
    [System.Serializable]
    public abstract class ChartElement
    {
        protected ChartElement(int measure, int tick)
        {
            this.measure = measure;
            this.tick = tick;
        }

        protected ChartElement()
        {
        }

        [NotNull]
        public ChartElement Clone()
        {
            return (ChartElement)MemberwiseClone();
        }

        /// <summary>
        /// Calculates the object's time in milliseconds <br />
        /// according to all BPM and TimeSignature changes.
        /// </summary>
        public virtual void CalculateTime([NotNull] List<Gimmick> bgmDataGimmicks)
        {
            if (bgmDataGimmicks.Count == 0)
            {
                Debug.LogError($"Cannot calculate Time of {this} because no bgmData has been set! Use CreateBgmData() to generate bgmData.");
                return;
            }

            int timeStamp = Measure * 1920 + Tick;
            Gimmick lastBgmData = bgmDataGimmicks.LastOrDefault(x => x.Measure * 1920 + x.Tick < timeStamp) ?? bgmDataGimmicks[0];

            float lastTime = lastBgmData.TimeMs;
            float currentMeasure = (Measure * 1920 + Tick) * SaturnMath.tickToMeasure;
            float lastMeasure = (lastBgmData.Measure * 1920 + lastBgmData.Tick) * SaturnMath.tickToMeasure;
            float timeSig = lastBgmData.TimeSig.Ratio;
            float bpm = lastBgmData.BeatsPerMinute;

            TimeMs = lastTime + (currentMeasure - lastMeasure) * (4 * timeSig * (60000f / bpm));
        }

        /// <summary>
        /// Calculates a scaled timestamp for a chartObject. <br />
        /// <b>This is slow and should not run every frame!</b>
        /// </summary>
        /// <remarks>
        /// This relies on TimeMs already being populated, so make sure to call CalculateTime() before.
        /// This function also relies on scaled HiSpeed timestamps to already be calculated. <br />
        /// Make sure <c>ChartManager.CreateHiSpeedData()</c> has already been called before this.
        /// </remarks>
        public virtual void CalculateScaledTime([NotNull] List<Gimmick> hiSpeedGimmicks)
        {
            if (hiSpeedGimmicks.Count == 0)
            {
                ScaledVisualTime = TimeMs;
            }

            Gimmick lastHiSpeed = hiSpeedGimmicks.LastOrDefault(x => x.TimeMs <= TimeMs);
            float hiSpeedScaledTime = lastHiSpeed?.ScaledVisualTime ?? 0;
            float hiSpeedTime = lastHiSpeed?.TimeMs ?? 0;
            float hiSpeed = lastHiSpeed?.HiSpeed ?? 1;

            ScaledVisualTime = hiSpeedScaledTime + (TimeMs - hiSpeedTime) * hiSpeed;
        }

        public virtual void ReverseTime(float startTime, float midTime, float endTime)
        {
            float mirrorTime = startTime + (endTime - midTime);
            // Remaps from [mid <> end] to [mirror <> start]
            float remap = SaturnMath.Remap(ScaledVisualTime, midTime, endTime, mirrorTime, startTime);
            ScaledVisualTime = remap;
        }

        private int measure;
        public virtual int Measure { get => measure; set => measure = value; }
        [Range(0, 1919)] private int tick;
        public virtual int Tick { get => tick; set => tick = value; }
        // ChartTick is the number of ticks since the beginning of the chart, combining both Measure and Tick into a single value.
        public int ChartTick => Measure * 1920 + Tick;
        public float TimeMs;
        public float ScaledVisualTime;
    }
}
