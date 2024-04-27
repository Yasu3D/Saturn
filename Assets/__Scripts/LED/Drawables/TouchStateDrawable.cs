using System;
using System.Collections;
using System.Collections.Generic;
using SaturnGame.LED;
using SaturnGame.RhythmGame;
using UnityEngine;

namespace SaturnGame
{
    /// <summary>
    /// Lights up segments that are being touched.
    /// Currently does not do the fancy "ripple" effect as seen in mercury.
    /// </summary>
    public class TouchStateDrawable : LedDrawable
    {
        [SerializeField] private InputManager inputManager;
        
        public override void Draw(ref Color32[] data)
        {
            // TODO: @cg505 hook this into the inputManager. Keep in mind that LEDs will poll input at 30hz. (33.333hz)
            // Ideally input data should just *exist* in one place and other scripts should just be able to look at it.
            
            for (int anglePos = 0; anglePos < 60; anglePos++)
            for (int depthPos = 0; depthPos < 4; depthPos++)
            {
                // TODO: replace false with proper values. See above^
                data[anglePos * 8 + 2 * depthPos] = false ? Color.white : Color.black;
                data[anglePos * 8 + 2 * depthPos + 1] = false ? Color.white : Color.black;
            }
        }
    }
}
