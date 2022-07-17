using FileExtractor.Common.Logging;
using FileExtractor.Common.Threading;
using FileExtractor.Utils.Compression.Common;

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