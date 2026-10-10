namespace FileExtractor.Utils.Compression;

public interface IArchiveTypeResolver
{
    /// <summary>
    /// Determines the archive type from the file extension. Returns <see langword="false"/> for
    /// files that are not supported archives.
    /// </summary>
    bool TryResolve(string filePath, out ArchiveType archiveType);
}
