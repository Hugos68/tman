using Microsoft.Extensions.Logging;
using Tman.Core.Catalog;
using Tman.Core.Maps;

namespace Tman.Core.Alterations.Steps
{
    internal sealed class VistaMoodWarning : Alteration
    {
        protected override void Run(Map map, AlterationContext context)
        {
            if (map.Collection != Collections.Stadium)
            {
                context.Logger.LogWarning("{Collection} map with embedded blocks: the game crashes when the editor switches its mood; play it, or set the mood before altering", map.Collection);
            }
        }
    }
}
