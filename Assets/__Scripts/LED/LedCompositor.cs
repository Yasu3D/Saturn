using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace SaturnGame.LED
{
public class LedCompositor : MonoBehaviour
{
    public List<LedDrawable> LedDrawables;
    public Color BaseColor = Color.clear;
    private readonly Color[,] ledValues = new Color[8, 60];

    [SerializeField] private bool useNativeLedImplementation;
    [SerializeField] [Range(0, 1)] private float ledBrightness;

    [SerializeField] private Color32[,] ledValues = new Color32[8,60];

    private readonly LedData ledData = new()
    {
        unitCount = 60 * 8,
        rgbaValues = new Color32[480],
    };

    // TODO: needs to be volatile?
    private bool sendingLedData;

    private NativeLedOutput nativeLedOutput;

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

    private byte AdjustBrightness(byte value)
    {
        return (byte)(value * ledBrightness);
    }

    private void ClearCanvas(Color32 color)
    {
        for (int i = 0; i < 8; i++)
        for (int j = 0; j < 60; j++)
            values[i, j] = color;
    }

    [CanBeNull]
    public Color[,] Draw()
    {
        // Fill all LEDs with the base color first
        ClearCanvas(ledValues, BaseColor);

        // TODO: presort list, avoid linq (memory alloc)
        foreach (LedDrawable drawable in LedDrawables.Where(x => x.isActiveAndEnabled).OrderBy(x => x.Layer))
        {
            Color[,] layer = drawable.Draw();
            if (layer == null) continue;

            if (layer.GetLength(0) != ledValues.GetLength(0) || layer.GetLength(1) != ledValues.GetLength(1))
                throw new($"Invalid layer size from {drawable.GetType().Name}");

            for (int i = 0; i < 8; i++)
            for (int j = 0; j < 60; j++)
            {
                // https://en.wikipedia.org/wiki/Alpha_compositing

                Color bottom = ledValues[i, j];
                Color top = layer[i, j];

                float newAlpha = top.a + bottom.a * (1f - top.a);

                // Avoid division by zero.
                if (newAlpha == 0f)
                {
                    ledValues[i, j] = Color.clear;
                    continue;
                }

                // Calculate the RGB. Note, this will also set alpha, but we will overwrite that.
                ledValues[i, j] = (top * top.a + bottom * bottom.a * (1f - top.a)) / newAlpha;

                ledValues[i, j].a = newAlpha;
            }
        }

        return ledValues;
    }
}
}
