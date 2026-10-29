using System;
using System.Linq;
using tman.Inventory;
using tman.Maps;

namespace tman.Alterations.Internal
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
