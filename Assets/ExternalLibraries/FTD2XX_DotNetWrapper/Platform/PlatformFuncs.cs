// Prefer SaturnPlatform.PlatformFuncs

/*
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;

namespace FTD2XX_NET.Platform
{
    public class PlatformFuncs: IPlatformFuncs
    {
        // https://linux.die.net/man/3/dlopen
        // https://code.woboq.org/userspace/glibc/bits/dlfcn.h.html
        const int RTLD_NOW = 0x00002;

        protected IntPtr _libHandle;

        protected delegate IntPtr LoadLibraryDelegate(string libraryPath);
        protected delegate IntPtr DlOpenDelegate(string libraryPath, int flags);
        protected delegate IntPtr GetSymbolDelegate(IntPtr libraryHandle, string name);
        protected delegate int FreeLibraryDelegate(IntPtr libraryHandle);

        protected LoadLibraryDelegate _loadLibraryFunc;
        protected GetSymbolDelegate _getSymbolFunc;
        protected FreeLibraryDelegate _freeLibraryFunc;

        public PlatformFuncs()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                OperatingSystem = OperatingSystem.Windows;

                // TryLoad ?
                _libHandle = NativeLibrary.Load("kernel32.dll");


                // TryGetExport
                var loadlibp = NativeLibrary.GetExport(_libHandle, "LoadLibraryA");
                _loadLibraryFunc = Marshal.GetDelegateForFunctionPointer<LoadLibraryDelegate>(loadlibp);

                // TryGetExport
                var getsymbolp = NativeLibrary.GetExport(_libHandle, "GetProcAddress");
                _getSymbolFunc = Marshal.GetDelegateForFunctionPointer<GetSymbolDelegate>(getsymbolp);

                // TryGetExport
                var freelibraryp = NativeLibrary.GetExport(_libHandle, "FreeLibrary");
                _freeLibraryFunc = Marshal.GetDelegateForFunctionPointer<FreeLibraryDelegate>(freelibraryp);

            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                OperatingSystem = OperatingSystem.Linux;

                if (!NativeLibrary.TryLoad("libdl.so"  , out _libHandle) && 
                    !NativeLibrary.TryLoad("libdl.so.2", out _libHandle))
                {
                    throw new Exception("Can't load neither libdl.so nor libdl.so.2");
                }


                // TryGetExport
                var loadlibp = NativeLibrary.GetExport(_libHandle, "dlopen");
                var loadlibpf = Marshal.GetDelegateForFunctionPointer<DlOpenDelegate>(loadlibp);
                _loadLibraryFunc = (path) => loadlibpf(path, RTLD_NOW);

                // TryGetExport
                var getsymbolp = NativeLibrary.GetExport(_libHandle, "dlsym");
                _getSymbolFunc = Marshal.GetDelegateForFunctionPointer<GetSymbolDelegate>(getsymbolp);

                // TryGetExport
                var freelibraryp = NativeLibrary.GetExport(_libHandle, "dlclose");
                _freeLibraryFunc = Marshal.GetDelegateForFunctionPointer<FreeLibraryDelegate>(freelibraryp);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                OperatingSystem = OperatingSystem.OSX;
                throw new NotImplementedException("Dynamic library loading on OSX is not implemented");
            }
            else
            {
                throw new NotImplementedException("Application is running on unknown operation system.");
            }
        }


        public IntPtr LoadLibrary(string name) => _loadLibraryFunc(name);
        public IntPtr GetSymbol(IntPtr libraryHandle, string symbolName) => _getSymbolFunc(libraryHandle, symbolName);
        public int FreeLibrary(IntPtr libraryHandle) => _freeLibraryFunc(libraryHandle);

        public OperatingSystem OperatingSystem { get; protected set; }


        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls


        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                if (_libHandle != null)
                {
                    NativeLibrary.Free(_libHandle);
                }
                _libHandle = IntPtr.Zero;

                _loadLibraryFunc = null;
                _getSymbolFunc = null;
                _freeLibraryFunc = null;

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~PlatformFuncs2()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion


    }
}
*/
