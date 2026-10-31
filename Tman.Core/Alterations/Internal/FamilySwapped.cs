using System.Linq;
using Tman.Core.Inventory;
using Tman.Core.Maps;

namespace Tman.Core.Alterations.Internal
{
    public class FamilySwapped(string from, string to) : Alteration
    {
        protected override void Run(Map map)
        {
            foreach (var placement in map.Placements.Where(placement => placement.IsBlock && placement.Id.StartsWith(from)).ToList())
            {
                var twin = Blocks.FromId(to + placement.Id.Substring(from.Length));
                if (twin is null || placement.BlockModel is not { } model || twin.SizeX != model.SizeX || twin.SizeZ != model.SizeZ)
                {
                    continue;
                }
                map.Replace(placement, twin, DirectionOffset.None);
            }
        }
    }
}
