using System.Reflection;
using CommandLine;
using FileExtractor;
using FileExtractor.Data;
using FileExtractor.Utils;

await Parser.Default
    .ParseArguments<CommandLineOptions>(args)
    .WithParsedAsync(
        async options =>
        {
            var executablePath = Path.GetDirectoryName(
                Assembly.GetExecutingAssembly().Location) ?? Directory.GetCurrentDirectory();
            var configurationPath = options.Configuration ?? Path.Combine(executablePath, "configuration.csv");
            var sourcePath = options.Source ?? executablePath;
            var destinationPath = options.Destination ?? executablePath;


            var fileNameProvider = new CsvFileNameProvider(configurationPath);
            var zipFileExtractor = new ZipFileExtractor(new TaskRunner());

            await zipFileExtractor.ExtractFiles(
                Directory
                    .EnumerateFiles(sourcePath)
                    .Where(x => x.EndsWith(".zip")),
                fileNameProvider.EnumerateFiles(),
                destinationPath);
        });