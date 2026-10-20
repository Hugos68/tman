using GBX.NET;
using GBX.NET.Engines.Game;
using tman.src;
using tman.src.Alterations;

ApplicationInitializer.Initialize();

var map = Gbx.Parse<CGameCtnChallenge>("c:\\Users\\hugok\\Documents\\Trackmania\\Maps\\Campaigns\\Fall 2026\\United Kingdom 2026.Map.Gbx");

var runner = new AlterationRunner(map);

runner.Run(new AuthorRemoved(), new CheckpointsCleaned());

map.Save("c:\\Users\\hugok\\Documents\\Trackmania\\Maps\\Campaigns\\Fall 2026\\United Kingdom 2026 Checkpoints Cleaned.Map.Gbx");