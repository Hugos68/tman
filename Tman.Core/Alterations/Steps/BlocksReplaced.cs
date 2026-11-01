using System;
using Tman.Core.Catalog;
using Tman.Core.Maps;
using Tman.Core.Maps.Placements;

namespace Tman.Core.Alterations.Steps
{
    internal sealed class BlocksReplaced(Func<Block, Block?> replacement) : BlockReplacement<Block>(replacement)
    {
        protected override void Replace(Map map, Placement placement, Block target) => map.Replace(placement, target);
    }
}
