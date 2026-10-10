using FileExtractor.Data;

namespace FileExtractor.Utils.Compression.Common;

/// <summary>
/// Decides whether an archive entry matches a file configured for extraction.
/// Shared by all archive extractors so every archive type applies the same rules.
/// </summary>
public interface IArchiveEntryMatcher
{
    /// <summary>
    /// Returns <see langword="true"/> when the entry name matches the configured name (which may contain
    /// the <c>*</c> and <c>?</c> wildcards) and the entry is located in the configured location.
    /// </summary>
    bool IsMatch(FileInfoData fileInfo, string entryName, string entryFullName);
}
