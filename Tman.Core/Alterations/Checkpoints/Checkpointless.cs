using System.Collections.Generic;
using Tman.Core.Alterations.Steps;

namespace Tman.Core.Alterations.Checkpoints
{
    public sealed class Checkpointless : StandardAlteration
    {

        protected override string DisplayName => "Checkpointless";

        protected override IEnumerable<Alteration> Steps =>
        [
            new InventoryRemoved(placement => placement.IsCheckpoint)
        ];
    }
}
