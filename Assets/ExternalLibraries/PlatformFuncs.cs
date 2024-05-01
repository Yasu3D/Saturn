using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using OperatingSystem = FTD2XX_NET.Platform.OperatingSystem;

namespace SaturnPlatform
{
[UsedImplicitly]
public class PlatformFuncs : FTD2XX_NET.Platform.IPlatformFuncs
{
    public PlatformFuncs()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            OperatingSystem = OperatingSystem.Windows;
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            OperatingSystem = OperatingSystem.Linux;
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            OperatingSystem = OperatingSystem.OSX;
            throw new NotImplementedException("Dynamic library loading on OSX is not implemented");
        }
        else
            throw new NotImplementedException("Application is running on unknown operation system.");
    }

    public OperatingSystem OperatingSystem { get; }

    public IntPtr LoadLibrary(string name)
    {
        return OperatingSystem switch
        {
            OperatingSystem.Windows => WindowsPlatformFuncs.LoadLibraryA(name),
            OperatingSystem.Linux => LinuxPlatformFuncs.dlopen(name, LinuxPlatformFuncs.RTLD_NOW),
            OperatingSystem.OSX => throw new NotImplementedException(),
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    public IntPtr GetSymbol(IntPtr libraryHandle, string symbolName)
    {
        return OperatingSystem switch
        {
            OperatingSystem.Windows => WindowsPlatformFuncs.GetProcAddress(libraryHandle, symbolName),
            OperatingSystem.Linux => LinuxPlatformFuncs.dlsym(libraryHandle, symbolName),
            OperatingSystem.OSX => throw new NotImplementedException(),
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    public int FreeLibrary(IntPtr libraryHandle)
    {
#if UNITY_EDITOR_LINUX
        // Avoid actually running free in the editor. We may still be writing LED data in another thread,
        // which can cause an editor segfault. TODO: see if this also affect windows, etc.
        // This will technically leak the loaded library but it should be deduplicated anyway.
        return 0;
#endif

#pragma warning disable CS0162 // Unreachable code detected
        // ReSharper disable HeuristicUnreachableCode
        int ret = OperatingSystem switch
        {
            OperatingSystem.Windows => WindowsPlatformFuncs.FreeLibrary(libraryHandle),
            OperatingSystem.Linux => LinuxPlatformFuncs.dlclose(libraryHandle),
            OperatingSystem.OSX => throw new NotImplementedException(),
            _ => throw new ArgumentOutOfRangeException(),
        };

        // Since FTD2XX_NET.cs doesn't properly check the return value, do it here.
        if (ret != 0) throw new Exception($"Failed to free library {libraryHandle}");

        return ret;
        // ReSharper restore HeuristicUnreachableCode
#pragma warning restore CS0162 // Unreachable code detected
    }


    public void Dispose()
    {
        // Don't need to do anything.
    }

    private static class LinuxPlatformFuncs
    {
        // ReSharper disable InconsistentNaming, IdentifierTypo, CommentTypo, StringLiteralTypo

        // See dlopen(3)
        public const int RTLD_NOW = 0x00002;

        // See dlopen(3)
        [DllImport("libdl.so.2")]
        public static extern IntPtr dlopen(string filename, int flags);

        // See dlsym(3)
        [DllImport("libdl.so.2")]
        public static extern IntPtr dlsym(IntPtr handle, string symbol);

        // See dlopen(3)
        [DllImport("libdl.so.2")]
        public static extern int dlclose(IntPtr libraryHandle);

        // ReSharper restore InconsistentNaming, IdentifierTypo, CommentTypo, StringLiteralTypo
    }

    private static class WindowsPlatformFuncs
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibraryA(string name);


        [DllImport("kernel32.dll")]
        public static extern IntPtr GetProcAddress(IntPtr libraryHandle, string symbolName);


        [DllImport("kernel32.dll")]
        public static extern int FreeLibrary(IntPtr libraryHandle);
    }
}
}
