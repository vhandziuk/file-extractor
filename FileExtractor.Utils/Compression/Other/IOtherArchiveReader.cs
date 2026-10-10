using FileExtractor.Utils.Compression.Common;

namespace FileExtractor.Utils.Compression.Other;

/// <summary>
/// Forward-only reader over the file entries of an archive. Each entry can be extracted only once,
/// while it is the current entry.
/// </summary>
public interface IOtherArchiveReader : IDisposable
{
    IGenericArchiveEntry Entry { get; }

    bool MoveToNextEntry();
}
