using System.Collections.Generic;
using Tman.Core.Alterations.Steps;
using Tman.Core.Catalog;
using Tman.Core.Surfaces;

namespace Tman.Core.Alterations.Other
{
    internal sealed class Invisible : StandardAlteration
    {
        protected override string DisplayName => "Invisible";

        protected override IEnumerable<Alteration> Steps =>
        [
            new VistaMoodWarning(),
            new ItemsMadeInvisible(),
            new TerrainMadeInvisible(),
            new SurfaceBlocksPlaced(block => block.Waypoint == Waypoint.None ? new SurfaceBlock(Surface.Invisible, block) : null),
            new BakedClipsMadeInvisible(),
            new WaypointsFreed(),
            new BakedClipsRemoved()
        ];
    }
}
