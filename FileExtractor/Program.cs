﻿using System.Reflection;
using CommandLine;
using FileExtractor;

await Parser.Default
    .ParseArguments<CommandLineOptions>(args)
    .WithParsedAsync(
        async options =>
        {
            var executablePath = Path.GetDirectoryName(
                Assembly.GetExecutingAssembly().Location) ?? Directory.GetCurrentDirectory();

            var configurationPath = options.Configuration ?? Path.Combine(executablePath, "configuration.csv");
            var sourcePath = options.Source ?? executablePath;
            var destinationPath = options.Destination ?? sourcePath;

            await using var container = new Container();
            await container.RunAsync(async app => await app.Run(sourcePath, destinationPath, configurationPath));
        });