using System.Collections.Generic;
using Tman.Core.Alterations.Steps;
using Tman.Core.Catalog;
using Tman.Core.Surfaces;

namespace Tman.Core.Alterations.Other
{
    internal sealed class Invisible : StandardAlteration
    {
        protected override string DisplayName => "Invisible";

        protected override IEnumerable<Alteration> Steps => For(Surface.Invisible);

        internal static IEnumerable<Alteration> For(Surface surface) =>
        [
            new VistaMoodWarning(),
            new ItemsMadeInvisible(surface),
            new TerrainMadeInvisible(surface),
            new BakedClipsMadeInvisible(surface),
            new SurfaceBlocksPlaced(block => block.Waypoint == Waypoint.None ? new SurfaceBlock(surface, block) : null),
            new WaypointsFreed(),
            new BakedClipsRemoved()
        ];
    }
}
