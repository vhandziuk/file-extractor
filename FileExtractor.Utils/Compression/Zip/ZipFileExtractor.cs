using FileExtractor.Common.Logging;
using FileExtractor.Common.Threading;
using FileExtractor.Utils.Compression.Common;

namespace FileExtractor.Utils.Compression.Zip;

public sealed class ZipFileExtractor : GenericArchiveExtractorBase, IZipFileExtractor
{
    public ZipFileExtractor(
        IFileSystemUtils fileSystemUtils,
        IZipFileUtils zipFileUtils,
        ITaskRunner taskRunner,
        ILogger<ZipFileExtractor> logger)
        : base(fileSystemUtils, zipFileUtils, taskRunner, logger)
    {
    }
}