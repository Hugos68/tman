using System.Collections.Generic;
using System.IO;
using tman.src.Alterations.Internal;
using tman.src.Application;
using tman.src.Core;
using tman.src.Inventory;

namespace tman.src.Alterations
{
    public class Manslaughter : Alteration
    {
        protected override IEnumerable<Alteration> PreappliedAlterations =>
        [
            new ValidationRemoved(),
            new LightmapRemoved(),
            new InventoryRemoved(InventoryUtility.IsFinish, InventoryUtility.IsFinish),
            new CheckpointsCleaned(),
            new MapNameEdited(name => name + " Manslaughter"),
            new ItemFinishes(new Dictionary<string, string>
            {
                ["StandCornerIn"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "manslaughter", "FinishStandCornerIn.Item.Gbx"),
                ["StandCornerOut"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "manslaughter", "FinishStandCornerOut.Item.Gbx"),
                ["StandDiagIn"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "manslaughter", "FinishStandDiagIn.Item.Gbx"),
                ["StandDiagOut"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "manslaughter", "FinishStandDiagOut.Item.Gbx"),
                ["StandStraight"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "manslaughter", "FinishStandStraight.Item.Gbx"),
            })
        ];
    }
}
