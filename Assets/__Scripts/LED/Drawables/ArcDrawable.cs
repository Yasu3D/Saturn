using UnityEngine;

namespace SaturnGame.LED
{
/// <summary>
/// Draws an arc with variable Position, Size, Thickness, Depth and Color.
/// </summary>
public class CustomArcDrawable : LedDrawable
{
    [Range(0, 59)] public int Position = 0;
    [Range(0, 60)] public int Size = 15;

    [Range(0, 8)] public int Thickness = 1;
    [Range(0, 7)] public int Depth = 0;

    private Color[,] data;
    public Color LedColor = Color.black;

    private void OnEnable()
    {
        data = new Color[8, 60];
        LedCompositor.ClearCanvas(data, Color.clear);

        for (int i = 0; i < Size; i++)
        for (int j = 0; j < Thickness; j++)
        {
            int x = SaturnMath.Modulo(i + Position, 60);
            int y = Mathf.Min(j + Depth, 7);

            data[y, x] = LedColor;
        }
    }

    public override Color[,] Draw()
    {
        return data;
    }
}
}
