using System;
using GBX.NET;
using GBX.NET.Engines.Game;
using tman.src;
using tman.src.Alterations;

ApplicationInitializer.Initialize();

var map = Gbx.Parse<CGameCtnChallenge>("c:\\Users\\hugok\\Documents\\Trackmania\\Maps\\Campaigns\\Fall 2026\\Egypt 2026.Map.Gbx");

AlterationRunner.Run(map, new WithoutValidation(), new Freewheel(), new MapNameSuffix(" Freewheel"));

map.Save("c:\\Users\\hugok\\Documents\\Trackmania\\Maps\\Campaigns\\Fall 2026\\Egypt 2026 Freewheel.Map.Gbx");

// var map = Gbx.Parse<CGameCtnChallenge>("c:\\Users\\hugok\\Documents\\Trackmania\\Maps\\My Maps\\test.Map.Gbx",new GbxReadSettings()
// {
//     SafeSkippableChunks = true
// });

// map.Node.Blocks.ForEach(block =>
// {
//     Console.WriteLine(block.BlockModel.Id);
// });

