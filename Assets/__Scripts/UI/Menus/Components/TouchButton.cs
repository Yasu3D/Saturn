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
    [SerializeField] private int size;

    public int Position => position;
    public int Size => size;
    public int Thickness => (MaxDepthPos - MinDepthPos) * 2;
    public int Depth => MinDepthPos * 2;
    
    // it's assumed that buttons always occupy depthPos 2-3
    public int MinDepthPos => 2;
    public int MaxDepthPos => 3;

    public Color32 LedColor => targetGraphic.canvasRenderer.GetColor();

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
