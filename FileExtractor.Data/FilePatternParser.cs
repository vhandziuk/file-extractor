namespace FileExtractor.Data;

public sealed class FilePatternParser : IFilePatternParser
{
    private static readonly char[] _directorySeparators = [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar];

    // the location is compared literally with the end of the entry's folder, so it cannot contain wildcards;
    // a colon would be a drive letter, which an archive path never has
    private static readonly char[] _invalidLocationChars = ['*', '?', ':'];

    public FileInfoData Parse(string pattern)
    {
        var separatorIndex = pattern.LastIndexOfAny(_directorySeparators);
        var name = pattern[(separatorIndex + 1)..];
        var location = separatorIndex < 0 ? string.Empty : pattern[..separatorIndex];

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                $"File pattern '{pattern}' must end with a file name or wildcard, for example 'docs/*.txt'", nameof(pattern));
        }

        if (location.IndexOfAny(_invalidLocationChars) >= 0)
        {
            throw new ArgumentException(
                $"Folder of file pattern '{pattern}' must be a path inside the archive without wildcards or drive letters", nameof(pattern));
        }

        return new FileInfoData(string.Empty, name, NormalizeLocation(location));
    }

    // Entry folders are compared in Windows form, and "/docs" or "./docs" mean the same folder as "docs"
    private static string NormalizeLocation(string location) =>
        string.Join(
            Path.DirectorySeparatorChar,
            location.Split(_directorySeparators).Where(segment => segment is not ("" or ".")));
}
