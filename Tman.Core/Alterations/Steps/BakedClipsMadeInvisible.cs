using System;
using System.Collections.Generic;
using System.Linq;
using GBX.NET;
using Microsoft.Extensions.Logging;
using Tman.Core.Catalog;
using Tman.Core.Maps;
using Tman.Core.Maps.Placements;
using Tman.Core.Surfaces;

namespace Tman.Core.Alterations.Steps
{
    internal sealed class BakedClipsMadeInvisible(Surface surface) : Alteration
    {
        private const string Special = "Special";

        private const string InsideSpecial = "InsideSpecial";

        private static readonly Int3[] Neighbours = [new(0, 1, 0), new(0, -1, 0), new(1, 0, 0), new(-1, 0, 0), new(0, 0, 1), new(0, 0, -1)];

        protected override void Run(Map map, AlterationContext context)
        {
            var terrain = map.TerrainKinds;
            var specials = map.Placements.OnlyBlocks().Where(placement => !placement.IsFree && placement.BlockModel is not null && placement.Id.Contains(Special, StringComparison.Ordinal)).GroupBy(placement => placement.Cell).ToDictionary(group => group.Key, group => group.First().BlockModel!);
            var baked = map.Baked.Where(placement => !terrain.Contains(placement.Id)).ToList();
            var replaced = baked.Count(placement => context.Replace(map, placement, new SurfaceBlock(surface, placement.BlockModel ?? Block.Unlisted(placement.Id), Owner(placement, specials))));
            context.Logger.LogInformation("{Step}: {Replaced} of {Baked} baked blocks made invisible", GetType().Name, replaced, baked.Count);
        }

        private static Block? Owner(Placement clip, Dictionary<Int3, Block> specials) =>
            clip.Id.Contains(InsideSpecial, StringComparison.Ordinal) ? Neighbours.Select(offset => specials.GetValueOrDefault(clip.Cell + offset)).FirstOrDefault(owner => owner is not null) : null;
    }
}
