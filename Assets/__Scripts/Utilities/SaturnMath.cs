using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaturnGame
{
    public static class SaturnMath
    {
        /// <summary>
        /// <b>= 1 / 1920</b> <br />
        /// Used to convert a number of Ticks to it's equivalent fraction of a Measure. <br />
        /// <i>This conversion should happen as late as possible!</i><br />
        /// <c>measureFraction = ticks * SaturnMath.tickToMeasure;</c>
        /// </summary>
        public const float tickToMeasure = 1.0f / 1920.0f;
    }
}
