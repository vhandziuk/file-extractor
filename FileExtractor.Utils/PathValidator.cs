namespace FileExtractor.Utils;

public sealed class PathValidator : IPathValidator
{
    private static readonly char[] _invalidFileNameChars = Path.GetInvalidFileNameChars();
    private static readonly char[] _directorySeparators = [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar];

    // Windows opens a device instead of a file for these names, in any folder and with any extension
    private static readonly HashSet<string> _reservedDeviceNames = new(
        [
            "CON", "PRN", "AUX", "NUL",
            .. Enumerable.Range(1, 9).Select(number => $"COM{number}"),
            .. Enumerable.Range(1, 9).Select(number => $"LPT{number}")
        ],
        StringComparer.OrdinalIgnoreCase);

    public bool IsContained(string rootPath, string path)
    {
        var fullRootPath = Path.GetFullPath(rootPath);
        var fullPath = Path.GetFullPath(path);

        return !HasStreamSeparator(fullPath)
            && (string.Equals(Path.TrimEndingDirectorySeparator(fullPath), Path.TrimEndingDirectorySeparator(fullRootPath), StringComparison.OrdinalIgnoreCase)
                || fullPath.StartsWith(WithTrailingSeparator(fullRootPath), StringComparison.OrdinalIgnoreCase));
    }

    // Windows silently strips a trailing dot or space, so such a name (including "." and "..") would be
    // written as a different file than the one that was checked.
    public bool IsValidFileName(string fileName) =>
        !string.IsNullOrEmpty(fileName)
        && fileName.IndexOfAny(_invalidFileNameChars) < 0
        && !fileName.EndsWith('.')
        && !fileName.EndsWith(' ')
        && !IsReservedDeviceName(fileName);

    // An empty segment ("a\\b", a leading separator) or a rooted path would not be combined below the folder as written.
    public bool IsValidRelativePath(string path) =>
        path.Length == 0
        || (!Path.IsPathRooted(path) && path.Split(_directorySeparators).All(IsValidFileName));

    // "file.txt:stream" writes to an alternate data stream; the only valid colon is the drive's
    private static bool HasStreamSeparator(string fullPath) =>
        fullPath.IndexOf(':', Path.GetPathRoot(fullPath)?.Length ?? 0) >= 0;

    private static string WithTrailingSeparator(string path) =>
        Path.EndsInDirectorySeparator(path) ? path : path + Path.DirectorySeparatorChar;

    private static bool IsReservedDeviceName(string fileName)
    {
        var dotIndex = fileName.IndexOf('.');
        var baseName = dotIndex < 0 ? fileName : fileName[..dotIndex];

        return _reservedDeviceNames.Contains(baseName.TrimEnd());
    }
}
