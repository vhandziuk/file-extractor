using System.Collections.Generic;
using System.Linq;
using FileExtractor.Utils.Compression.Common;
using Moq;
using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Readers;
using Xunit;

namespace FileExtractor.Utils.UnitTest.Compression.Common;

public class SharpCompressEntryReaderTest
{
    private readonly Mock<IArchive> _archiveMock = new();

    private readonly SharpCompressEntryReader _sut = new();

    [Fact]
    public void ReadEntries_ArchiveIsNotSolid_WrapsFileEntriesOnly()
    {
        var fileEntry1 = GetMockedArchiveEntry(@"SomeDirectory\SomeName1.dat", isDirectory: false);
        var directoryEntry = GetMockedArchiveEntry(@"SomeDirectory\", isDirectory: true);
        var fileEntry2 = GetMockedArchiveEntry(@"SomeDirectory\SomeName2.dat", isDirectory: false);
        LetArchiveBeSolid(false);
        LetArchiveEntriesBe(fileEntry1, directoryEntry, fileEntry2);
        var wrappedEntries = new Dictionary<IArchiveEntry, IGenericArchiveEntry>
        {
            [fileEntry1] = Mock.Of<IGenericArchiveEntry>(),
            [fileEntry2] = Mock.Of<IGenericArchiveEntry>()
        };

        var result = _sut.ReadEntries(_archiveMock.Object, entry => wrappedEntries[entry]).ToList();

        Assert.Equal([wrappedEntries[fileEntry1], wrappedEntries[fileEntry2]], result);
        _archiveMock.Verify(archive => archive.ExtractAllEntries(), Times.Never);
    }

    [Fact]
    public void ReadEntries_ArchiveIsSolid_ReadsFileEntriesSequentially()
    {
        LetArchiveBeSolid(true);
        LetReaderEntriesBe(
            GetMockedEntry(@"SomeDirectory\SomeName1.dat", isDirectory: false),
            GetMockedEntry(@"SomeDirectory\", isDirectory: true),
            GetMockedEntry(@"SomeDirectory\Nested\SomeName2.dat", isDirectory: false));
        var wrapEntryCalled = false;

        var result = _sut
            .ReadEntries(_archiveMock.Object, _ => { wrapEntryCalled = true; return Mock.Of<IGenericArchiveEntry>(); })
            .Select(entry => (entry.Name, entry.FullName))
            .ToList();

        Assert.Equal(
            [
                ("SomeName1.dat", @"SomeDirectory\SomeName1.dat"),
                ("SomeName2.dat", @"SomeDirectory\Nested\SomeName2.dat")
            ],
            result);
        Assert.False(wrapEntryCalled);
        _archiveMock.VerifyGet(archive => archive.Entries, Times.Never);
    }

    [Fact]
    public void ReadEntries_SolidArchiveEnumerated_DisposesReader()
    {
        LetArchiveBeSolid(true);
        var readerMock = LetReaderEntriesBe(GetMockedEntry("SomeName.dat", isDirectory: false));

        _sut.ReadEntries(_archiveMock.Object, _ => Mock.Of<IGenericArchiveEntry>()).ToList();

        readerMock.Verify(reader => reader.Dispose(), Times.Once);
    }

    [Fact]
    public void ReadEntries_SolidArchiveNotEnumerated_DoesNotOpenReader()
    {
        LetArchiveBeSolid(true);

        _sut.ReadEntries(_archiveMock.Object, _ => Mock.Of<IGenericArchiveEntry>());

        _archiveMock.Verify(archive => archive.ExtractAllEntries(), Times.Never);
    }

    private static IArchiveEntry GetMockedArchiveEntry(string key, bool isDirectory) =>
        Mock.Of<IArchiveEntry>(entry => entry.Key == key && entry.IsDirectory == isDirectory);

    private static IEntry GetMockedEntry(string key, bool isDirectory) =>
        Mock.Of<IEntry>(entry => entry.Key == key && entry.IsDirectory == isDirectory);

    private void LetArchiveBeSolid(bool isSolid) =>
        _archiveMock
            .SetupGet(archive => archive.IsSolid)
            .Returns(isSolid);

    private void LetArchiveEntriesBe(params IArchiveEntry[] entries) =>
        _archiveMock
            .SetupGet(archive => archive.Entries)
            .Returns(entries);

    private Mock<IReader> LetReaderEntriesBe(params IEntry[] entries)
    {
        var remainingEntries = new Queue<IEntry>(entries);
        IEntry? currentEntry = null;

        var readerMock = new Mock<IReader>();
        readerMock
            .Setup(reader => reader.MoveToNextEntry())
            .Returns(() => remainingEntries.TryDequeue(out currentEntry));
        readerMock
            .SetupGet(reader => reader.Entry)
            .Returns(() => currentEntry!);

        _archiveMock
            .Setup(archive => archive.ExtractAllEntries())
            .Returns(readerMock.Object);

        return readerMock;
    }
}
