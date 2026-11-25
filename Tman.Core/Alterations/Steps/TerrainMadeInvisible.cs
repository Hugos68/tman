using System.Linq;
using Microsoft.Extensions.Logging;
using Tman.Core.Catalog;
using Tman.Core.Maps;
using Tman.Core.Maps.Placements;
using Tman.Core.Surfaces;

namespace Tman.Core.Alterations.Steps
{
    internal sealed class TerrainMadeInvisible(Surface surface) : Alteration
    {
        protected override void Run(Map map, AlterationContext context)
        {
            if (map.Collection == Collections.Stadium)
            {
                return;
            }
            var kinds = map.TerrainKinds;
            var blocks = map.Placements.OnlyBlocks().Where(placement => !placement.IsFree).ToList();
            var roads = blocks.Where(placement => placement.Block!.IsGround && placement.BlockModel is { } model && (placement.IsWaypoint || DeckFamily.Of(model) is not null));
            var covered = roads.SelectMany(placement => placement.Cells()).ToHashSet();
            var hidden = blocks.Where(placement => !placement.IsWaypoint && kinds.Contains(placement.Id) && covered.Contains((placement.Cell.X, placement.Cell.Z))).ToList();
            foreach (var placement in hidden)
            {
                map.Remove(placement);
            }
            var ocean = map.Ocean(kinds);
            var terrain = blocks.Except(hidden).Concat(map.Baked).Where(placement => !placement.IsWaypoint && kinds.Contains(placement.Id) && (ocean is null || !Map.IsOcean(placement, ocean))).ToList();
            var invisible = terrain.Count(placement => context.Replace(map, placement, new SurfaceBlock(surface, placement.BlockModel ?? Block.Unlisted(placement.Id))));
            if (ocean is not null)
            {
                map.Flood(ocean);
            }
            context.Logger.LogInformation("{Step}: {Invisible} of {Terrain} terrain blocks made invisible, {Hidden} under roads, platforms and waypoints removed, {Ocean} filling the map", GetType().Name, invisible, terrain.Count, hidden.Count, ocean?.Id ?? "no open water");
        }
    }
}
