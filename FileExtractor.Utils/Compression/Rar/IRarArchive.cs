namespace FileExtractor.Utils.Compression.Rar;

public interface IRarArchive : IDisposable
{
    public IReadOnlyCollection<IRarArchiveEntry> Entries { get; }
}