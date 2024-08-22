using UnityEngine;

namespace SaturnGame.LED
{
public class TouchRippleDrawable : AnimatedDrawable
{
    public int TouchAnglePos;
    public int TouchDepthPos;
    public TouchRipplePool Pool;
    protected override int FrameCount => Pool.Frames.Count;

    private readonly Color[,] data = new Color[8, 60];

    protected override Color[,] DrawFrame(int frameNum)
    {
        LedCompositor.ClearCanvas(data, Color.clear);

        const int offsetX = -4;
        const int offsetY = -6;

        for (int i = 0; i < 14; i++)
        for (int j = 0; j < 9; j++)
        {
            int y = i + offsetY + TouchDepthPos * 2;
            int x = SaturnMath.Modulo(j + offsetX + TouchAnglePos, 60);

            if (y is > 7 or < 0) continue;

            data[y, x] = Pool.Frames[frameNum].Colors[i, j];
        }

        return data;
    }

    public override void Stop()
    {
        base.Stop();
        Pool.ReleaseObject(this);
    }

    // Set default framerate
    private void Reset()
    {
        Framerate = 60;
    }
}
}
