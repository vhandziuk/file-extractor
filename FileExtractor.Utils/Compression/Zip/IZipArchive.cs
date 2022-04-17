namespace FileExtractor.Utils.Compression.Zip;

public interface IZipArchive : IDisposable
{
    public IReadOnlyCollection<IZipArchiveEntry> Entries { get; }
}