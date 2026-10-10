using System.IO.Enumeration;
using FileExtractor.Data;

namespace FileExtractor.Utils.Compression.Common;

public sealed class ArchiveEntryMatcher : IArchiveEntryMatcher
{
    // An entry without a name (a directory, or a single-file archive that stores no name)
    // cannot be written to a file, even when the configured name is "*".
    public bool IsMatch(FileInfoData fileInfo, string entryName, string entryFullName) =>
        !string.IsNullOrEmpty(entryName)
        && FileSystemName.MatchesSimpleExpression(fileInfo.Name, entryName)
        && IsInLocation(fileInfo.Location, entryFullName);

    // A blank location matches the entry in any folder of the archive; otherwise the entry's
    // parent path has to end with the configured location.
    private static bool IsInLocation(string location, string entryFullName) =>
        string.IsNullOrWhiteSpace(location)
        || Path.GetDirectoryName(entryFullName)?.EndsWith(location, StringComparison.OrdinalIgnoreCase) == true;
}
