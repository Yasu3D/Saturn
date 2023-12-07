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
    }
}
