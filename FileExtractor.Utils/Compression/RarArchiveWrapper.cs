using SharpCompress.Archives.Rar;

namespace FileExtractor.Utils.Compression;

internal sealed class RarArchiveWrapper : IRarArchive
{
    private readonly RarArchive _rarArchive;
    public RarArchiveWrapper(RarArchive rarArchive) =>
        _rarArchive = rarArchive;

    public IReadOnlyCollection<IRarArchiveEntry> Entries =>
        _rarArchive.Entries.Select(entry => new RarArchiveEntryWrapper(entry)).ToArray();

    public void Dispose() => _rarArchive?.Dispose();
}