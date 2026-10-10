namespace FileExtractor.Utils.Compression.Common;

public interface IGenericArchive : IDisposable
{
    /// <summary>
    /// File entries of the archive, without directories. The sequence can be enumerated only once,
    /// and an entry is valid only during its own iteration step (solid archives are read forward-only).
    /// </summary>
    public IEnumerable<IGenericArchiveEntry> Entries { get; }
}
