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
        [SerializeField] private TouchRipplePool touchRipplePool;
        [SerializeField] private float blinkSpeed = 2;
        
        [SerializeField] private bool[] input = new bool[240];
        private readonly bool[] prevInput = new bool[240];
        
        [SerializeField] private Color colorA = new(1.0f, 1.0f, 1.0f, 1.0f);
        [SerializeField] private Color colorB = new(0.7f, 0.7f, 0.7f, 1.0f);
        private Color GetColor() => Color.Lerp(colorA, colorB, SaturnMath.PositiveSine(Time.time * blinkSpeed));

        //private void Update()
        //{
        //    for (int i = 0; i < 240; i++)
        //    {
        //        if (input[i] && !prevInput[i])
        //        {
        //            TouchRippleDrawable drawable = touchRipplePool.GetObject();
        //            drawable.gameObject.SetActive(true);
        //            drawable.Layer = Layer;
        //            drawable.TouchPosition = i;
        //        }
        //        
        //        prevInput[i] = input[i];
        //    }
        //}
        
        public override void Draw(ref Color32[,] data)
        {
            for (int i = 0; i < 480; i++)
            {
                data[i % 8, i / 8] += input[i / 2] ? GetColor() : Color.clear;
            }
        }
    }
}
