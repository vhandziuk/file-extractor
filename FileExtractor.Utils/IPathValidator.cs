namespace FileExtractor.Utils;

public interface IPathValidator
{
    /// <summary>
    /// Returns whether <paramref name="path"/>, once normalized, is <paramref name="rootPath"/> itself or lies below it,
    /// and does not address an alternate data stream.
    /// </summary>
    bool IsContained(string rootPath, string path);

    /// <summary>
    /// Returns whether <paramref name="fileName"/> is a plain Windows file name: no path separators, wildcards,
    /// control characters or stream separators, no trailing dot or space, and no reserved device name.
    /// </summary>
    bool IsValidFileName(string fileName);

    /// <summary>
    /// Returns whether <paramref name="path"/> is empty or a relative path whose <c>\</c>- or <c>/</c>-separated
    /// segments are all valid file names (see <see cref="IsValidFileName"/>), so it cannot leave the folder it is combined with.
    /// </summary>
    bool IsValidRelativePath(string path);
}
