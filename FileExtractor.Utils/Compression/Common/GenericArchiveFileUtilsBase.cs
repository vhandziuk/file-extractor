namespace FileExtractor.Utils.Compression.Common;

public abstract class GenericArchiveFileUtilsBase : IGenericArchiveFileUtils
{
    private readonly IGenericArchiveFile _archiveFile;

    protected GenericArchiveFileUtilsBase(IGenericArchiveFile archiveFile) =>
        _archiveFile = archiveFile;

    public IGenericArchive OpenRead(string archiveFileName) =>
        _archiveFile.OpenRead(archiveFileName);
}