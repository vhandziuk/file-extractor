namespace FileExtractor.Utils.Compression;

internal sealed record ArchiveEntryWrapper : IArchiveEntry
{
    public string Name { get; init; }

    public string FullName { get; init; }

    public void ExtractToFile(string destinationFileName, bool overwrite)
    {
        throw new NotImplementedException();
    }
}