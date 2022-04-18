using FileExtractor.Utils.Compression.Common;
using SharpCompress.Archives.Rar;

namespace FileExtractor.Utils.Compression.Rar;

public sealed class RarFileWrapper : IRarFile
{
    public IGenericArchive OpenRead(string archiveFileName) =>
        new RarArchiveWrapper(RarArchive.Open(archiveFileName));
}