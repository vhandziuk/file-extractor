using FileExtractor.Utils.Compression.Common;
using SharpCompress.Archives.Rar;

namespace FileExtractor.Utils.Compression.Rar;

public sealed class RarFileWrapper(ISharpCompressEntryReader entryReader) : IRarFile
{
    public IGenericArchive OpenRead(string archiveFileName) =>
        new RarArchiveWrapper(RarArchive.OpenArchive(archiveFileName), entryReader);
}
