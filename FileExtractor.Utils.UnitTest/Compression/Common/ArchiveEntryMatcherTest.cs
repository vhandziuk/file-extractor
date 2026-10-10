using FileExtractor.Data;
using FileExtractor.Utils.Compression.Common;
using Xunit;

namespace FileExtractor.Utils.UnitTest.Compression.Common;

public class ArchiveEntryMatcherTest
{
    [Theory]
    [InlineData("SomeName.dat", "SomeName.dat")]
    [InlineData("SomeName.dat", "somename.DAT")]
    [InlineData("*.dat", "SomeName.dat")]
    [InlineData("Some?ame.*", "SomeName.dat")]
    public void IsMatch_NameMatches_ReturnsTrue(string configuredName, string entryName)
    {
        var fileInfo = new FileInfoData("", configuredName, "SomeDirectory");

        var result = ArchiveEntryMatcher.IsMatch(fileInfo, entryName, $@"SomeDirectory\{entryName}");

        Assert.True(result);
    }

    [Theory]
    [InlineData("SomeName.dat", "SomeOtherName.dat")]
    [InlineData("*.txt", "SomeName.dat")]
    [InlineData("Some?Name.dat", "SomeName.dat")]
    [InlineData("*", "")]
    public void IsMatch_NameDoesNotMatch_ReturnsFalse(string configuredName, string entryName)
    {
        var fileInfo = new FileInfoData("", configuredName, "SomeDirectory");

        var result = ArchiveEntryMatcher.IsMatch(fileInfo, entryName, $@"SomeDirectory\{entryName}");

        Assert.False(result);
    }

    [Theory]
    [InlineData("SomeName.dat")]
    [InlineData(@"SomeDirectory\SomeName.dat")]
    [InlineData(@"SomeDirectory\Nested\SomeName.dat")]
    [InlineData("SomeDirectory/Nested/SomeName.dat")]
    public void IsMatch_LocationIsEmpty_MatchesEntryInAnyFolder(string entryFullName)
    {
        var fileInfo = new FileInfoData("", "SomeName.dat", "");

        var result = ArchiveEntryMatcher.IsMatch(fileInfo, "SomeName.dat", entryFullName);

        Assert.True(result);
    }

    [Theory]
    [InlineData(@"SomeDirectory\Nested\SomeName.dat", "Nested")]
    [InlineData(@"SomeDirectory\Nested\SomeName.dat", @"SomeDirectory\Nested")]
    [InlineData(@"SomeDirectory\Nested\SomeName.dat", @"somedirectory\NESTED")]
    [InlineData("SomeDirectory/Nested/SomeName.dat", @"SomeDirectory\Nested")]
    public void IsMatch_EntryParentPathEndsWithLocation_ReturnsTrue(string entryFullName, string location)
    {
        var fileInfo = new FileInfoData("", "SomeName.dat", location);

        var result = ArchiveEntryMatcher.IsMatch(fileInfo, "SomeName.dat", entryFullName);

        Assert.True(result);
    }

    [Theory]
    [InlineData("SomeName.dat", "SomeDirectory")]
    [InlineData(@"SomeOtherDirectory\SomeName.dat", "SomeDirectory")]
    [InlineData(@"SomeDirectory\Nested\SomeName.dat", "SomeDirectory")]
    public void IsMatch_EntryParentPathDoesNotEndWithLocation_ReturnsFalse(string entryFullName, string location)
    {
        var fileInfo = new FileInfoData("", "SomeName.dat", location);

        var result = ArchiveEntryMatcher.IsMatch(fileInfo, "SomeName.dat", entryFullName);

        Assert.False(result);
    }
}
