namespace FileExtractor.Utils.Compression;

public interface IRarArchive : IDisposable
{
    public IReadOnlyCollection<IRarArchiveEntry> Entries { get; }
}