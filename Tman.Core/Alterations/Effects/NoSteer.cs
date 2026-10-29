using System.Collections.Generic;
using tman.Alterations.Internal;
using tman.Inventory;

namespace tman.Alterations.Effects
{
    public class NoSteer : Alteration
    {
        protected override IEnumerable<Alteration> PreappliedAlterations =>
        [
            new ValidationRemoved(),
            new LightmapRemoved(),
            new StartEffect(Blocks.GateSpecialNoSteering, new(0,0,0)),
            new CheckpointEffect(Blocks.GateSpecialNoSteering, new(0, 0, 0)),
            new MapNameEdited(name => name + " NoSteer")
        ];
    }
}