using SharpCompress.Archives;

namespace FileExtractor.Utils.Compression.Common;

/// <summary>
/// Lists the file entries of a SharpCompress archive (rar, 7z) as <see cref="IGenericArchiveEntry"/> items,
/// following the single-pass contract of <see cref="IGenericArchive.Entries"/>.
/// </summary>
public interface ISharpCompressEntryReader
{
    IEnumerable<IGenericArchiveEntry> ReadEntries(IArchive archive, Func<IArchiveEntry, IGenericArchiveEntry> wrapEntry);
}
