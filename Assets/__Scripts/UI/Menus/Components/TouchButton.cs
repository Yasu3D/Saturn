using UnityEngine;
using UnityEngine.UI;

namespace SaturnGame.UI
{
/// <summary>
/// TouchButton is a Button that can also be touched using the touch ring.
/// </summary>
[AddComponentMenu("TouchButton")]
public class TouchButton : Button, ITouchable
{
    [SerializeField] private int position;
    public int Position => position;
    [SerializeField] private int size;
    public int Size => size;
    // it's assumed that buttons always occupy depthPos 2-3

    private bool isPressed;

    protected override void OnEnable()
    {
        base.OnEnable();
        TouchRegistry.RegisterTouchable(this);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        TouchRegistry.UnregisterTouchable(this);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        TouchRegistry.UnregisterTouchable(this);
    }

    public void OnTouchPress()
    {
        if (!IsActive() || !IsInteractable())
            return;

        isPressed = true;
        DoStateTransition(SelectionState.Pressed, false);
    }

    public void OnTouchRelease()
    {
        if (!isPressed || !IsActive() || !IsInteractable())
            return;

        UISystemProfilerApi.AddMarker("Button.onClick", this);
        onClick.Invoke();
        DoStateTransition(currentSelectionState, false);
    }
}
}
