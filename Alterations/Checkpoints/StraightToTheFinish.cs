using System.Collections.Generic;
using tman.Alterations.Internal;
using tman.Inventory;
namespace tman.Alterations.Checkpoints
{
    public class StraightToTheFinish : Alteration
    {
        protected override IEnumerable<Alteration> PreappliedAlterations =>
        [
            new ValidationRemoved(),
            new LightmapRemoved(),
            new CheckpointsCleaned(),
            new MapNameEdited(name => name + " Straight to the Finish")
        ];
    }
}