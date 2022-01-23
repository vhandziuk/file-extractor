using System.Reflection;
using CommandLine;
using FileExtractor;

await Parser.Default.ParseArguments<CommandLineOptions>(args)
    .WithParsedAsync(
        async options =>
        {
            var executablePath = Path.GetDirectoryName(
                Assembly.GetExecutingAssembly().Location) ?? Directory.GetCurrentDirectory();
            var configurationPath = options.Configuration ?? Path.Combine(executablePath, "configuration.csv");
            var outputPath = options.Output ?? executablePath;


            var fileNameProvider = new CsvFileNameProvider(configurationPath);
            var zipFileExtractor = new ZipFileExtractor(new TaskRunner());

            await zipFileExtractor.ExtractFiles(
                Directory
                    .EnumerateFiles(executablePath)
                    .Where(x => x.EndsWith(".zip")),
                fileNameProvider.EnumerateFiles(),
                outputPath);
        });