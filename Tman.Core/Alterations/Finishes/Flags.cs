using System.Collections.Generic;
using Tman.Core.Alterations.Internal;
using Tman.Core.Inventory;
using Tman.Core.Maps;

namespace Tman.Core.Alterations.Finishes
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
            new RelativelyPlacedItems(new Dictionary<Selector, Item>
            {
                [Items.Flag8m] = Custom.FinishFlag8m,
                [Items.Flag16m] = Custom.FinishFlag16m,
            })
        ];
    }
}
