using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Tman.Core.Maps;
using Tman.Core.Maps.Placements;
using Tman.Core.Surfaces;

namespace Tman.Core.Alterations.Steps
{
    internal sealed class TerrainDentsKept : Alteration
    {
        private const string On = "On";

        protected override void Run(Map map, AlterationContext context)
        {
            var blocks = map.Placements.OnlyBlocks().Where(placement => !placement.IsFree).ToList();
            var byCell = blocks.ToLookup(placement => placement.Cell);
            var kept = 0;
            foreach (var placement in blocks.Where(placement => !placement.IsWaypoint && placement.BlockModel is not null && DeckFamily.Of(placement.BlockModel) is { IsOpenRoad: true, NativeSurface: { } native } && native == Surface.Road))
            {
                if (byCell[placement.Cell].Any(terrain => terrain != placement && placement.Id.Contains(On + terrain.Id, StringComparison.Ordinal))
                    && context.Replace(map, placement, new SurfaceBlock(Surface.Terrain, placement.BlockModel!)))
                {
                    kept++;
                }
            }
            context.Logger.LogInformation("{Step}: {Kept} open roads set in the terrain keep its dent", GetType().Name, kept);
        }
    }
}
