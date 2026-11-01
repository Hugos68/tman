using System;
using Tman.Core.Catalog;
using Tman.Core.Maps;
using Tman.Core.Maps.Placements;
using Tman.Core.Surfaces;

namespace Tman.Core.Alterations.Steps
{
    internal sealed class SurfaceBlocksPlaced(Func<Block, SurfaceBlock?> replacement) : BlockReplacement<SurfaceBlock>(replacement)
    {
        protected override void Replace(Map map, Placement placement, SurfaceBlock target) => map.Replace(placement, target);
    }
}
