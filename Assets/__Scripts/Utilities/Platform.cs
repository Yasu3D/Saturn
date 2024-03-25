using System;
using JetBrains.Annotations;

namespace SaturnGame
{
    /// <summary>
    /// Platform provides utilities related to filesystem and other OS-specific tooling.
    /// </summary>
    public static class Platform
    {
        [NotNull]
        public static Uri PathToWebUri([NotNull] string absolutePath)
        {
            // Note: Windows should get a path like "file:///C:/path/to/file.mp3"
            // Linux should get a path like "file:///path/to/file.mp3"
            // Naive ("file:///" + path) does not work because the URI will start with "file:////" (4 /) on linux
            // Warning: this code may break on relative paths.
            string pathWithoutLeadingSlash = absolutePath[0] == '/' ? absolutePath.Substring(1) : absolutePath;

            // Re: choice of Uri.EscapeDataString:
            // - previously we used HttpUtility.UrlEncode, but this encodes spaces as '+', which (for some reason)
            //   causes problems on Linux if there are no other special characters in the string.
            // - WebUtility.UrlEncode has the same problem as HttpUtility.UrlEncode.
            // - UriEscapeDataString encodes spaces as %20. It allows some characters that are illegal in Windows
            //   filesystems, but that seems like the caller's problem.
            // See also https://stackoverflow.com/questions/575440/url-encoding-using-c-sharp/16894322
            // God help me if we find more cases where this breaks.
            return new Uri("file:///" + Uri.EscapeDataString(pathWithoutLeadingSlash));
        }
    }
}
