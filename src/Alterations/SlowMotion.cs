using System.Collections.Generic;
using tman.src.Alterations.Internal;

namespace tman.src.Alterations
{
    public class SlowMotion : Alteration
    {
        protected override IEnumerable<Alteration> PreappliedAlterations =>
        [
            new WithoutValidation(),
            new StartEffect("SlowMotion", new(0,0,0)),
            new CheckpointEffect("SlowMotion", new(0, 0, 0)),
            new MapNameEdited(name => name + " SlowMotion")
        ];
    }
}