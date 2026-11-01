using System;
using System.Linq;
using Tman.Core.Maps;
using Tman.Core.Maps.Placements;

namespace Tman.Core.Alterations.Steps
{
    internal sealed class InventoryRemoved(Func<Placement, bool> predicate) : Alteration
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
