using System.Collections.Generic;
using Tman.Core.Alterations.Internal;
using Tman.Core.Inventory;

namespace Tman.Core.Alterations.Checkpoints
{
    public class Checkpointless : Alteration
    {

        protected override IEnumerable<Alteration> PreappliedAlterations =>
        [
            new ValidationRemoved(),
            new LightmapRemoved(),
            new InventoryRemoved(placement => placement.IsCheckpoint),
            new MapNameEdited(name => name + " Checkpointless")
        ];
    }
}