using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaturnGame
{
    public static class SaturnMath
    {
        /// <summary>
        /// Used to convert a number of Ticks to it's equivalent fraction of a Measure. <br />
        /// <i>This conversion should happen as late as possible!</i><br />
        /// <c>measureFraction = ticks * SaturnMath.tickToMeasure;</c>
        /// </summary>
        public const float tickToMeasure = 1.0f / 1920.0f;

        /// <summary>
        /// Unclamped version of Unity's <c>Mathf.InverseLerp</c>.
        /// </summary>
        /// <returns>
        /// Where <c>value</c> lies between <c>a</c> and <c>b</c>.
        /// </returns>
        public static float InverseLerp(float a, float b, float value)
        {
            if (a != b)
            {
                return (value - a) / (b - a);
            }

            return 0f;
        }

        /// <summary>
        /// Returns the Euclidean remainder ("true modulo") of a number. <br />
        /// The result will always be positive, unlike using the <c>%</c> operator in C#.
        /// </summary>
        public static int Modulo(int x, int m)
        {
            return (x % m + m) % m;
        }

        /// <summary>
        /// Remaps a value from range <c>inMin - inMax</c> to <c>outMin - outMax</c>
        /// </summary>
        public static float Remap(float input, float inMin, float inMax, float outMin, float outMax, bool clamp = false)
        {
            if (inMin == inMax || outMin == outMax) return 0;
            if (inMin == outMin && inMax == outMax) return input;

            float result = outMin + (input - inMin) * (outMax - outMin) / (inMax - inMin);
            return clamp ? Mathf.Clamp(result, outMin, outMax) : result;
        }
    }
}
