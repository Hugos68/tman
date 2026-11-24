using System.Linq;
using Microsoft.Extensions.Logging;
using Tman.Core.Catalog;
using Tman.Core.Maps;
using Tman.Core.Surfaces;

namespace Tman.Core.Alterations.Steps
{
    internal sealed class BakedClipsMadeInvisible : Alteration
    {
        protected override void Run(Map map, AlterationContext context)
        {
            var terrain = map.TerrainKinds;
            var baked = map.Baked.Where(placement => !terrain.Contains(placement.Id)).ToList();
            var replaced = baked.Count(placement => context.Replace(map, placement, new SurfaceBlock(Surface.Invisible, placement.BlockModel ?? Block.Unlisted(placement.Id))));
            context.Logger.LogInformation("{Step}: {Replaced} of {Baked} baked blocks made invisible", GetType().Name, replaced, baked.Count);
        }
    }
}
