using System;
using System.CommandLine;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tman.Interface;

namespace Tman.Cli
{
    internal static class Program
    {
        private const string LoggerName = "tman";

        private static async Task<int> Main(string[] args)
        {
            var alterationArgument = new Argument<string>("alteration")
            {
                Description = "The alteration to apply."
            };

            var inputOption = new Option<string>("--input", "-i")
            {
                Description = "The input map file or wildcard pattern.",
                Required = true
            };

            var outputOption = new Option<string>("--output", "-o")
            {
                Description = "The output map file or wildcard pattern.",
                Required = true
            };

            var verboseOption = new Option<bool>("--verbose", "-v")
            {
                Description = "Log every step, kept block and built surface block."
            };

            var quietOption = new Option<bool>("--quiet", "-q")
            {
                Description = "Log warnings and errors only."
            };

            var rootCommand = new RootCommand("Apply an alteration to Trackmania maps.");

            rootCommand.Arguments.Add(alterationArgument);
            rootCommand.Options.Add(inputOption);
            rootCommand.Options.Add(outputOption);
            rootCommand.Options.Add(verboseOption);
            rootCommand.Options.Add(quietOption);

            rootCommand.SetAction(async (parseResult, cancellationToken) =>
            {
                var alterationName = parseResult.GetValue(alterationArgument)!;
                var input = parseResult.GetValue(inputOption)!;
                var output = parseResult.GetValue(outputOption)!;
                var level = parseResult.GetValue(verboseOption) ? LogLevel.Debug : parseResult.GetValue(quietOption) ? LogLevel.Warning : LogLevel.Information;

                using var host = TmanHost.Create(logging => logging.SetMinimumLevel(level).AddSimpleConsole(console =>
                {
                    console.SingleLine = true;
                    console.IncludeScopes = true;
                }));
                var logger = host.Logger(LoggerName);
                try
                {
                    var paths = MapPaths.Resolve(input, output);
                    await Batch.RunAsync(host.Pipeline, alterationName, paths, (done, total, outputPath) => logger.LogInformation("[{Done}/{Total}] {Alteration}: {Output}", done, total, alterationName, outputPath), cancellationToken);
                    return 0;
                }
                catch (Exception exception) when (
                    exception is ArgumentException or
                    DirectoryNotFoundException or
                    FileNotFoundException or
                    IOException or
                    UnauthorizedAccessException)
                {
                    logger.LogError("{Message}", exception.Message);
                    return 1;
                }
            });

            return await rootCommand.Parse(args).InvokeAsync();
        }
    }
}
