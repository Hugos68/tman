using System;
using System.Linq;
using Tman.Core.Inventory;
using Tman.Core.Maps;

namespace Tman.Core.Alterations.Steps
{
    public class BlocksReplaced(Func<Block, Block?> replacement) : Alteration
    {
        protected override void Run(Map map)
        {
            foreach (var placement in map.Placements.OnlyBlocks().ToList())
            {
                if (placement.BlockModel is { } model && replacement(model) is { } target)
                {
                    map.Replace(placement, target);
                }
            }
        }
    }
}
