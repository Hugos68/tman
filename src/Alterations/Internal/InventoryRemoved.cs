using System;
using System.Linq;
using tman.src.Core;
using tman.src.Maps;

namespace tman.src.Alterations.Internal
{
    public class InventoryRemoved(Func<Placement, bool> predicate) : Alteration
    {
        protected override void Run(Map map)
        {
            foreach (var placement in map.Placements.Where(predicate).ToList())
            {
                map.Remove(placement);
            }
        }
    }
}
