namespace FileExtractor.Data;

public interface IFilePatternParser
{
    /// <summary>
    /// Parses a file pattern given on the command line, such as <c>file.txt</c>, <c>*.dll</c> or <c>docs/*.txt</c>,
    /// into the file to extract: the last segment is the name (with the <c>*</c> and <c>?</c> wildcards) and the
    /// segments before it are the location of the entry in the archive.
    /// </summary>
    /// <exception cref="ArgumentException">The pattern has no file name, or its location is not a plain path inside the archive.</exception>
    FileInfoData Parse(string pattern);
}
