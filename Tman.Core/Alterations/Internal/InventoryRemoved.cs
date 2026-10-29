using System;
using System.Linq;
using Tman.Inventory;
using Tman.Maps;

namespace Tman.Alterations.Internal
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
