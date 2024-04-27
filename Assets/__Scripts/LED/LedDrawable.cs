using System;
using UnityEngine;

namespace SaturnGame.LED
{
    /// <summary>
    /// A single draw operation. Any drawing code must go in Draw().
    /// </summary>
    /// <remarks>
    /// A LedDrawable will automatically add itself to the <c>LedDrawableQueue</c> when enabled.
    /// </remarks>
    public abstract class LedDrawable : MonoBehaviour
    {
        public bool Enabled = true;
        public int Layer = 0;
        
        public abstract void Draw(ref Color32[] data);
        
        private void FixedUpdate()
        {
            if (!Enabled) return;
            LedCompositor.Instance.LedDrawableQueue.Add(this);
        }
    }
}
