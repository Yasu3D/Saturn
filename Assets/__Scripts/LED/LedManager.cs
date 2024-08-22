using System.Threading;
using SaturnGame.LED;
using SaturnGame.UI;
using UnityEngine;
using USBIntLEDDll;

namespace SaturnGame.LED
{
public class LedManager : PersistentSingleton<LedManager>
{
    // BaseLedCompositor is the ultimate compositor that will be used to get the actual LED values.
    public LedCompositor BaseLedCompositor;
    [SerializeField] private bool useNativeLedImplementation;
    private NativeLedOutput nativeLedOutput;
    [SerializeField] private RingDebugManager ringDebugManager;

    [SerializeField] [Range(0, 1)] private float ledBrightness;

    private readonly LedData ledData = new()
    {
        unitCount = 60 * 8,
        rgbaValues = new Color32[480],
    };

    private void Start()
    {
        if (useNativeLedImplementation)
        {
            nativeLedOutput = new();
            nativeLedOutput.Init();
        }
        else
            USBIntLED.Safe_USBIntLED_Init();
    }
    private async void FixedUpdate()
    {
        Color[,] ledValues = BaseLedCompositor.Draw();
        if (ringDebugManager != null) ringDebugManager.UpdateColors(ledValues);

        await SetCabLeds(ledValues);
    }

    private volatile bool sendingLedData;

    private async Awaitable SetCabLeds(Color[,] ledValues)
    {
        // makeshift lock - we only grab the lock while on the main thread, and we only check it from the main thread
        // So I think it should be safe from race conditions.
        if (sendingLedData) return;
        sendingLedData = true;

        try
        {
            // write to LEDs

            // LedData 0 is anglePos 44, then LedData is increasing CW (in the negative direction)
            for (int ledDataAnglePos = 0; ledDataAnglePos < 60; ledDataAnglePos++)
            for (int depthLedPos = 0; depthLedPos < 8; depthLedPos++)
            {
                int anglePos = SaturnMath.Modulo(44 - ledDataAnglePos, 60);
                ledData.rgbaValues[ledDataAnglePos * 8 + depthLedPos] = new Color(
                    ledValues[depthLedPos, anglePos].r * ledBrightness,
                    ledValues[depthLedPos, anglePos].g * ledBrightness,
                    ledValues[depthLedPos, anglePos].b * ledBrightness,
                    0);
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


    protected override async void OnApplicationQuit()
    {
        base.OnApplicationQuit();

        // default values are 0, 0, 0, 0, which should draw as black
        Color[,] black = new Color[8, 60];
        await SetCabLeds(black);
    }
}
}
