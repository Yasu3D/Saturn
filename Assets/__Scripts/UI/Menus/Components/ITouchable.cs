using UnityEngine;
using UnityEngine.EventSystems;

namespace SaturnGame.UI
{
public interface ITouchable : IEventSystemHandler
{
    public int Position { get; }

    public int Size { get; }

    // Min depthPos of the touchable area, inclusive
    public int MinDepthPos { get; }

    // Max depthPos of the touchable area, inclusive
    public int MaxDepthPos { get; }

    public Color32 LedColor { get; }

    public bool Touched(TouchState touchState)
    {
        for (int offset = Position; offset < Position + Size; offset++)
        for (int depthPos = MinDepthPos; depthPos <= MaxDepthPos; depthPos++)
        {
            if (touchState.IsPressed(SaturnMath.Modulo(offset, 60), depthPos))
                return true;
        }

        return false;
    }

    void OnTouchPress();
    void OnTouchRelease();
}
}
