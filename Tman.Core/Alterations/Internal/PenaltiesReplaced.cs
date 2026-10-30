using System.Linq;
using Tman.Core.Inventory;
using Tman.Core.Maps;

namespace Tman.Core.Alterations.Internal
{
    public class PenaltiesReplaced : Alteration
    {
        protected override void Run(Map map)
        {
            foreach (var placement in map.Placements.Where(placement => placement.IsBlock && Blocks.PenaltyOf.ContainsKey(placement.Id)).ToList())
            {
                map.Replace(placement, Blocks.PenaltyOf[placement.Id], DirectionOffset.None);
            }
        }
    }
}
