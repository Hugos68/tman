using System;
using System.Collections.Generic;
using System.Linq;
using tman.src.Core;
using tman.src.Maps;

namespace tman.src.Alterations.Internal
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
