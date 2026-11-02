using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Tman.Core.Maps;
using Tman.Core.Maps.Placements;

namespace Tman.Core.Alterations.Steps
{
    internal sealed class PlacementsRemoved(Predicate<Placement> predicate) : Alteration
    {
        protected override void Run(Map map, AlterationContext context)
        {
            var removed = map.Placements.Where(placement => predicate(placement)).ToList();
            foreach (var placement in removed)
            {
                map.Remove(placement);
            }
            context.Logger.LogInformation("{Count} placements removed", removed.Count);
        }
    }
}
