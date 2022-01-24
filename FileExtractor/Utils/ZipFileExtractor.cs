using System.Collections.Generic;
using System.Globalization;
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

    public async ValueTask ExtractFiles(IEnumerable<string> archives, string outputPath, IAsyncEnumerable<FileInfoData> fileData)
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
        zipArchives.SelectMany(archive => archive.Entries);

    private async ValueTask ExtractInternal(IEnumerable<ZipArchiveEntry> zipEntries, string outputPath, IAsyncEnumerable<FileInfoData> fileData)
    {
        var extractedPath = GetExtractedPath(outputPath);
        if (!_fileSystemUtils.DirectoryExists(extractedPath))
            _fileSystemUtils.CreateDirectory(extractedPath);

        var data = await fileData.ToDictionaryAsync(entry => entry.Name);
        foreach (var zipEntry in zipEntries.Where(entry => ShouldBeExtracted(entry, data)))
            zipEntry.ExtractToFile(Path.Combine(extractedPath, zipEntry.Name), overwrite: true);
    }

    private static string GetExtractedPath(string outputPath) =>
        outputPath.EndsWith(value: "Extracted", ignoreCase: true, culture: CultureInfo.InvariantCulture)
            ? outputPath
            : Path.Combine(outputPath, "Extracted");

    private static bool ShouldBeExtracted(ZipArchiveEntry entry, Dictionary<string, FileInfoData> data) =>
        data.ContainsKey(entry.Name)
        && (string.IsNullOrEmpty(data[entry.Name].DirectoryName)
            ? true
            : Path.GetDirectoryName(entry.FullName).EndsWith(data[entry.Name].DirectoryName, StringComparison.OrdinalIgnoreCase));
}