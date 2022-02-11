namespace FileExtractor.Utils.Compression;

public interface IZipArchive : IDisposable
{
    public IReadOnlyCollection<IZipArchiveEntry> Entries { get; }
}