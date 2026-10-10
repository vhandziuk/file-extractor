namespace FileExtractor.Utils.Compression;

public interface IArchiveFinder
{
    /// <summary>
    /// Returns the supported archives that <paramref name="path"/> denotes: the archive itself for a file, the archives
    /// in it for a folder, or the archives matching the <c>*</c> and <c>?</c> wildcards in the last segment of the path.
    /// With <paramref name="recursive"/>, folders and wildcards are searched in subfolders as well.
    /// Returns an empty array when nothing matches or the path does not exist.
    /// </summary>
    string[] FindArchives(string path, bool recursive);
}
