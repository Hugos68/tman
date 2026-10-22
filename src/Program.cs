using GBX.NET;
using GBX.NET.Engines.Game;
using tman.src;
using tman.src.Alterations;

ApplicationInitializer.Initialize();

var map = Gbx.Parse<CGameCtnChallenge>("c:\\Users\\hugok\\Documents\\Trackmania\\Maps\\Campaigns\\Fall 2026\\India 2026.Map.Gbx");

new SlowMotion().Apply(map);

map.Save("c:\\Users\\hugok\\Documents\\Trackmania\\Maps\\Campaigns\\Fall 2026\\India 2026 SlowMotion.Map.Gbx");
