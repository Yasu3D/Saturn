using SaturnGame.UI;
using UnityEngine;

namespace SaturnGame.LED
{
    /// <summary>
    /// Draws the colored area in front of menu touch buttons
    /// </summary>
    public class TouchButtonDrawable : LedDrawable
    {
        [SerializeField] private TouchButton touchButton;
        
        public override void Draw(ref Color32[,] data)
        {
            for (int i = 0; i < touchButton.Size; i++)
            for (int j = 0; j < touchButton.Thickness; i++)
            {
                int x = SaturnMath.Modulo(i + touchButton.Position, 60);
                int y = Mathf.Min(j + touchButton.Depth, 7);

                data[y, x] = touchButton.LedColor;
            }
        }
    }
}