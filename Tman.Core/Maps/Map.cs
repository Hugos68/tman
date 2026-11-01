using System;
using System.Collections.Generic;
using System.Linq;
using GBX.NET;
using GBX.NET.Engines.Game;
using GBX.NET.LZO;
using Tman.Core.Catalog;
using Tman.Core.Maps.Placements;

namespace Tman.Core.Maps
{
    public sealed partial class Map
    {
        private const int VistaGroundRowHigh = 15;

        private const int VistaGroundRowLow = 5;

        private const int CarCollectionId = 10003;

        private static readonly Dictionary<Car, string> CarModels = new()
        {
            [Car.Desert] = "CarDesert",
            [Car.Rally] = "CarRally",
            [Car.Snow] = "CarSnow",
            [Car.Sport] = "CarSport",
        };

        static Map()
        {
            Gbx.LZO = new Lzo();
        }

        internal CGameCtnChallenge Challenge { get; }

        internal Map(CGameCtnChallenge challenge)
        {
            Challenge = challenge;
        }

        public static Map Load(string path) => new(Gbx.ParseNode<CGameCtnChallenge>(path, new() { SafeSkippableChunks = true }));

        public void Save(string path) => Challenge.Save(path);

        public string Name
        {
            get => Challenge.MapName;
            set => Challenge.MapName = value;
        }

        public string Uid => Challenge.MapUid;

        public Int3 Size => Challenge.Size;

        public string Decoration => Challenge.Decoration.Id;

        internal Collections Collection => Challenge.GetEnvironment() is "Stadium2020" ? Collections.Stadium : Enum.Parse<Collections>(Challenge.GetEnvironment());

        internal int GroundRow => Collection switch
        {
            Collections.RedIsland or Collections.WhiteShore => VistaGroundRowHigh,
            Collections.BlueBay or Collections.GreenCoast => VistaGroundRowLow,
            _ => Challenge.DecoBaseHeightOffset
        };

        internal Car Car
        {
            set => Challenge.PlayerModel = new Ident(CarModels[value], CarCollectionId, "");
        }

        public int Laps
        {
            get => Challenge.NbLaps;
            set
            {
                Challenge.IsLapRace = true;
                Challenge.NbLaps = value;
            }
        }

        internal bool Supports(Block block) => block.Collections.HasFlag(Collection);

        internal bool Supports(Item item) => item.Collections.HasFlag(Collection);

        internal IEnumerable<Placement> Placements =>
            (Challenge.Blocks ?? []).Select(block => new Placement(block))
                .Concat((Challenge.AnchoredObjects ?? []).Select(item => new Placement(item)));

        internal IEnumerable<Placement> Terrain =>
            (Challenge.Blocks ?? []).Concat(Challenge.BakedBlocks ?? []).Where(block => !block.IsFree).Select(block => new Placement(block));

        internal List<Placement> Of(Selector selector) => Placements.Where(selector.Matches).ToList();

        internal List<Placement> Waypoints(Waypoint waypoint) =>
            Placements.Where(placement => placement.IsWaypoint && placement.Waypoint == waypoint).ToList();

        internal void Remove(Placement placement)
        {
            if (placement.Block is not null)
            {
                Challenge.RemoveBlock(placement.Block);
            }
            else
            {
                Challenge.AnchoredObjects?.Remove(placement.Item!);
            }
        }

        public void GenerateUid() => Challenge.GenerateMapUid();
    }
}
