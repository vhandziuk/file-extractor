using SharpCompress.Common;

namespace FileExtractor.Utils.Compression.Common;

internal abstract class SharpCompressEntryWrapperBase : IGenericArchiveEntry
{
    private readonly string _key;
    private readonly DateTime? _lastModifiedTime;

    // The entry is read here because a reader-based entry changes once the reader moves on.
    protected SharpCompressEntryWrapperBase(IEntry entry)
    {
        // single-file compressed archives (e.g. .gz) may not store an entry name
        _key = entry.Key ?? string.Empty;
        _lastModifiedTime = entry.LastModifiedTime;
    }

    public string Name => Path.GetFileName(_key);

    public string FullName => _key;

    // SharpCompress's WriteToFile rejects every destination that goes through a reparse point and has no
    // option to allow it, which would defeat --follow-symlinks for these archive types only. The link
    // policy is applied by ArchiveEntryExtractor for all archive types, so the file is opened here.
    public void ExtractToFile(string destinationFileName, bool overwrite)
    {
        using var stream = new FileStream(
            destinationFileName,
            overwrite ? FileMode.Create : FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None);
        WriteTo(stream);

        // flushed first, so the buffered write cannot update the time again
        stream.Flush();
        if (_lastModifiedTime is { } lastModifiedTime)
            File.SetLastWriteTime(stream.SafeFileHandle, lastModifiedTime);
    }

    protected abstract void WriteTo(Stream stream);
}
