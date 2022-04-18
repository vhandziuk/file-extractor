using System.Collections.Concurrent;
using FileExtractor.Common.Logging;
using FileExtractor.Common.Threading;
using FileExtractor.Data;
using FileExtractor.Utils.Compression.Common;
using FileExtractor.Utils.FileSystem;

namespace FileExtractor.Utils.Compression.SevenZip;

public sealed class SevenZipFileExtractor : GenericArchiveExtractorBase, ISevenZipFileExtractor
{
    public SevenZipFileExtractor(
        IFileSystemUtils fileSystemUtils,
        ISevenZipFileUtils sevenZipFileUtils,
        ITaskRunner taskRunner,
        ILogger<SevenZipFileExtractor> logger)
        : base(fileSystemUtils, sevenZipFileUtils, taskRunner, logger)
    {
    }
}