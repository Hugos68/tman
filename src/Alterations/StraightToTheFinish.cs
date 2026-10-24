using System.Collections.Generic;
using tman.src.Alterations.Internal;
using tman.src.Core;
namespace tman.src.Alterations
{
    public class StraightToTheFinish : Alteration
    {
        protected override IEnumerable<Alteration> PreappliedAlterations =>
        [
            new ValidationRemoved(),
            new CheckpointsCleaned(),
            new MapNameEdited(name => name + " Straight to the Finish")
        ];
    }
}