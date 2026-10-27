using System.Collections.Generic;
using tman.src.Alterations.Internal;
using tman.src.Core;

namespace tman.src.Alterations
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