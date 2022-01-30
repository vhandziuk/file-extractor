using CommandLine;
using FileExtractor;

await Parser.Default
    .ParseArguments<CommandLineOptions>(args)
    .WithParsedAsync(
        async options =>
        {
            await using var container = new Container();
            await container.RunAsync(async app => await app.RunAsync(options));
        });