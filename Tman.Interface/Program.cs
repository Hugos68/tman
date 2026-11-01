using System;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tman.Core.Alterations;
using Tman.Core.Maps;

namespace Tman.Interface
{
    internal static class Program
    {
        private const string MapExtension = ".Map.Gbx";

        private const char Wildcard = '*';

        private static readonly char[] WildcardCharacters = ['*', '?'];

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

            var rootCommand = new RootCommand("Apply an alteration to Trackmania maps.");

            rootCommand.Arguments.Add(alterationArgument);
            rootCommand.Options.Add(inputOption);
            rootCommand.Options.Add(outputOption);

            rootCommand.SetAction(async (parseResult, cancellationToken) =>
            {
                var alterationName = parseResult.GetValue(alterationArgument)!;
                var input = parseResult.GetValue(inputOption)!;
                var output = parseResult.GetValue(outputOption)!;

                try
                {
                    var alteration = CreateAlteration(alterationName);
                    var inputFiles = InputFiles(input);

                    ValidatePaths(input, output);

                    await Parallel.ForEachAsync(inputFiles, cancellationToken, (inputFile, token) =>
                    {
                        token.ThrowIfCancellationRequested();
                        Apply(alteration, inputFile, output, inputFiles.Length > 1);
                        return ValueTask.CompletedTask;
                    });

                    return 0;
                }
                catch (Exception exception) when (
                    exception is ArgumentException or
                    DirectoryNotFoundException or
                    FileNotFoundException or
                    IOException or
                    UnauthorizedAccessException)
                {
                    Console.Error.WriteLine(exception.Message);
                    return 1;
                }
            });

            return await rootCommand.Parse(args).InvokeAsync();
        }

        private static Alteration CreateAlteration(string name)
        {
            var type = typeof(Alteration).Assembly.GetTypes().FirstOrDefault(candidate => candidate.IsSubclassOf(typeof(Alteration)) && !candidate.IsAbstract && candidate.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                ?? throw new ArgumentException($"Unknown alteration: {name}");

            return (Alteration)(Activator.CreateInstance(type)
                ?? throw new ArgumentException($"Could not create alteration: {name}"));
        }

        private static string[] InputFiles(string input)
        {
            var fullPath = Path.GetFullPath(input);
            var directory = Path.GetDirectoryName(fullPath)!;
            var pattern = Path.GetFileName(fullPath);

            if (!Directory.Exists(directory))
            {
                throw new DirectoryNotFoundException($"Input directory not found: {directory}");
            }

            var files = IsWildcard(pattern)
                ? Directory.GetFiles(directory, pattern)
                : File.Exists(fullPath) ? [fullPath] : [];

            if (files.Length == 0)
            {
                throw new FileNotFoundException($"No input files found: {input}");
            }

            return files;
        }

        private static void ValidatePaths(string input, string output)
        {
            var wildcardInput = IsWildcard(Path.GetFileName(input));

            if (wildcardInput && !output.Contains(Wildcard))
            {
                throw new ArgumentException($"When using a wildcard input, the output path must contain '{Wildcard}'.");
            }

            if (!wildcardInput && output.Contains(Wildcard))
            {
                throw new ArgumentException($"The output path cannot contain '{Wildcard}' when using a single input file.");
            }
        }

        private static bool IsWildcard(string fileName) => fileName.IndexOfAny(WildcardCharacters) >= 0;

        private static void Apply(Alteration alteration, string input, string output, bool multipleFiles)
        {
            var map = Map.Load(input);

            alteration.Apply(map);

            var fileName = Path.GetFileName(input);

            var fileStem = fileName.EndsWith(MapExtension, StringComparison.OrdinalIgnoreCase)
                ? fileName[..^MapExtension.Length]
                : Path.GetFileNameWithoutExtension(fileName);

            var outputPath = Path.GetFullPath(multipleFiles ? output.Replace(Wildcard.ToString(), fileStem) : output);

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

            map.Save(outputPath);

            Console.WriteLine($"Applied {alteration.GetType().Name}: {outputPath}");
        }
    }
}
