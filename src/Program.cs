using GBX.NET;
using GBX.NET.Engines.Game;
using tman.src;
using tman.src.Alterations;

ApplicationInitializer.Initialize();

var map = Gbx.Parse<CGameCtnChallenge>("c:\\Users\\hugok\\Documents\\Trackmania\\Maps\\My Maps\\test.Map.Gbx", new (){ SafeSkippableChunks = true });

new Freewheel().Apply(map);

map.Save("c:\\Users\\hugok\\Documents\\Trackmania\\Maps\\My Maps\\test Freewheel.Map.Gbx");
