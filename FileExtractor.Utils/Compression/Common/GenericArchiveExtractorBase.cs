using System.IO.Enumeration;
using FileExtractor.Common.Logging;
using FileExtractor.Common.Threading;
using FileExtractor.Data;

namespace FileExtractor.Utils.Compression.Common;

public abstract class GenericArchiveExtractorBase : IArchiveExtractor
{
    private readonly IFileSystemUtils _fileSystemUtils;
    private readonly IGenericArchiveFileUtils _archiveFileUtils;
    private readonly ITaskRunner _taskRunner;
    private readonly ILogger _logger;

    protected GenericArchiveExtractorBase(
        IFileSystemUtils fileSystemUtils,
        IGenericArchiveFileUtils archiveFileUtils,
        ITaskRunner taskRunner,
        ILogger logger)
    {
        _fileSystemUtils = fileSystemUtils;
        _archiveFileUtils = archiveFileUtils;
        _taskRunner = taskRunner;
        _logger = logger;
    }

    public async Task<IEnumerable<FileInfoData>> ExtractFiles(IEnumerable<string> archiveFiles, string outputPath, IEnumerable<FileInfoData> fileData) =>
        await _taskRunner.Run(
            () =>
            {
                var archives = Enumerable.Empty<IGenericArchive>();
                try
                {
                    archives = archiveFiles.Select(_archiveFileUtils.OpenRead);
                    var archiveEntries = GetEntries(archives);
                    return ExtractInternal(archiveEntries, outputPath, fileData);
                }
                finally
                {
                    foreach (var archive in archives)
                        archive.Dispose();
                }
            });

    private static IEnumerable<IGenericArchiveEntry> GetEntries(IEnumerable<IGenericArchive> archives) =>
        archives.SelectMany(archive => archive.Entries);

    private IEnumerable<FileInfoData> ExtractInternal(IEnumerable<IGenericArchiveEntry> archiveEntries, string outputPath, IEnumerable<FileInfoData> fileData)
    {
        if (archiveEntries?.Any() != true)
        {
            _logger.Warning("The supplied archive(s) contain no entries");
            return Enumerable.Empty<FileInfoData>();
        }

        var data = fileData
            .GroupBy(entry => entry.Directory)
            .ToDictionary(group => group.Key, group => group.ToDictionary(entry => entry.Name));

        if (!_fileSystemUtils.DirectoryExists(outputPath))
            _fileSystemUtils.CreateDirectory(outputPath);

        var extractedFiles = new HashSet<FileInfoData>();

        foreach (var pair in data)
        {
            var destinationPath = Path.Combine(outputPath, pair.Key);
            if (!_fileSystemUtils.DirectoryExists(destinationPath))
                _fileSystemUtils.CreateDirectory(destinationPath);

            foreach (var (fileInfo, matchingEntries) in GetMatchingEntries(archiveEntries, pair.Value))
            {
                matchingEntries.ForEach(
                    entry =>
                    {
                        var extractedFilePath = Path.Combine(destinationPath, entry.Name);

                        if (_fileSystemUtils.FileExists(extractedFilePath))
                        {
                            _logger.Warning("File {File} already exists in {Path}. Skipping extraction", entry.Name, destinationPath);
                            return;
                        }

                        _logger.Information("Extracting {File} to {Path}", entry.Name, destinationPath);
                        entry.ExtractToFile(extractedFilePath, overwrite: true);
                    }
                );
                extractedFiles.Add(fileInfo);
            }
        }

        return extractedFiles;
    }

    private static IEnumerable<(FileInfoData, List<IGenericArchiveEntry>)> GetMatchingEntries(IEnumerable<IGenericArchiveEntry> entries, Dictionary<string, FileInfoData> data)
    {
        foreach (var pair in data)
        {
            var matchingEntries = entries
                .Where(entry =>
                    FileSystemName.MatchesSimpleExpression(pair.Key, entry.Name)
                    && (string.IsNullOrWhiteSpace(pair.Value.Location)
                        ? string.IsNullOrWhiteSpace(Path.GetDirectoryName(entry.FullName))
                        : Path.GetDirectoryName(entry.FullName)?.EndsWith(pair.Value.Location, StringComparison.OrdinalIgnoreCase) == true))
                .ToList();

            if (!matchingEntries.Any())
                continue;

            yield return (pair.Value, matchingEntries);
        }
    }
}