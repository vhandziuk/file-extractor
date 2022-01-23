using System.Globalization;
using System.IO.Compression;

namespace FileExtractor
{
    internal sealed class ZipFileExtractor : IZipFileExtractor
    {
        private readonly ITaskRunner _taskRunner;

        public ZipFileExtractor(ITaskRunner taskRunner)
        {
            _taskRunner = taskRunner;
        }

        public async Task ExtractFiles(IEnumerable<string> archives, IAsyncEnumerable<FileInfoData> fileData, string outputPath)
        {
            await _taskRunner.Run<Task>(
                async () =>
                {
                    ZipArchive[]? zipArchives = null;
                    try
                    {
                        zipArchives = archives.Select(ZipFile.OpenRead).ToArray();
                        var zipEntries = GetEntries(zipArchives).ToArray();

                        await foreach (var file in fileData)
                        {
                            if (zipEntries.FirstOrDefault(
                                    entry =>
                                    {
                                        var info = new FileInfo(entry.FullName);
                                        return file.Name?.Equals(info.Name, StringComparison.OrdinalIgnoreCase) == true
                                               && (string.IsNullOrEmpty(file.DirectoryName)
                                                       ? true
                                                       : file.DirectoryName?.Equals(info.Directory?.Name, StringComparison.OrdinalIgnoreCase) == true);
                                    }) is ZipArchiveEntry zipEntry)
                            {
                                var extractedPath =
                                    outputPath.EndsWith(value: "Extracted", ignoreCase: true, culture: CultureInfo.InvariantCulture)
                                        ? outputPath
                                        : Path.Combine(outputPath, "Extracted");

                                if (!Directory.Exists(extractedPath))
                                    Directory.CreateDirectory(extractedPath);

                                zipEntry.ExtractToFile(Path.Combine(extractedPath, zipEntry.Name));
                            }
                        }
                    }
                    finally
                    {
                        if (zipArchives != null)
                            foreach (var archive in zipArchives)
                                archive.Dispose();
                    }
                });
        }

        private IEnumerable<ZipArchiveEntry> GetEntries(IEnumerable<ZipArchive> zipArchives) =>
            zipArchives.SelectMany(archive => archive.Entries);
    }
}