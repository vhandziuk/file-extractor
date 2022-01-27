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

    public async Task ExtractFiles(IEnumerable<string> archives, string outputPath, IEnumerable<FileInfoData> fileData)
    {
        await _taskRunner.Run(
            () =>
            {
                var zipArchives = Enumerable.Empty<ZipArchive>();
                try
                {
                    zipArchives = archives.Select(_zipFileUtils.OpenRead);
                    var zipEntries = GetEntries(zipArchives);
                    ExtractInternal(zipEntries, outputPath, fileData);
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

    private void ExtractInternal(IEnumerable<ZipArchiveEntry> zipEntries, string outputPath, IEnumerable<FileInfoData> fileData)
    {
        if (zipEntries?.Any() != true)
            return;
        if (fileData?.Any() != true)
            return;

        var data = fileData.ToDictionaryAsync(entry => entry.Name);

        var extractedPath = GetExtractedPath(outputPath);
        if (!_fileSystemUtils.DirectoryExists(extractedPath))
            _fileSystemUtils.CreateDirectory(extractedPath);

        foreach (var zipEntry in zipEntries.Where(entry => ContainsEntry(entry, data)))
            zipEntry.ExtractToFile(Path.Combine(extractedPath, zipEntry.Name), overwrite: true);
    }

    private static bool ContainsEntry(ZipArchiveEntry entry, Dictionary<string, FileInfoData> data) =>
        data.ContainsKey(entry.Name)
        && (string.IsNullOrEmpty(data[entry.Name].DirectoryName)
            ? true
            : Path.GetDirectoryName(entry.FullName).EndsWith(data[entry.Name].DirectoryName, StringComparison.OrdinalIgnoreCase));

    private static string GetExtractedPath(string outputPath) =>
        outputPath.EndsWith(value: "Extracted", StringComparison.OrdinalIgnoreCase)
            ? outputPath
            : Path.Combine(outputPath, "Extracted");
}