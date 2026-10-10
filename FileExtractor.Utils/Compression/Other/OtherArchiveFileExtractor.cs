using FileExtractor.Common.Logging;
using FileExtractor.Common.Threading;
using FileExtractor.Data;
using FileExtractor.Utils.Compression.Common;

namespace FileExtractor.Utils.Compression.Other;

public sealed class OtherArchiveFileExtractor : IOtherArchiveFileExtractor
{
    private readonly IFileSystemUtils _fileSystemUtils;
    private readonly IOtherArchiveFile _otherArchiveFile;
    private readonly ITaskRunner _taskRunner;
    private readonly ILogger<OtherArchiveFileExtractor> _logger;

    public OtherArchiveFileExtractor(
        IFileSystemUtils fileSystemUtils,
        IOtherArchiveFile otherArchiveFile,
        ITaskRunner taskRunner,
        ILogger<OtherArchiveFileExtractor> logger)
    {
        _fileSystemUtils = fileSystemUtils;
        _otherArchiveFile = otherArchiveFile;
        _taskRunner = taskRunner;
        _logger = logger;
    }

    public async Task<IEnumerable<FileInfoData>> ExtractFiles(IEnumerable<string> archives, string outputPath, IEnumerable<FileInfoData> fileData) =>
        await _taskRunner.Run(() => ExtractInternal(archives, outputPath, fileData.ToList()));

    private IEnumerable<FileInfoData> ExtractInternal(IEnumerable<string> archives, string outputPath, IReadOnlyCollection<FileInfoData> fileData)
    {
        if (!_fileSystemUtils.DirectoryExists(outputPath))
            _fileSystemUtils.CreateDirectory(outputPath);

        var extractedFiles = new HashSet<FileInfoData>();

        foreach (var archive in archives)
        {
            using var reader = _otherArchiveFile.OpenReader(archive);
            while (reader.MoveToNextEntry())
            {
                var entry = reader.Entry;
                var matchingFiles = fileData
                    .Where(fileInfo => ArchiveEntryMatcher.IsMatch(fileInfo, entry.Name, entry.FullName))
                    .ToList();

                ExtractEntry(entry, outputPath, matchingFiles);
                extractedFiles.UnionWith(matchingFiles);
            }
        }

        return extractedFiles;
    }

    private void ExtractEntry(IGenericArchiveEntry entry, string outputPath, IEnumerable<FileInfoData> matchingFiles)
    {
        string extractedFilePath = null;

        foreach (var fileInfo in matchingFiles)
        {
            var destinationPath = Path.Combine(outputPath, fileInfo.Directory);
            if (!_fileSystemUtils.DirectoryExists(destinationPath))
                _fileSystemUtils.CreateDirectory(destinationPath);

            var destinationFilePath = Path.Combine(destinationPath, entry.Name);
            if (_fileSystemUtils.FileExists(destinationFilePath))
            {
                _logger.Warning("File {File} already exists in {Path}. Skipping extraction", entry.Name, destinationPath);
                continue;
            }

            _logger.Information("Extracting {File} to {Path}", entry.Name, destinationPath);
            extractedFilePath = WriteEntry(entry, destinationFilePath, extractedFilePath);
        }
    }

    // A forward-only reader can read the entry data only once, so every further destination
    // receives a copy of the first extracted file.
    private string WriteEntry(IGenericArchiveEntry entry, string destinationFilePath, string extractedFilePath)
    {
        if (extractedFilePath is not null)
        {
            _fileSystemUtils.Copy(extractedFilePath, destinationFilePath, true);
            return extractedFilePath;
        }

        entry.ExtractToFile(destinationFilePath, overwrite: true);
        return destinationFilePath;
    }
}
