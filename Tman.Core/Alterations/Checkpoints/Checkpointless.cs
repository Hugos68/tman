using System.Collections.Generic;
using Tman.Core.Alterations.Steps;
using Tman.Core.Inventory;

namespace Tman.Core.Alterations.Checkpoints
{
    public class Checkpointless : StandardAlteration
    {

        protected override string DisplayName => "Checkpointless";

        protected override IEnumerable<Alteration> Steps =>
        [
            new InventoryRemoved(placement => placement.IsCheckpoint)
        ];
    }
}
