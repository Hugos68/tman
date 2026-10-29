using System.Collections.Generic;
using Tman.Alterations.Internal;
using Tman.Inventory;

namespace Tman.Alterations.Effects
{
    public class SlowMotion : Alteration
    {
        protected override IEnumerable<Alteration> PreappliedAlterations =>
        [
            new ValidationRemoved(),
            new LightmapRemoved(),
            new StartEffect(Blocks.GateSpecialSlowMotion, new(0,0,0)),
            new CheckpointEffect(Blocks.GateSpecialSlowMotion, new(0, 0, 0)),
            new MapNameEdited(name => name + " SlowMotion")
        ];
    }
}