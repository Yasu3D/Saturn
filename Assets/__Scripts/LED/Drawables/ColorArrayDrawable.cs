using System;
using System.IO;
using UnityEngine;

namespace SaturnGame.LED
{
/// <summary>
/// Draws a .sca file. Can either be one frame at framerate 0, or an animation with a user-definable framerate and the
/// option to loop. (See <see cref="AnimatedDrawable"/>.)
/// </summary>
public class ColorArrayDrawable : AnimatedDrawable
{
    [SerializeField] private string scaPath;
    private ColorArray colorArray;
    protected override int FrameCount => colorArray.Frames.Count;

    private void OnEnable()
    {
        colorArray = ColorArray.Parse(Path.Combine(Application.streamingAssetsPath, scaPath));
        if (colorArray.Width != 60 || colorArray.Depth != 8)
        {
            throw new ArgumentOutOfRangeException("ColorArrayDrawable must have a width of 60 and a depth of 8." +
                                                  $" (Got {colorArray.Width}x{colorArray.Depth}.)");
        }
    }

    protected override Color[,] DrawFrame(int frameNum)
    {
        return colorArray.Frames[frameNum].Colors;
    }

    // for debug
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) Play();
    }

    // Set default framerate when creating a new object.
    private void Reset()
    {
        Framerate = 30;
    }
}
}
