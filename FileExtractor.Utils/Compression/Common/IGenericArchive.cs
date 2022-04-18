namespace FileExtractor.Utils.Compression.Common;

public interface IGenericArchive : IDisposable
{
    public IReadOnlyCollection<IGenericArchiveEntry> Entries { get; }
}