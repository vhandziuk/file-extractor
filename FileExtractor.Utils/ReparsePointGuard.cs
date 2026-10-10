namespace FileExtractor.Utils;

public sealed class ReparsePointGuard(IFileSystemUtils fileSystemUtils) : IReparsePointGuard
{
    // The root is chosen by the user and may legitimately be a link; a link below it could have been planted
    // by anyone who can write there, and creating folders or files through it would write elsewhere.
    public bool ContainsReparsePoint(string rootPath, string path)
    {
        var rootPrefix = WithTrailingSeparator(Path.GetFullPath(rootPath));

        for (var current = Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));
             current is not null && current.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase);
             current = Path.GetDirectoryName(current))
        {
            if (fileSystemUtils.IsReparsePoint(current))
                return true;
        }

        return false;
    }

    private static string WithTrailingSeparator(string path) =>
        Path.EndsInDirectorySeparator(path) ? path : path + Path.DirectorySeparatorChar;
}
