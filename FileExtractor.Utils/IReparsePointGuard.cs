namespace FileExtractor.Utils;

public interface IReparsePointGuard
{
    /// <summary>
    /// Returns whether <paramref name="path"/> or any of its existing parent folders below <paramref name="rootPath"/>
    /// is a reparse point (symbolic link, junction or mount point). <paramref name="rootPath"/> itself is not checked.
    /// </summary>
    bool ContainsReparsePoint(string rootPath, string path);
}
