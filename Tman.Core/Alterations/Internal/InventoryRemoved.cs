using System;
using System.Linq;
using Tman.Core.Inventory;
using Tman.Core.Maps;

namespace Tman.Core.Alterations.Internal
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
