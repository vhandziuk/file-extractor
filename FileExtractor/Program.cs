using CommandLine;
using FileExtractor.Application;
using FileExtractor.DiRegistration;

await Parser.Default
    .ParseArguments<CommandLineOptions>(args)
    .WithParsedAsync(
        async options =>
        {
            await using var container = new Container();
            await container.RunAsync(async app => await app.RunAsync(options));

            if (options.NoWait)
                return;

            Console.Write("Press any key to continue...");
            Console.ReadKey();
        });
