using System.Collections.Generic;
using Tman.Core.Alterations.Steps;
using Tman.Core.Alterations.Steps.Waypoints;
using Tman.Core.Catalog;
using Tman.Core.Maps.Placements;

namespace Tman.Core.Alterations.Finishes
{
    internal sealed class Podium : StandardAlteration
    {
        protected override string DisplayName => "Podium";

        protected override IEnumerable<Alteration> Steps =>
        [
            new PlacementsRemoved(placement => placement.IsFinish),
            new CheckpointsCleaned(),
            new RelativelyPlacedItems(new Dictionary<Selector, Item>
            {
                [Items.Podium] = Custom.FinishPodium,
            })
        ];
    }
}
