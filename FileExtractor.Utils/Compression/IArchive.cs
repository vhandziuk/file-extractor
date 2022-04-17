namespace FileExtractor.Utils.Compression;

public interface IArchive : IDisposable
{
    public IReadOnlyCollection<IArchiveEntry> Entries { get; }
}