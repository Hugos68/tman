using GBX.NET;
using GBX.NET.Engines.Game;
using tman.src;
using tman.src.Alterations;

ApplicationInitializer.Initialize();

var map = Gbx.Parse<CGameCtnChallenge>("c:\\Users\\hugok\\Documents\\Trackmania\\Maps\\Campaigns\\Fall 2026\\Fall 2026 - 01.Map.Gbx");

AlterationRunner.Run(map, new AuthorRemoved(), new Flags(), new MapNameSuffix(" Flags"));

map.Save("c:\\Users\\hugok\\Documents\\Trackmania\\Maps\\Campaigns\\Fall 2026\\Fall 2026 - 01 Flags.Map.Gbx");