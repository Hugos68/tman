using System.Collections.Generic;
using Tman.Alterations.Internal;
using Tman.Inventory;
using Tman.Maps;

namespace Tman.Alterations.Finishes
{
    public class Flags : Alteration
    {
        protected override IEnumerable<Alteration> PreappliedAlterations =>
        [
            new ValidationRemoved(),
            new LightmapRemoved(),
            new InventoryRemoved(placement => placement.IsFinish),
            new CheckpointsCleaned(),
            new MapNameEdited(name => name + " Flags"),
            new ItemFinishes(new Dictionary<Selector, Item>
            {
                [Items.Flag8m] = Custom.FinishFlag8m,
                [Items.Flag16m] = Custom.FinishFlag16m,
            })
        ];
    }
}
