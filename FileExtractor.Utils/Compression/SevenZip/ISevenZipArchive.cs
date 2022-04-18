namespace FileExtractor.Utils.Compression.SevenZip;

public interface ISevenZipArchive : IDisposable
{
    public IReadOnlyCollection<ISevenZipArchiveEntry> Entries { get; }
}