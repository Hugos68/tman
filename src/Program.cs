
using System;
using System.IO;
using System.CommandLine;
using GBX.NET;
using GBX.NET.Engines.Game;
using tman.src;
using tman.src.Application;

ApplicationInitializer.Initialize();

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
    var alterationInput = parseResult.GetValue(alterationArgument)!;
    var input = parseResult.GetValue(inputOption)!;
    var output = parseResult.GetValue(outputOption)!;

    var inputDirectory = Path.GetDirectoryName(Path.GetFullPath(input))!;
    var inputPattern = Path.GetFileName(input);
    var isWildcard = inputPattern.Contains('*') || inputPattern.Contains('?');

    if (!Directory.Exists(inputDirectory))
    {
        Console.Error.WriteLine($"Input directory not found: {inputDirectory}");
        return 1;
    }

    var inputFiles = isWildcard
        ? Directory.GetFiles(inputDirectory, inputPattern)
        : File.Exists(input) ? [Path.GetFullPath(input)] : [];

    if (inputFiles.Length == 0)
    {
        Console.Error.WriteLine($"No input files found: {input}");
        return 1;
    }

    if (isWildcard && !output.Contains('*'))
    {
        Console.Error.WriteLine(
            "When using a wildcard input, the output path must contain '*'.");
        return 1;
    }

    if (!isWildcard && inputFiles.Length == 1 && output.Contains('*'))
    {
        Console.Error.WriteLine(
            "The output path cannot contain '*' when using a single input file.");
        return 1;
    }

    foreach (var inputFile in inputFiles)
    {
        var map = Gbx.Parse<CGameCtnChallenge>(inputFile, new()
        {
            SafeSkippableChunks = true
        });

        var type = typeof(Alteration).Assembly.GetType($"tman.src.Alterations.{alterationInput}");

        if (type is null || !typeof(Alteration).IsAssignableFrom(type))
        {
            throw new ArgumentException($"Unknown alteration: {alterationInput}");
        }

        var alteration = (Alteration)Activator.CreateInstance(type)!;

        alteration.Apply(map);

        var outputPath = output;

        if (isWildcard)
        {
            const string extension = ".Map.Gbx";
            var fileName = Path.GetFileName(inputFile);

            var fileStem = fileName.EndsWith(extension, StringComparison.OrdinalIgnoreCase)
                ? fileName[..^extension.Length]
                : Path.GetFileNameWithoutExtension(fileName);

            outputPath = outputPath.Replace("*", fileStem);
        }

        outputPath = Path.GetFullPath(outputPath);
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

        map.Save(outputPath);

        Console.WriteLine($"Applied {alterationInput}: {outputPath}");
    }

    return 0;
});

return rootCommand.Parse(args).Invoke();
