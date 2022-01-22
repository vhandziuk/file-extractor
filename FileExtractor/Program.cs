using FileExtractor;

var executablePath = Directory.GetCurrentDirectory();

var fileNameProvider = new CsvFileNameProvider(Path.Combine(executablePath, "configuration.csv"));
var zipFileExtractor = new ZipFileExtractor(new TaskRunner());

await zipFileExtractor.ExtractFiles(
    Directory
        .EnumerateFiles(executablePath)
        .Where(x => x.EndsWith(".zip")),
    fileNameProvider.EnumerateFiles());