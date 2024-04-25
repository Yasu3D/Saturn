using System;
using System.Runtime.InteropServices;
using OperatingSystem = FTD2XX_NET.Platform.OperatingSystem;

namespace SaturnPlatform
{
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
            OperatingSystem.Windows => throw new NotImplementedException(),
            OperatingSystem.Linux => LinuxPlatformFuncs.dlopen(name, LinuxPlatformFuncs.RTLD_NOW),
            OperatingSystem.OSX => throw new NotImplementedException(),
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    public IntPtr GetSymbol(IntPtr libraryHandle, string symbolName)
    {
        return OperatingSystem switch
        {
            OperatingSystem.Windows => throw new NotImplementedException(),
            OperatingSystem.Linux => LinuxPlatformFuncs.dlsym(libraryHandle, symbolName),
            OperatingSystem.OSX => throw new NotImplementedException(),
            _ => throw new ArgumentOutOfRangeException(),
        };    }

    public int FreeLibrary(IntPtr libraryHandle)
    {
        return OperatingSystem switch
        {
            OperatingSystem.Windows => throw new NotImplementedException(),
            OperatingSystem.Linux => LinuxPlatformFuncs.dlclose(libraryHandle),
            OperatingSystem.OSX => throw new NotImplementedException(),
            _ => throw new ArgumentOutOfRangeException(),
        };    }


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


}
}
