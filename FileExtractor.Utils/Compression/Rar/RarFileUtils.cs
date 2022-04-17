namespace FileExtractor.Utils.Compression.Rar;

public sealed class RarFileUtils : IRarFileUtils
{
    private readonly IRarFile _rarFile;

    public RarFileUtils(IRarFile rarFile) =>
        _rarFile = rarFile;

    public IRarArchive OpenRead(string archiveFileName) =>
        _rarFile.OpenRead(archiveFileName);
}