using System.Collections.Generic;
using Tman.Core.Alterations.Steps;
using Tman.Core.Inventory;

namespace Tman.Core.Alterations.Surfaces
{
    public abstract class SurfaceAlteration : StandardAlteration
    {
        protected abstract Surface Surface { get; }

        protected override string DisplayName => Surface.Name;

        protected override IEnumerable<Alteration> Steps =>
        [
            new BlocksReplaced(SurfaceRules.PlainOfPenalty),
            new BlocksReplaced(block => SurfaceRules.PlatformTwin(block, Surface)),
            new SurfaceBlocksPlaced(block => SurfaceRules.SurfaceBlockOf(block, Surface))
        ];
    }
}
