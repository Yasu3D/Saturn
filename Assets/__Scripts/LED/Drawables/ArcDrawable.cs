using UnityEngine;

namespace SaturnGame.LED
{
/// <summary>
/// Draws an arc with variable Position, Size, Thickness, Depth and Color.
/// </summary>
    public class CustomArcDrawable : LedDrawable
    {
        [Range(0, 60)] public int Position = 0;
        [Range(0, 60)] public int Size = 15;
        
        [Range(0, 4)] public int Thickness = 1;
        [Range(0, 3)] public int Depth = 0;

        public Color32 LedColor = Color.black;
        
        public override void Draw(ref Color32[] data)
        {
            for (int offsetX = Position; offsetX < Position + Size; offsetX++)
            for (int offsetY = 0; offsetY < Thickness; offsetY++)
            {
                int anglePos = SaturnMath.Modulo(offsetX, 60);
                int depthPos = Mathf.Min(offsetY + Depth, 3);
                
                data[anglePos * 8 + depthPos * 2] = LedColor;
                data[anglePos * 8 + depthPos * 2 + 1] = LedColor;
            }
        }
    }
}
