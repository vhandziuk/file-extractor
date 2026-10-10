namespace FileExtractor.Utils.Compression;

public sealed class ArchiveFinder(
    IFileSystemUtils fileSystemUtils,
    IArchiveTypeResolver archiveTypeResolver) : IArchiveFinder
{
    private const string AllFilesPattern = "*.*";

    private static readonly char[] _wildcards = ['*', '?'];

    public string[] FindArchives(string path, bool recursive)
    {
        var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

        if (fileSystemUtils.DirectoryExists(path))
            return FilterArchives(fileSystemUtils.GetFiles(path, AllFilesPattern, searchOption));

        if (IsWildcardPath(path))
            return FindMatchingArchives(Path.GetFullPath(path), searchOption);

        return fileSystemUtils.FileExists(path) ? FilterArchives([path]) : [];
    }

    private static bool IsWildcardPath(string path) =>
        Path.GetFileName(path).IndexOfAny(_wildcards) >= 0;

    // Only the last segment may contain wildcards; a pattern such as "*.zip" alone is relative to the current folder.
    private string[] FindMatchingArchives(string fullPath, SearchOption searchOption)
    {
        var directory = Path.GetDirectoryName(fullPath);
        if (directory is null || !fileSystemUtils.DirectoryExists(directory))
            return [];

        return FilterArchives(fileSystemUtils.GetFiles(directory, Path.GetFileName(fullPath), searchOption));
    }

    // Windows matches a three-letter extension pattern such as "*.zip" against longer extensions (".zipx") too,
    // so the matches are filtered by their type as well.
    private string[] FilterArchives(IEnumerable<string> filePaths) =>
        [.. filePaths.Where(filePath => archiveTypeResolver.TryResolve(filePath, out _))];
}
