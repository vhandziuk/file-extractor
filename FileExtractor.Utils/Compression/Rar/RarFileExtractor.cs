using System.Collections.Concurrent;
using FileExtractor.Common.Logging;
using FileExtractor.Common.Threading;
using FileExtractor.Data;
using FileExtractor.Utils.Compression.Common;
using FileExtractor.Utils.FileSystem;

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