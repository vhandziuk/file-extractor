using FileExtractor.Common.Logging;
using FileExtractor.Common.Threading;
using FileExtractor.Utils.Compression.Common;

namespace FileExtractor.Utils.Compression.Rar;

public sealed class RarFileExtractor : GenericArchiveExtractorBase, IRarFileExtractor
{
    public RarFileExtractor(
        IFileSystemUtils fileSystemUtils,
        IRarFileUtils rarFileUtils,
        ITaskRunner taskRunner,
        ILogger<RarFileExtractor> logger)
        : base(
            fileSystemUtils,
            rarFileUtils,
            taskRunner,
            logger)
    {
    }
}