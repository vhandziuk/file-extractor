namespace FileExtractor.Data;

public interface ITextFile
{
    IEnumerable<string> ReadLines(string path);
}
