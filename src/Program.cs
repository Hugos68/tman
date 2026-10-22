using GBX.NET;
using GBX.NET.Engines.Game;
using tman.src;
using tman.src.Alterations;

ApplicationInitializer.Initialize();

var map = Gbx.Parse<CGameCtnChallenge>("c:\\Users\\hugok\\Documents\\Trackmania\\Maps\\Campaigns\\Fall 2026\\Egypt 2026.Map.Gbx");

AlterationRunner.Run(map, new Freewheel());

map.Save("c:\\Users\\hugok\\Documents\\Trackmania\\Maps\\Campaigns\\Fall 2026\\Egypt 2026 Freewheel.Map.Gbx");
