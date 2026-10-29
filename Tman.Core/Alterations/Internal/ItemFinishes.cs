using System;
using System.Collections.Generic;
using System.Linq;
using Tman.Inventory;
using Tman.Maps;

namespace Tman.Alterations.Internal
{
    public class ItemFinishes(IReadOnlyDictionary<Selector, Item> finishes, Func<Placement, bool>? filter = null) : Alteration
    {
        protected override void Run(Map map)
        {
            foreach (var (selector, finish) in finishes)
            {
                var sources = map.Of(selector).Where(placement => filter is null || filter(placement)).ToList();
                if (sources.Count == 0)
                {
                    continue;
                }
                map.Embed(finish);
                foreach (var source in sources)
                {
                    map.PlaceRelative(source, finish);
                }
            }
        }
    }
}
