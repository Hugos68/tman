
using GBX.NET;
using GBX.NET.Engines.Game;
using tman.src;
using tman.src.Alterations;
using System.CommandLine;
using System.IO;
using System;


ApplicationInitializer.Initialize();

var alterationArgument = new Argument<string>("alteration")
{
    Description = "The alteration to apply."
};

var inputOption = new Option<FileInfo>("--input", "-i")
{
    Description = "The input .Map.Gbx file.",
    Required = true
};

var outputOption = new Option<FileInfo>("--output", "-o")
{
    Description = "The output map file.",
    Required = true
};

var rootCommand = new RootCommand("Apply an alteration to a Trackmania map.");

rootCommand.Arguments.Add(alterationArgument);
rootCommand.Options.Add(inputOption);
rootCommand.Options.Add(outputOption);

rootCommand.SetAction(parseResult =>
{
    var alteration = parseResult.GetValue(alterationArgument);
    var input = parseResult.GetValue(inputOption)!;
    var output = parseResult.GetValue(outputOption)!;

    if (!input.Exists)
    {
        Console.Error.WriteLine($"Input file not found: {input.FullName}");
        return 1;
    }

    var map = Gbx.Parse<CGameCtnChallenge>(input.FullName, new()
    {
        SafeSkippableChunks = true
    });

    switch (alteration)
    {
        case "Checkpointless":
            new Checkpointless().Apply(map);
            break;
        case "Flags":
            new Flags().Apply(map);
            break;
        case "Freewheel":
            new Freewheel().Apply(map);
            break;
        case "Manslaughter":
            new Manslaughter().Apply(map);
            break;
        case "NoSteer":
            new NoSteer().Apply(map);
            break;
        case "SlowMotion":
            new SlowMotion().Apply(map);
            break;
        case "StraightToTheFinish":
            new StraightToTheFinish().Apply(map);
            break;
        default:
            Console.Error.WriteLine($"Unknown alteration: {alteration}");
            return 1;
    }

    output.Directory?.Create();
    map.Save(output.FullName);

    Console.WriteLine($"Applied {alteration}: {output.FullName}");
    return 0;
});

return rootCommand.Parse(args).Invoke();
