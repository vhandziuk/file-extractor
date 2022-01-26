using System.Collections.Immutable;
using System.IO.Compression;
using FileExtractor.Data;

namespace FileExtractor.Utils;

internal sealed class ZipFileExtractor : IZipFileExtractor
{
    private readonly IFileSystemUtils _fileSystemUtils;
    private readonly IZipFileUtils _zipFileUtils;
    private readonly ITaskRunner _taskRunner;

    public ZipFileExtractor(
        IFileSystemUtils fileSystemUtils,
        IZipFileUtils zipFileUtils,
        ITaskRunner taskRunner)
    {
        _fileSystemUtils = fileSystemUtils;
        _zipFileUtils = zipFileUtils;
        _taskRunner = taskRunner;
    }

    public async Task ExtractFiles(IEnumerable<string> archives, string outputPath, IAsyncEnumerable<FileInfoData> fileData)
    {
        await _taskRunner.Run(
            async () =>
            {
                var zipArchives = Enumerable.Empty<ZipArchive>();
                try
                {
                    zipArchives = archives.Select(_zipFileUtils.OpenRead);
                    var zipEntries = GetEntries(zipArchives);
                    await ExtractInternal(zipEntries, outputPath, fileData);
                }
                finally
                {
                    foreach (var archive in zipArchives)
                        archive.Dispose();
                }
            });
    }

    private static IEnumerable<ZipArchiveEntry> GetEntries(IEnumerable<ZipArchive> zipArchives) =>
        zipArchives.SelectMany(archive => archive.Entries).ToImmutableList();

    private async Task ExtractInternal(IEnumerable<ZipArchiveEntry> zipEntries, string outputPath, IAsyncEnumerable<FileInfoData> fileData)
    {
        var extractedPath = GetExtractedPath(outputPath);
        if (!_fileSystemUtils.DirectoryExists(extractedPath))
            _fileSystemUtils.CreateDirectory(extractedPath);

        await foreach (var file in fileData)
        {
            if (zipEntries.FirstOrDefault(entry => ContainsEntry(file, entry)) is ZipArchiveEntry zipEntry)
                zipEntry.ExtractToFile(Path.Combine(extractedPath, zipEntry.Name), overwrite: true);
        }
    }

    private static bool ContainsEntry(FileInfoData file, ZipArchiveEntry entry) =>
        entry.Name.Equals(file.Name, StringComparison.OrdinalIgnoreCase)
        && (string.IsNullOrEmpty(file.DirectoryName)
            ? true
            : Path.GetDirectoryName(entry.FullName).EndsWith(file.DirectoryName, StringComparison.OrdinalIgnoreCase));

    private static string GetExtractedPath(string outputPath) =>
        outputPath.EndsWith(value: "Extracted", StringComparison.OrdinalIgnoreCase)
            ? outputPath
            : Path.Combine(outputPath, "Extracted");
}