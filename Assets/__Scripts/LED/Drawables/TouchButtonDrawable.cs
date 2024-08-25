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
    private readonly Color[,] data = new Color[8, 60];

    public override Color[,] Draw()
    {
        LedCompositor.ClearCanvas(data, Color.clear);

        for (int i = 0; i < touchButton.Size; i++)
        for (int j = 0; j < touchButton.Thickness; j++)
        {
            int x = SaturnMath.Modulo(i + touchButton.Position, 60);
            int y = Mathf.Min(j + touchButton.Depth, 7);

            data[y, x] = touchButton.LedColor;
        }

        return data;
    }
}
}
