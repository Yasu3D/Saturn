using UnityEngine;

namespace SaturnGame.LED
{
/// <summary>
/// Provides a framework to draw an animation on the LED ring.
/// Subclasses must implement DrawFrame(), which will give the frame at the given index, e.g. by reading from an SCA.
/// Can either be one frame at framerate 0, or an animation with a user-definable framerate and the option to loop.
/// </summary>
public abstract class AnimatedDrawable : LedDrawable
{
    public bool Playing { get; private set; }
    public bool Loop;
    public int Framerate;

    private float startTime;

    protected abstract int FrameCount { get; }
    protected abstract Color[,] DrawFrame(int frameNum);

    public override Color[,] Draw()
    {
        if (!Playing) return null;

        int frame = (int)((Time.time - startTime) * Framerate);

        if (frame >= FrameCount)
        {
            if (Loop)
            {
                // Calculate how far into the new animation we are.
                // Note that this is also robust to a frame drop skipping an entire animation loop (or multiple).
                float animationLength = (1f * FrameCount / Framerate);
                float totalElapsedTime = (Time.time - startTime);
                float elapsedTimeCurrentLoop = totalElapsedTime % animationLength;
                startTime = Time.time - elapsedTimeCurrentLoop;
                frame = (int)(elapsedTimeCurrentLoop * Framerate);
            }
            else
            {
                Stop();
                return null;
            }
        }

        return DrawFrame(frame);
    }

    public virtual void Play()
    {
        Playing = true;
        startTime = Time.time;
    }

    public virtual void Stop()
    {
        Playing = false;
    }

    private void OnValidate()
    {
        Framerate = Mathf.Max(Framerate, 0);
    }
}
}
