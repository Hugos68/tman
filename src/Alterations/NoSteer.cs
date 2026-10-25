using System.Collections.Generic;
using tman.src.Alterations.Internal;
using tman.src.Core;

namespace tman.src.Alterations
{
    public class NoSteer : Alteration
    {
        protected override IEnumerable<Alteration> PreappliedAlterations =>
        [
            new ValidationRemoved(),
            new LightmapRemoved(),
            new StartEffect("NoSteer", new(0,0,0)),
            new CheckpointEffect("NoSteer", new(0, 0, 0)),
            new MapNameEdited(name => name + " NoSteer")
        ];
    }
}