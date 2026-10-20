using GBX.NET;
using GBX.NET.Engines.Game;
using tman.src;
using tman.src.Alterations;

ApplicationInitializer.Initialize();

var map = Gbx.Parse<CGameCtnChallenge>("c:\\Users\\hugok\\Documents\\Trackmania\\Maps\\Campaigns\\Fall 2026\\Fall 2026 - 01.Map.Gbx");

var runner = new AlterationRunner(map);

runner.Run(new AuthorRemoved(), new Freewheel());

map.Save("c:\\Users\\hugok\\Documents\\Trackmania\\Maps\\Campaigns\\Fall 2026\\Fall 2026 - 01 Freewheel.Map.Gbx");