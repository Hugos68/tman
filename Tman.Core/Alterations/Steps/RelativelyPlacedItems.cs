using System;
using System.Collections.Generic;
using System.Linq;
using Tman.Core.Inventory;
using Tman.Core.Maps;

namespace Tman.Core.Alterations.Steps
{
    public class RelativelyPlacedItems(IReadOnlyDictionary<Selector, Item> selectorItemDictionary, Func<Placement, bool>? filter = null) : Alteration
    {
        protected override void Run(Map map)
        {
            foreach (var (selector, item) in selectorItemDictionary)
            {
                var sources = map.Of(selector).Where(placement => filter is null || filter(placement)).ToList();
                if (sources.Count == 0)
                {
                    continue;
                }
                map.Embed(item);
                foreach (var source in sources)
                {
                    map.PlaceRelative(source, item);
                }
            }
        }
    }
}
