using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace FileExtractor.Common.Security;

[SupportedOSPlatform("windows")]
public sealed class DllSearchPathHardener : IDllSearchPathHardener
{
    // Application directory, System32 and AddDllDirectory paths. System32 alone would also block the
    // runtime's own native libraries (e.g. System.IO.Compression.Native) in the framework directory.
    private const uint LoadLibrarySearchDefaultDirs = 0x00001000;

    public void Harden()
    {
        // SetDllDirectory("") also covers LoadLibraryEx calls with LOAD_WITH_ALTERED_SEARCH_PATH,
        // which use the standard search order instead of the default directories.
        if (!SetDefaultDllDirectories(LoadLibrarySearchDefaultDirs) || !SetDllDirectory(string.Empty))
            throw new Win32Exception();
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetDefaultDllDirectories(uint directoryFlags);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool SetDllDirectory(string pathName);
}
