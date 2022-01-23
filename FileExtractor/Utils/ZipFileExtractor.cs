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

    public async Task ExtractFiles(IEnumerable<string> archives, IAsyncEnumerable<FileInfoData> fileData, string outputPath)
    {
        await _taskRunner.Run<Task>(
            async () =>
            {
                ZipArchive[] zipArchives = null;
                try
                {
                    zipArchives = archives.Select(_zipFileUtils.OpenRead).ToArray();
                    var zipEntries = GetEntries(zipArchives).ToArray();
                    await ExtractInternal(zipEntries, fileData, outputPath);
                }
                finally
                {
                    if (zipArchives != null)
                        foreach (var archive in zipArchives)
                            archive.Dispose();
                }
            });
    }

    private static IEnumerable<ZipArchiveEntry> GetEntries(IEnumerable<ZipArchive> zipArchives) =>
        zipArchives.SelectMany(archive => archive.Entries);

    private async Task ExtractInternal(ZipArchiveEntry[] zipEntries, IAsyncEnumerable<FileInfoData> fileData, string outputPath)
    {
        await foreach (var file in fileData)
        {
            if (zipEntries.FirstOrDefault(entry => ContainsEntry(file, entry)) is ZipArchiveEntry zipEntry)
            {
                var extractedPath = GetExtractedPath(outputPath);

                if (!_fileSystemUtils.DirectoryExists(extractedPath))
                    _fileSystemUtils.CreateDirectory(extractedPath);

                zipEntry.ExtractToFile(Path.Combine(extractedPath, zipEntry.Name));
            }
        }
    }

    private static bool ContainsEntry(FileInfoData file, ZipArchiveEntry entry)
    {
        var info = new FileInfo(entry.FullName);
        return file.Name?.Equals(info.Name, StringComparison.OrdinalIgnoreCase) == true
               && (string.IsNullOrEmpty(file.DirectoryName)
                       ? true
                       : file.DirectoryName?.Equals(info.Directory?.Name, StringComparison.OrdinalIgnoreCase) == true);
    }

    private static string GetExtractedPath(string outputPath)
    {
        return outputPath.EndsWith(value: "Extracted", ignoreCase: true, culture: CultureInfo.InvariantCulture)
               ? outputPath
               : Path.Combine(outputPath, "Extracted");
    }
}