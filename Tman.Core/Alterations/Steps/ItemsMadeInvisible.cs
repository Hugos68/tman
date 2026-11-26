using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Tman.Core.Catalog;
using Tman.Core.Maps;
using Tman.Core.Maps.Placements;
using Tman.Core.Surfaces;

namespace Tman.Core.Alterations.Steps
{
    internal sealed class ItemsMadeInvisible(Surface surface) : Alteration
    {
        private const float UnitScale = 1;

        private const float ScaleTolerance = 1e-4f;

        private const string ItemExtension = ".Item.Gbx";

        protected override void Run(Map map, AlterationContext context)
        {
            var items = map.Placements.OnlyItems().Where(placement => !placement.IsWaypoint).ToList();
            var replaced = items.Count(placement => MathF.Abs(placement.Item!.Scale - UnitScale) < ScaleTolerance && context.Replace(map, placement, new SurfaceBlock(surface, Block.Unlisted(Name(placement.Id)))));
            context.Logger.LogInformation("{Step}: {Replaced} of {Items} items made invisible", GetType().Name, replaced, items.Count);
        }

        private static string Name(string id)
        {
            var name = id.Split('\\')[^1];
            return name.EndsWith(ItemExtension, StringComparison.OrdinalIgnoreCase) ? name[..^ItemExtension.Length] : name;
        }
    }
}
