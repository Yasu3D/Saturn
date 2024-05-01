using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SaturnGame.UI;
using UnityEngine;
using USBIntLEDDll;

namespace SaturnGame.LED
{
/// <summary>
/// If there's any trouble with LED displaying, check execution order. This must be LAST!
/// </summary>
public class LedCompositor : PersistentSingleton<LedCompositor>
{
    public List<LedDrawable> LedDrawableQueue;
    [SerializeField] private RingDebugManager ringDebugManager;

    [SerializeField] private bool useNativeLedImplementation;
    [SerializeField] [Range(0, 1)] private float ledBrightness;

    [SerializeField] private Color32[] ledValues = new Color32[480]; // 60 * 8

    private readonly LedData ledData = new()
    {
        unitCount = 60 * 8,
        rgbaValues = new Color32[480],
    };

    private bool sendingLedData;

    private NativeLedOutput nativeLedOutput;

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

    private byte AdjustBrightness(byte value)
    {
        return (byte)(value * ledBrightness);
    }

    private async Awaitable SetCabLeds()
    {
        // makeshift lock - we only grab the lock while on the main thread, and we only check it from the main thread
        // So I think it should be safe from race conditions.
        if (sendingLedData) return;
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
                    AdjustBrightness(ledValues[anglePos * 8 + depthLedPos].r),
                    AdjustBrightness(ledValues[anglePos * 8 + depthLedPos].g),
                    AdjustBrightness(ledValues[anglePos * 8 + depthLedPos].b), 0);
            }

            await Awaitable.BackgroundThreadAsync();

            if (useNativeLedImplementation)
                nativeLedOutput.SetLeds(ledData.rgbaValues);
            else
                USBIntLED.Safe_USBIntLED_set(0, ledData);

            // wait for LED reset low period (at least 280 microseconds)
            // 0.5ms (500 us) seemed a bit unstable but this might have actually just been broken.
            // Awaitable.WaitForSecondsAsync only works on the main thread, but it's really slow to switch back to the
            // main thread (takes at least a frame). So just use Thread.Sleep for 1ms and then release the "lock"
            Thread.Sleep(1);
        }
        finally
        {
            sendingLedData = false;
        }
    }

    private async void FixedUpdate()
    {
        // Fill all LEDs with black first
        for (int i = 0; i < ledValues.Length; i++) ledValues[i] = Color.black;

        foreach (LedDrawable drawable in LedDrawableQueue.OrderBy(x => x.Layer))
            drawable.Draw(ref ledValues);

        LedDrawableQueue.Clear();

        // Send data to LED boards / debug display.

        if (ringDebugManager != null) ringDebugManager.UpdateColors(ledValues);

        await SetCabLeds();
    }

    private void Update()
    {
        // Toggle RingDebug when F2 is pressed
        if (Input.GetKeyDown(KeyCode.F2)) ringDebugManager.ToggleVisibility();
    }
}
}
