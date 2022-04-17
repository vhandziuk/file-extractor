using SharpCompress.Archives.Rar;

namespace FileExtractor.Utils.Compression.Rar;

public sealed class RarFileWrapper : IRarFile
{
    public IRarArchive OpenRead(string archiveFileName) =>
        new RarArchiveWrapper(RarArchive.Open(archiveFileName));
}