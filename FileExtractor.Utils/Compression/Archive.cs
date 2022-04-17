namespace FileExtractor.Utils.Compression;

internal sealed class ArchiveWrapper : IArchive
{
    public IReadOnlyCollection<IArchiveEntry> Entries => throw new NotImplementedException();

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
