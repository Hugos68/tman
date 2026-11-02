using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Tman.Core.Catalog;
using Tman.Core.Maps;
using Tman.Core.Maps.Placements;

namespace Tman.Core.Alterations.Steps
{
    internal sealed class RelativelyPlacedItems(IReadOnlyDictionary<Selector, Item> selectorItemDictionary, Func<Placement, bool>? filter = null) : Alteration
    {
        protected override void Run(Map map, AlterationContext context)
        {
            foreach (var (selector, item) in selectorItemDictionary)
            {
                var sources = map.Of(selector).Where(placement => filter is null || filter(placement)).ToList();
                if (sources.Count == 0)
                {
                    continue;
                }
                context.Embed(map, item);
                foreach (var source in sources)
                {
                    map.PlaceRelative(source, item);
                }
                context.Logger.LogInformation("{Item} placed on {Count} placements", item.Id, sources.Count);
            }
        }
    }
}
