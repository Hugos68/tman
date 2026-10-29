using System.Collections.Generic;
using tman.Alterations.Internal;
using tman.Inventory;

namespace tman.Alterations.Checkpoints
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