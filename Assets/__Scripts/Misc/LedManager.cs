using System.Linq;
using SaturnGame.UI;
using UnityEngine;
using USBIntLEDDll;

namespace SaturnGame
{
public class LedManager : PersistentSingleton<LedManager>
{
    [SerializeField] private RingDebugManager ringDebugManager;

    // Note: index into the array by [angle * 8 + depth] - may change if this is not what USBIntLED uses.
    // angle is the same as anglePos, depth is 2x depthPos (there are 8 depths, two per segment)
    private readonly Color32[] ledState = new Color32[480];

    private readonly LedData ledData;

    public LedManager()
    {
        ledData = new LedData
        {
            unitCount = 60 * 8,
            rgbaValues = new Color32[480],
        };
    }

    private void Start()
    {
        int? retVal = USBIntLED.Safe_USBIntLED_Init();
        Debug.Log($"usb initialization returned: {retVal}");
    }

    /// <summary>
    /// Sets the LEDs to represent the given touch state.
    /// Any touched segments are white, and any untouched segments are black.
    /// </summary>
    /// <param name="touchState"></param>
    public void SetLedsFromTouchState(TouchState touchState)
    {
        for (int anglePos = 0; anglePos < 60; anglePos++)
        for (int depthPos = 0; depthPos < 4; depthPos++)
        {
            ledState[anglePos * 8 + 2 * depthPos] = ledState[anglePos * 8 + 2 * depthPos + 1] =
                touchState.IsPressed(anglePos, depthPos) ? Color.white : Color.black;
        }
    }

    private void SetLedsFromTouchables()
    {
        // init all to black
        for (int anglePos = 0; anglePos < 60; anglePos++)
        for (int depthLedPos = 0; depthLedPos < 8; depthLedPos++)
            ledState[anglePos * 8 + depthLedPos] = Color.black;

        foreach (ITouchable touchable in TouchRegistry.RegisteredTouchables)
        {
            // for each touchable, write its color onto its touchable region on the ring
            for (int offset = touchable.Position; offset < touchable.Position + touchable.Size; offset++)
            for (int depthPos = touchable.MinDepthPos; depthPos <= touchable.MaxDepthPos; depthPos++)
            {
                int anglePos = SaturnMath.Modulo(offset, 60);
                ledState[anglePos * 8 + depthPos * 2] = ledState[anglePos * 8 + depthPos * 2 + 1] = touchable.LedColor;
            }
        }
    }

    private void Update()
    {
        // Toggle RingDebug when F2 is pressed
        if (Input.GetKeyDown(KeyCode.F2)) ringDebugManager.ToggleVisibility();

        if (TouchRegistry.RegisteredTouchables.Any())
        {
            // We are in a menu, recalculate led state
            SetLedsFromTouchables();
        }

        // write to LEDs
        // LedData 0 is anglepos 45, then LedData is increasing CW (in the negative direction)
        for (int ledDataAnglePos = 0; ledDataAnglePos < 60; ledDataAnglePos++)
        for (int depthLedPos = 0; depthLedPos < 8; depthLedPos++)
        {
            int anglePos = SaturnMath.Modulo(44 - ledDataAnglePos, 60);
            ledData.rgbaValues[ledDataAnglePos * 8 + depthLedPos] = ledState[anglePos * 8 + depthLedPos];
        }
        USBIntLED.Safe_USBIntLED_set(0, ledData);

        if (ringDebugManager != null && ringDebugManager.isActiveAndEnabled)
            ringDebugManager.UpdateColors(ledState);
    }
}
}
