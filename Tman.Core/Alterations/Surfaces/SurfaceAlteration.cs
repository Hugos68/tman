using System.Collections.Generic;
using Tman.Core.Alterations.Steps;
using Microsoft.Extensions.Logging;
using Tman.Core.Catalog;
using Tman.Core.Maps;
using Tman.Core.Surfaces;

namespace Tman.Core.Alterations.Surfaces
{
    internal abstract class SurfaceAlteration : StandardAlteration
    {
        protected abstract Surface Surface { get; }

        protected override string DisplayName => Surface.Name;

        protected override IEnumerable<Alteration> Steps =>
        [
            new VistaMoodWarning(),
            new BlocksReplaced(SurfaceRules.PlainOfPenalty),
            new BlocksReplaced(block => SurfaceRules.PlatformTwin(block, Surface)),
            new SurfaceBlocksPlaced(block => SurfaceRules.SurfaceBlockOf(block, Surface))
        ];
    }
}
