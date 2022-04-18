using FileExtractor.Utils.Compression.Common;

namespace FileExtractor.Utils.Compression.Rar;

public sealed class RarFileUtils : GenericArchiveFileUtilsBase, IRarFileUtils
{
    public RarFileUtils(IRarFile rarFile)
        : base(rarFile)
    {
    }
}