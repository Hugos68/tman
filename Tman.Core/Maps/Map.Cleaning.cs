using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using GBX.NET;
using GBX.NET.Engines.Game;
using Tman.Core.Maps.Placements;

namespace Tman.Core.Maps
{
    public sealed partial class Map
    {
        internal void RemoveValidation()
        {
            var gbx = Challenge;
            gbx.AuthorTime = new(-1);
            gbx.GoldTime = new(-1);
            gbx.SilverTime = new(-1);
            gbx.BronzeTime = new(-1);
            gbx.ChallengeParameters?.RaceValidateGhost = null;
            if (gbx.Xml is not null)
            {
                var header = XElement.Parse(gbx.Xml);
                header.Element("desc")?.SetAttributeValue("validated", "0");
                foreach (var time in new[] { "bronze", "silver", "gold", "authortime" })
                {
                    header.Element("times")?.SetAttributeValue(time, "-1");
                }
                gbx.Xml = header.ToString(SaveOptions.DisableFormatting);
            }
        }

        internal void RemoveBakedClips() => Challenge.BakedClipsAdditionalData?.Clear();

        internal IReadOnlySet<string> TerrainKinds =>
            (Challenge.ZoneGenealogy ?? []).SelectMany(column => (column.ZoneIds ?? []).Append(column.CurrentZoneId)).OfType<string>().ToHashSet();

        internal Placement? Ocean(IReadOnlySet<string> kinds)
        {
            var flat = Terrain.Select(placement => placement.Block!).Where(block => kinds.Contains(block.Name) && Column(block.Coord) is { CurrentIndex: 0 } column && column.CurrentZoneId == block.Name).ToList();
            if (flat.Count == 0)
            {
                return null;
            }
            var floor = flat.Min(block => block.Coord.Y);
            return new Placement(flat.Where(block => block.Coord.Y == floor).GroupBy(block => (block.Name, block.Direction, block.Flags)).MaxBy(group => group.Count())!.First());
        }

        internal static bool IsOcean(Placement placement, Placement ocean) =>
            placement.Block is { } block && ocean.Block is { } water && block.Name == water.Name && block.Coord.Y == water.Coord.Y && block.Direction == water.Direction && block.Flags == water.Flags;

        internal void Flood(Placement ocean)
        {
            var water = ocean.Block!;
            var columns = Challenge.ZoneGenealogy ?? [];
            var source = Column(water.Coord)!;
            var (zones, zone, direction) = (source.ZoneIds ?? [], source.CurrentZoneId, source.Dir);
            foreach (var placement in Terrain.Where(placement => IsOcean(placement, ocean)).ToList())
            {
                Remove(placement);
            }
            Challenge.BakedBlocks ??= [];
            for (var index = 0; index < columns.Count; index++)
            {
                columns[index].ZoneIds = [.. zones];
                columns[index].CurrentZoneId = zone;
                columns[index].CurrentIndex = 0;
                columns[index].Dir = direction;
                Challenge.BakedBlocks.Add(new CGameCtnBlock { Name = water.Name, Coord = new Int3(index / Depth, water.Coord.Y, index % Depth), Direction = water.Direction, Flags = water.Flags });
            }
        }

        private int Depth => (int)Challenge.Size.Z;

        private CGameCtnZoneGenealogy? Column(Int3 cell) => Challenge.ZoneGenealogy?.ElementAtOrDefault(cell.X * Depth + cell.Z);

        internal void RemoveLightmap()
        {
            var gbx = Challenge;
            gbx.HasLightmaps = false;
            gbx.LightmapCache = null;
            gbx.LightmapFrames = null;
            gbx.LightmapCacheSmall = null;
            gbx.LightmapCacheData = null;
        }
    }
}
