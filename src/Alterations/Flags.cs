using System.Collections.Generic;
using System.IO;
using tman.src.Alterations.Internal;
using tman.src.Application;
using tman.src.Core;
using tman.src.Inventory;

namespace tman.src.Alterations
{
    public class Flags : Alteration
    {
        protected override IEnumerable<Alteration> PreappliedAlterations =>
        [
            new ValidationRemoved(),
            new LightmapRemoved(),
            new InventoryRemoved(InventoryUtility.IsFinish, InventoryUtility.IsFinish),
            new CheckpointsCleaned(),
            new MapNameEdited(name => name + " Flags"),
            new ItemFinishes(new Dictionary<string, string>
            {
                ["Flag8m"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "flags", "FinishFlag8m.Item.Gbx"),
                ["Flag16m"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "flags", "FinishFlag16m.Item.Gbx"),
            })
        ];
    }
}
