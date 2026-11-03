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
            new BlocksReplaced(SurfaceRules.PlainOfPenalty),
            new BlocksReplaced(block => SurfaceRules.PlatformTwin(block, Surface)),
            new SurfaceBlocksPlaced(block => SurfaceRules.SurfaceBlockOf(block, Surface))
        ];

        protected override void Run(Map map, AlterationContext context)
        {
            if (map.Collection != Collections.Stadium)
            {
                context.Logger.LogWarning("{Collection} map with embedded blocks: the game crashes when the editor switches its mood; play it, or set the mood before altering", map.Collection);
            }
        }
    }
}
