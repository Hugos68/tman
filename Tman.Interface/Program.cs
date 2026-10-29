using System;
using System.CommandLine;
using System.IO;
using System.Linq;
using tman.Alterations;
using tman.Maps;

internal class Program
{
    private static int Main(string[] args)
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

        rootCommand.SetAction(parseResult =>
        {
            var userAlteration = parseResult.GetValue(alterationArgument)!;
            var userInput = parseResult.GetValue(inputOption)!;
            var userOutput = parseResult.GetValue(outputOption)!;

            try
            {
                var alteration = CreateAlteration(userAlteration);
                var inputFiles = GetInputFiles(userInput);

                ValidatePaths(userInput, userOutput);

                foreach (var inputFile in inputFiles)
                {
                    ApplyAlteration(alteration, inputFile, userOutput, inputFiles.Length > 1);
                }

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

        return rootCommand.Parse(args).Invoke();
    }

    private static Alteration CreateAlteration(string name)
    {
        var type = typeof(Alteration).Assembly.GetTypes().FirstOrDefault(candidate => candidate.IsSubclassOf(typeof(Alteration)) && !candidate.IsAbstract && candidate.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (type is null)
        {
            throw new ArgumentException($"Unknown alteration: {name}");
        }

        return (Alteration)(Activator.CreateInstance(type)
            ?? throw new ArgumentException($"Could not create alteration: {name}"));
    }

    private static string[] GetInputFiles(string input)
    {
        var fullPath = Path.GetFullPath(input);
        var directory = Path.GetDirectoryName(fullPath)!;
        var pattern = Path.GetFileName(fullPath);

        if (!Directory.Exists(directory))
        {
            throw new DirectoryNotFoundException(
                $"Input directory not found: {directory}");
        }

        var isWildcard = pattern.Contains('*') || pattern.Contains('?');

        var files = isWildcard
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
        var fileName = Path.GetFileName(input);
        var isWildcard = fileName.Contains('*') || fileName.Contains('?');

        if (isWildcard && !output.Contains('*'))
        {
            throw new ArgumentException(
                "When using a wildcard input, the output path must contain '*'.");
        }

        if (!isWildcard && output.Contains('*'))
        {
            throw new ArgumentException(
                "The output path cannot contain '*' when using a single input file.");
        }
    }

    private static void ApplyAlteration(
        Alteration alteration,
        string input,
        string output,
        bool multipleFiles)
    {
        var map = Map.Load(input);

        alteration.Apply(map);

        const string extension = ".Map.Gbx";
        var fileName = Path.GetFileName(input);

        var fileStem = fileName.EndsWith(extension, StringComparison.OrdinalIgnoreCase)
            ? fileName[..^extension.Length]
            : Path.GetFileNameWithoutExtension(fileName);

        var outputPath = Path.GetFullPath(
            multipleFiles ? output.Replace("*", fileStem) : output);

        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

        map.Save(outputPath);

        Console.WriteLine($"Applied {alteration.GetType().Name}: {outputPath}");
    }
}