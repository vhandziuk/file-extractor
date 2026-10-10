using System.IO.Enumeration;
using FileExtractor.Data;

namespace FileExtractor.Utils.Compression.Common;

/// <summary>
/// Decides whether an archive entry matches a file configured for extraction.
/// Shared by all archive extractors so every archive type applies the same rules.
/// </summary>
public static class ArchiveEntryMatcher
{
    /// <summary>
    /// Returns <see langword="true"/> when the entry name matches the configured name (which may contain
    /// the <c>*</c> and <c>?</c> wildcards) and the entry is located in the configured location.
    /// </summary>
    public static bool IsMatch(FileInfoData fileInfo, string entryName, string entryFullName) =>
        FileSystemName.MatchesSimpleExpression(fileInfo.Name, entryName)
        && IsInLocation(fileInfo.Location, entryFullName);

    // A blank location matches the entry in any folder of the archive; otherwise the entry's
    // parent path has to end with the configured location.
    private static bool IsInLocation(string location, string entryFullName) =>
        string.IsNullOrWhiteSpace(location)
        || Path.GetDirectoryName(entryFullName)?.EndsWith(location, StringComparison.OrdinalIgnoreCase) == true;
}
