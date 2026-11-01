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
            GBX.NET.Gbx.LZO = new Lzo();
        }

        internal CGameCtnChallenge Gbx { get; }

        internal Map(CGameCtnChallenge gbx)
        {
            Gbx = gbx;
        }

        public static Map Load(string path) => new(GBX.NET.Gbx.ParseNode<CGameCtnChallenge>(path, new() { SafeSkippableChunks = true }));

        public void Save(string path) => Gbx.Save(path);

        public string Name
        {
            get => Gbx.MapName;
            set => Gbx.MapName = value;
        }

        public string Uid => Gbx.MapUid;

        public Int3 Size => Gbx.Size;

        public string Decoration => Gbx.Decoration.Id;

        internal Collections Collection => Gbx.GetEnvironment() is "Stadium2020" ? Collections.Stadium : Enum.Parse<Collections>(Gbx.GetEnvironment());

        internal int GroundRow => Collection switch
        {
            Collections.RedIsland or Collections.WhiteShore => VistaGroundRowHigh,
            Collections.BlueBay or Collections.GreenCoast => VistaGroundRowLow,
            _ => Gbx.DecoBaseHeightOffset
        };

        internal Car Car
        {
            set => Gbx.PlayerModel = new Ident(CarModels[value], CarCollectionId, "");
        }

        public int Laps
        {
            get => Gbx.NbLaps;
            set
            {
                Gbx.IsLapRace = true;
                Gbx.NbLaps = value;
            }
        }

        internal bool Supports(Block block) => block.Collections.HasFlag(Collection);

        internal bool Supports(Item item) => item.Collections.HasFlag(Collection);

        internal IEnumerable<Placement> Placements =>
            (Gbx.Blocks ?? []).Select(block => new Placement(block))
                .Concat((Gbx.AnchoredObjects ?? []).Select(item => new Placement(item)));

        internal IEnumerable<Placement> Terrain =>
            (Gbx.Blocks ?? []).Concat(Gbx.BakedBlocks ?? []).Where(block => !block.IsFree).Select(block => new Placement(block));

        internal List<Placement> Of(Selector selector) => Placements.Where(selector.Matches).ToList();

        internal List<Placement> Waypoints(Waypoint waypoint) =>
            Placements.Where(placement => placement.IsWaypoint && placement.Waypoint == waypoint).ToList();

        internal void Remove(Placement placement)
        {
            if (placement.Block is not null)
            {
                Gbx.RemoveBlock(placement.Block);
            }
            else
            {
                Gbx.AnchoredObjects?.Remove(placement.Item!);
            }
        }

        public void GenerateUid() => Gbx.GenerateMapUid();
    }
}
