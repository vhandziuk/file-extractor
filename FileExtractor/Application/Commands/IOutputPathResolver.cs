namespace FileExtractor.Application.Commands;

internal interface IOutputPathResolver
{
    /// <summary>
    /// Returns the full path of <paramref name="outputPath"/>, or of <paramref name="defaultPath"/> when it is
    /// not provided, and creates the directory when it does not exist yet.
    /// </summary>
    string Resolve(string? outputPath, string defaultPath);
}
