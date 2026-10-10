namespace FileExtractor.Common.Security;

internal interface IMediumIntegrityFileSystem
{
    /// <summary>
    /// Creates <paramref name="path"/> while impersonating a Medium integrity copy of the process token,
    /// the level Windows Explorer runs at, even when the process is elevated.
    /// Falls back to a plain <see cref="Directory.CreateDirectory(string)"/> when the token cannot be prepared or impersonated.
    /// </summary>
    void CreateDirectory(string path);

    /// <summary>
    /// Stamps <paramref name="path"/> with the inheritable mandatory label <c>S:(ML;OICI;NW;;;ME)</c>
    /// (Medium, No-Write-Up), so files created in it later, also by an elevated run, stay at Medium integrity.
    /// Best-effort: returns <see langword="false"/> when the label cannot be written.
    /// </summary>
    bool ApplyLabel(string path);
}
