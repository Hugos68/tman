using System.Collections.Generic;
using GBX.NET.Engines.Game;
using tman.src.Alterations.Helpers;
using tman.src.Inventory;

namespace tman.src.Alterations
{
    public class CheckpointsRemoved : Alteration
    {

        protected override IEnumerable<Alteration> PreappliedAlterations =>
        [
            new WithoutValidation(),
            new WithoutInventory(InventoryUtility.IsCheckpoint, InventoryUtility.IsCheckpoint),
            new StraightToTheFinish(),
        ];
    }
}