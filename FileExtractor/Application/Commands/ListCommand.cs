using FileExtractor.Application.Options;
using FileExtractor.Common.Logging;
using FileExtractor.Utils;
using FileExtractor.Utils.Compression;

namespace FileExtractor.Application.Commands;

internal sealed class ListCommand(
    IArchiveFinder archiveFinder,
    IArchiveTypeResolver archiveTypeResolver,
    IArchiveExtractorFactory archiveExtractorFactory,
    IConsole console,
    ILogger<ListCommand> logger) : IListCommand
{
    private const string EntryIndentation = "  ";

    public Task RunAsync(IListOptions options, CancellationToken cancellationToken)
    {
        var archives = archiveFinder.FindArchives(options.Archive, options.Recursive);
        if (archives.Length == 0)
        {
            logger.Warning("No supported archive files found at {Path}. The program will now exit", options.Archive);
            return Task.CompletedTask;
        }

        foreach (var archive in archives.Order(StringComparer.OrdinalIgnoreCase))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ListArchive(archive);
        }

        return Task.CompletedTask;
    }

    private void ListArchive(string archive)
    {
        if (!archiveTypeResolver.TryResolve(archive, out var archiveType))
        {
            logger.Warning("Archive {Archive} has an unsupported type. Skipping it", archive);
            return;
        }

        try
        {
            var entries = archiveExtractorFactory.Create(archiveType).ListEntries(archive);

            console.WriteLine(archive);
            foreach (var entry in entries)
                console.WriteLine(EntryIndentation + entry);
        }
        catch (Exception ex)
        {
            // one corrupt or locked archive must not stop the listing of the others
            logger.Error(ex, "Failed to list files in {Archive}. Skipping it", archive);
        }
    }
}
