using System.Collections.Generic;
using tman.src.Alterations.Internal;
using tman.src.Core;
using tman.src.Inventory;

namespace tman.src.Alterations
{
    public class Checkpointless : Alteration
    {

        protected override IEnumerable<Alteration> PreappliedAlterations =>
        [
            new ValidationRemoved(),
            new LightmapRemoved(),
            new InventoryRemoved(InventoryUtility.IsCheckpoint, InventoryUtility.IsCheckpoint),
            new MapNameEdited(name => name + " Checkpointless")
        ];
    }
}