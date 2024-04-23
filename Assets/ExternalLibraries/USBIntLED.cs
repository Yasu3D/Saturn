using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace USBIntLEDDll
{
[StructLayout(LayoutKind.Sequential)]
public struct LedData
{
    public uint unitCount;

    // Unity's Color32 is a struct of 4 bytes, so we can use it directly.
    // 480 = 60 (angle) * 8 (depth)
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 480)]
    public Color32[] rgbaValues;
}

// ReSharper disable once InconsistentNaming
public static class USBIntLED
{
    private static bool dllMissing;

    [DllImport("USBIntLED.DLL")]
    private static extern int USBIntLED_Init();

    public static int? Safe_USBIntLED_Init()
    {
        if (dllMissing)
            return null;

        try
        {
            return USBIntLED_Init();
        }
        catch (DllNotFoundException)
        {
            dllMissing = true;
            return null;
        }
    }

    [DllImport("USBIntLED.DLL")]
    private static extern int USBIntLED_set(int data1, LedData ledData);

    public static int? Safe_USBIntLED_set(int data1, LedData ledData)
    {
        if (dllMissing)
            return null;

        try
        {
            return USBIntLED_set(data1, ledData);
        }
        catch (DllNotFoundException)
        {
            dllMissing = true;
            return null;
        }
    }
}
}
