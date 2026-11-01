using System.Collections.Generic;
using Tman.Core.Alterations.Steps;
using Tman.Core.Inventory;
using Tman.Core.Maps;

namespace Tman.Core.Alterations.Finishes
{
    public class Flags : StandardAlteration
    {
        protected override string DisplayName => "Flags";

        protected override IEnumerable<Alteration> Steps =>
        [
            new InventoryRemoved(placement => placement.IsFinish),
            new CheckpointsCleaned(),
            new RelativelyPlacedItems(new Dictionary<Selector, Item>
            {
                [Items.Flag8m] = Custom.FinishFlag8m,
                [Items.Flag16m] = Custom.FinishFlag16m,
            })
        ];
    }
}
