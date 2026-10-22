using System.Collections.Generic;
using tman.src.Alterations.Helpers;
namespace tman.src.Alterations
{
    public class StraightToTheFinish : Alteration
    {
        protected override IEnumerable<Alteration> PreappliedAlterations =>
        [
            new WithoutValidation(),
            new CheckpointsCleaned()
        ];
    }
}