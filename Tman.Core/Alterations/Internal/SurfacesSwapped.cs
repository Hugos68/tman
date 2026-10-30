using System.Linq;
using Tman.Core.Inventory;
using Tman.Core.Maps;

namespace Tman.Core.Alterations.Internal
{
    public class SurfacesSwapped(Surface surface) : Alteration
    {
        protected override void Run(Map map)
        {
            foreach (var placement in map.Placements.Where(placement => placement.IsBlock && Blocks.Surfaces.ContainsKey((placement.Id, surface))).ToList())
            {
                map.Replace(placement, Blocks.Surfaces[(placement.Id, surface)], DirectionOffset.None);
            }
        }
    }
}
