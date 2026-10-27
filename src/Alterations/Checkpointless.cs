using System.Collections.Generic;
using tman.Alterations.Internal;
using tman.Core;
using tman.Maps;

namespace tman.Alterations
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