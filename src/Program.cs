using System;
using GBX.NET;
using GBX.NET.Engines.Game;
using tman.src;
using tman.src.Alterations;

ApplicationInitializer.Initialize();

var map = Gbx.Parse<CGameCtnChallenge>("c:\\Users\\hugok\\Documents\\Trackmania\\Maps\\Campaigns\\Fall 2026\\Egypt 2026.Map.Gbx");

AlterationRunner.Run(map, new WithoutValidation(), new StraightToTheFinish(), new Manslaughter(), new MapNameSuffix(" Manslaughter"));

map.Save("c:\\Users\\hugok\\Documents\\Trackmania\\Maps\\Campaigns\\Fall 2026\\Egypt 2026 Manslaughter.Map.Gbx");