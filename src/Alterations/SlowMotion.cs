using System.Collections.Generic;
using tman.src.Alterations.Internal;
using tman.src.Core;

namespace tman.src.Alterations
{
    public class SlowMotion : Alteration
    {
        protected override IEnumerable<Alteration> PreappliedAlterations =>
        [
            new ValidationRemoved(),
            new StartEffect("SlowMotion", new(0,0,0)),
            new CheckpointEffect("SlowMotion", new(0, 0, 0)),
            new MapNameEdited(name => name + " SlowMotion")
        ];
    }
}