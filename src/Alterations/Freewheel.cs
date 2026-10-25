using System.Collections.Generic;
using tman.src.Alterations.Internal;
using tman.src.Core;

namespace tman.src.Alterations
{
    public class Freewheel : Alteration
    {
        protected override IEnumerable<Alteration> PreappliedAlterations =>
        [
            new ValidationRemoved(),
            new LightmapRemoved(),
            new StartEffect("Turbo", new(0, 0, 2)),
            new StartEffect("NoEngine", new(0, 0, 4)),
            new CheckpointEffect("NoEngine", new(0, 0,-2)),
            new CheckpointEffect("Turbo", new(0, 0, 2)),
            new CheckpointEffect("NoEngine", new(0, 0, 4)),
            new MapNameEdited(name => name + " Freewheel")
        ];
    }
}