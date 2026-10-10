namespace FileExtractor.Common.Security;

public interface IDllSearchPathHardener
{
    /// <summary>
    /// Removes the current directory and PATH from the native DLL search order of the process,
    /// so a DLL planted next to the processed archives cannot be loaded. Call it once at startup.
    /// </summary>
    void Harden();
}
