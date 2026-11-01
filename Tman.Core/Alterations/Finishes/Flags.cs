using System.Collections.Generic;
using Tman.Core.Alterations.Steps;
using Tman.Core.Alterations.Steps.Waypoints;
using Tman.Core.Catalog;
using Tman.Core.Maps.Placements;

namespace Tman.Core.Alterations.Finishes
{
    public sealed class Flags : StandardAlteration
    {
        protected override string DisplayName => "Flags";

        protected override IEnumerable<Alteration> Steps =>
        [
            new PlacementsRemoved(placement => placement.IsFinish),
            new CheckpointsCleaned(),
            new RelativelyPlacedItems(new Dictionary<Selector, Item>
            {
                [Items.Flag8m] = Custom.FinishFlag8m,
                [Items.Flag16m] = Custom.FinishFlag16m,
            })
        ];
    }
}
