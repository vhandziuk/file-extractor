using CommandLine;
using FileExtractor.Application;
using FileExtractor.DiRegistration;

await Parser.Default
    .ParseArguments<CommandLineOptions>(args)
    .WithParsedAsync(
        async options =>
        {
            // not disposed: the Ctrl+C handler may still fire while the process shuts down
            var cancellationTokenSource = new CancellationTokenSource();
            Console.CancelKeyPress += (_, eventArgs) =>
            {
                // the first Ctrl+C stops the extraction after the current entries; a second one terminates the process
                eventArgs.Cancel = !cancellationTokenSource.IsCancellationRequested;
                cancellationTokenSource.Cancel();
            };

            await using var container = new Container();
            await container.RunAsync(async app => await app.RunAsync(options, cancellationTokenSource.Token));

            if (options.NoWait || cancellationTokenSource.IsCancellationRequested || Console.IsInputRedirected)
                return;

            Console.Write("Press any key to continue...");
            Console.ReadKey();
        });
