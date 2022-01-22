namespace FileExtractor
{
    internal class CsvFileNameProvider : IFileNameProvider
    {
        private readonly string _filePath;

        public CsvFileNameProvider(string filePath)
        {
            _filePath = filePath;
        }

        public async IAsyncEnumerable<FileInfoData> EnumerateFiles()
        {
            using (var stream = new FileStream(_filePath, FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(stream))
            {
                while (await reader.ReadLineAsync() is string line)
                {
                    var data = line.Split(',');
                    yield return new FileInfoData(data[0], data[1]);
                }
            }
        }
    }
}