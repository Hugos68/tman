using System;
using GBX.NET;
using GBX.NET.Engines.Game;
using tman.src;
using tman.src.Alterations;

ApplicationInitializer.Initialize();

var map = Gbx.Parse<CGameCtnChallenge>("c:\\Users\\hugok\\Documents\\Trackmania\\Maps\\Campaigns\\Fall 2026\\Egypt 2026.Map.Gbx");

AlterationRunner.Run(map, new WithoutValidation(), new Freewheel(), new MapNameSuffix(" Freewheel"));

map.Save("c:\\Users\\hugok\\Documents\\Trackmania\\Maps\\Campaigns\\Fall 2026\\Egypt 2026 Freewheel.Map.Gbx");

// var map = Gbx.Parse<CGameCtnChallenge>(
//     @"C:\Users\hugok\Documents\Trackmania\Maps\My Maps\test Manslaughter.Map.Gbx",
//     new GbxReadSettings
//     {
//         SafeSkippableChunks = true
//     });

// Console.WriteLine("=== BLOCKS ===");

// foreach (var block in map.Node.Blocks)
// {
//     Console.WriteLine($"Block: {block.Name}");
//     Console.WriteLine($"  Coord: {block.Coord}");
//     Console.WriteLine($"  Direction: {block.Direction}");
//     Console.WriteLine($"  AbsolutePositionInMap: {block.AbsolutePositionInMap}");
//     Console.WriteLine($"  YawPitchRoll: {block.YawPitchRoll}");
//     Console.WriteLine($"  IsFree: {block.IsFree}");
//     Console.WriteLine();
// }

// Console.WriteLine("=== ANCHORED OBJECTS ===");

// foreach (var obj in map.Node.AnchoredObjects)
// {
//     Console.WriteLine($"Item: {obj.ItemModel?.Id}");
//     Console.WriteLine($"  AbsolutePositionInMap: {obj.AbsolutePositionInMap}");
//     Console.WriteLine($"  YawPitchRoll: {obj.YawPitchRoll}");
//     Console.WriteLine($"  Scale: {obj.Scale}");
//     Console.WriteLine();
// }