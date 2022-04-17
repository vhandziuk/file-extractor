using SharpCompress.Archives.Rar;

namespace FileExtractor.Utils.Compression;

public sealed class RarArchiveUtils : IRarArchiveUtils
{
    private readonly IRarFile _rarFile;

    public RarArchiveUtils(IRarFile rarFile) =>
        _rarFile = rarFile;

    public IRarArchive OpenRead(string archiveFileName) =>
        _rarFile.Open(archiveFileName);
}