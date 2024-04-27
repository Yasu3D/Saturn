using System;
using SaturnGame.LED;
using SaturnGame.UI;
using UnityEngine;

namespace SaturnGame
{
    /// <summary>
    /// Draws the colored area in front of menu touch buttons
    /// </summary>
    public class TouchButtonDrawable : LedDrawable
    {
        [SerializeField] private TouchButton touchButton;
        
        public override void Draw(ref Color32[] data)
        {
            for (int offset = touchButton.Position; offset < touchButton.Position + touchButton.Size; offset++)
            for (int depthPos = touchButton.MinDepthPos; depthPos <= touchButton.MaxDepthPos; depthPos++)
            {
                int anglePos = SaturnMath.Modulo(offset, 60);
                data[anglePos * 8 + depthPos * 2] = touchButton.LedColor;
                data[anglePos * 8 + depthPos * 2 + 1] = touchButton.LedColor;
            }
        }
    }
}