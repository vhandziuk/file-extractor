using CommandLine;
using FileExtractor.Application.Options;
using FileExtractor.Common.Security;
using FileExtractor.DiRegistration;

// before anything loads a native DLL: the current folder (often the one with the processed archives)
// must not be searched for DLLs
if (OperatingSystem.IsWindows())
    new DllSearchPathHardener().Harden();

await Parser.Default
    .ParseArguments(args, typeof(ConfigurationOptions), typeof(ExtractOptions), typeof(ListOptions))
    .WithParsedAsync<ICommonOptions>(
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
