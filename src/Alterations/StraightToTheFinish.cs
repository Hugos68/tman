using System.Collections.Generic;
using tman.src.Alterations.Internal;
namespace tman.src.Alterations
{
    public class StraightToTheFinish : Alteration
    {
        protected override IEnumerable<Alteration> PreappliedAlterations =>
        [
            new WithoutValidation(),
            new CheckpointsCleaned(),
            new MapNameEdited(name => name + " Straight to the Finish")
        ];
    }
}