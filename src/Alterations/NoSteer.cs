using System.Collections.Generic;
using tman.src.Alterations.Internal;

namespace tman.src.Alterations
{
    public class NoSteer : Alteration
    {
        protected override IEnumerable<Alteration> PreappliedAlterations =>
        [
            new WithoutValidation(),
            new StartEffect("NoSteer", new(0,0,0)),
            new CheckpointEffect("NoSteer", new(0, 0, 0)),
            new MapNameEdited(name => name + " NoSteer")
        ];
    }
}