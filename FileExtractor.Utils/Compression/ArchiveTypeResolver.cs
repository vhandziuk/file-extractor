using System.Collections.Frozen;

namespace FileExtractor.Utils.Compression;

public sealed class ArchiveTypeResolver : IArchiveTypeResolver
{
    private static readonly FrozenDictionary<string, ArchiveType> ArchiveTypesByExtension =
        new Dictionary<string, ArchiveType>
        {
            [".zip"] = ArchiveType.Zip,
            [".rar"] = ArchiveType.Rar,
            [".7z"] = ArchiveType.SevenZip,
            [".tar"] = ArchiveType.Other,
            [".bz2"] = ArchiveType.Other,
            [".gz"] = ArchiveType.Other,
            [".lz"] = ArchiveType.Other,
            [".xz"] = ArchiveType.Other
        }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    public bool TryResolve(string filePath, out ArchiveType archiveType) =>
        ArchiveTypesByExtension.TryGetValue(Path.GetExtension(filePath), out archiveType);
}
