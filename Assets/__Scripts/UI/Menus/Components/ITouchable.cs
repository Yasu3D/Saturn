using UnityEngine.EventSystems;

namespace SaturnGame.UI
{
public interface ITouchable : IEventSystemHandler
{
    public int Position { get; }
    public int Size { get; }

    public bool Touched(TouchState touchState)
    {
        for (int offset = Position; offset < Position + Size; offset++)
        for (int depthPos = 2; depthPos < 4; depthPos++)
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
