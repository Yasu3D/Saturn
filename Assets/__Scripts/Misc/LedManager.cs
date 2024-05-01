using System.Linq;
using System.Threading;
using SaturnGame.UI;
using UnityEngine;
using USBIntLEDDll;

namespace SaturnGame
{
public class LedManager : PersistentSingleton<LedManager>
{
    [SerializeField] private bool useNativeLedImplementation;
    [SerializeField] [Range(0, 1)] private float ledBrightness;
    [SerializeField] private RingDebugManager ringDebugManager;

    private NativeLedOutput nativeLedOutput;

    // Note: index into the array by [angle * 8 + depth] - may change if this is not what USBIntLED uses.
    // angle is the same as anglePos, depth is 2x depthPos (there are 8 depths, two per segment)
    private readonly Color32[] ledState = new Color32[480];

    private readonly LedData ledData = new()
    {
        unitCount = 60 * 8,
        rgbaValues = new Color32[480],
    };
    private bool sendingLedData;

    private void Start()
    {
        if (useNativeLedImplementation)
        {
            nativeLedOutput = new NativeLedOutput();
            nativeLedOutput.Init();
        }
        else
            USBIntLED.Safe_USBIntLED_Init();
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

    private byte AdjustBrightness(byte value)
    {
        return (byte)(value * ledBrightness);
    }

    private async void SetCabLeds()
    {
        // makeshift lock - we only grab the lock while on the main thread, and we only check it from the main thread
        // So I think it should be safe from race conditions.
        sendingLedData = true;

        try
        {
            // write to LEDs
            // LedData 0 is anglePos 45, then LedData is increasing CW (in the negative direction)
            for (int ledDataAnglePos = 0; ledDataAnglePos < 60; ledDataAnglePos++)
            for (int depthLedPos = 0; depthLedPos < 8; depthLedPos++)
            {
                int anglePos = SaturnMath.Modulo(44 - ledDataAnglePos, 60);
                ledData.rgbaValues[ledDataAnglePos * 8 + depthLedPos] = new Color32(
                    AdjustBrightness(ledState[anglePos * 8 + depthLedPos].r),
                    AdjustBrightness(ledState[anglePos * 8 + depthLedPos].g),
                    AdjustBrightness(ledState[anglePos * 8 + depthLedPos].b), 0);
            }

            await Awaitable.BackgroundThreadAsync();

            if (useNativeLedImplementation)
                nativeLedOutput.SetLeds(ledData.rgbaValues);
            else
                USBIntLED.Safe_USBIntLED_set(0, ledData);

            // wait for LED reset low period (at least 280 microseconds)
            // 0.5ms (500 us) is still a bit unstable - jk idk i think this was fucked up
            Thread.Sleep(1);
        }
        finally
        {
            sendingLedData = false;
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

        if (!sendingLedData)
            SetCabLeds();

        if (ringDebugManager != null && ringDebugManager.isActiveAndEnabled)
            ringDebugManager.UpdateColors(ledState);
    }

    private void OnDisable()
    {
        nativeLedOutput?.Destroy();
    }
}
}
