using SaturnGame.UI;
using UnityEngine;
using USBIntLEDDll;

namespace SaturnGame
{
public class LedManager : MonoBehaviour
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
            rgbaValues = ledState,
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
        {
            for (int depthPos = 0; depthPos < 4; depthPos++)
            {
                ledState[anglePos * 8 + 2 * depthPos] = ledState[anglePos * 8 + 2 * depthPos + 1] =
                    touchState.IsPressed(anglePos, depthPos) ? Color.white : Color.black;
            }
        }
    }

    private void Update()
    {
        // write to LEDs
        USBIntLED.Safe_USBIntLED_set(0, ledData);

        if (ringDebugManager != null && ringDebugManager.isActiveAndEnabled)
            ringDebugManager.UpdateColors(ledState);
    }
}
}
