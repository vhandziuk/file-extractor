namespace FileExtractor.Data;

public sealed class TextFileWrapper : ITextFile
{
    public IEnumerable<string> ReadLines(string path) => File.ReadLines(path);
}
