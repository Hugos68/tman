using System.Collections.Generic;
using tman.src.Alterations.Helpers;

namespace tman.src.Alterations
{
    public class Freewheel : Alteration
    {
        protected override IEnumerable<Alteration> PreappliedAlterations =>
        [
            new WithoutValidation(),
            new StartEffect("Turbo", new(0, 0, 0)),
            new StartEffect("NoEngine", new(0, 0, 2)),
            new CheckpointEffect("NoEngine", new(0, 0, -2)),
            new CheckpointEffect("Turbo", new(0, 0, 0)),
            new CheckpointEffect("NoEngine", new(0, 0, 2)),
        ];
    }
}