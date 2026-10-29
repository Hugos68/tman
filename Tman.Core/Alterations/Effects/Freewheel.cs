using System.Collections.Generic;
using Tman.Alterations.Internal;
using Tman.Inventory;

namespace Tman.Alterations.Effects
{
    public class Freewheel : Alteration
    {
        protected override IEnumerable<Alteration> PreappliedAlterations =>
        [
            new ValidationRemoved(),
            new LightmapRemoved(),
            new StartEffect(Blocks.GateSpecialTurbo, new(0, 0, 2)),
            new StartEffect(Blocks.GateSpecialNoEngine, new(0, 0, 4)),
            new CheckpointEffect(Blocks.GateSpecialNoEngine, new(0, 0,-2)),
            new CheckpointEffect(Blocks.GateSpecialTurbo, new(0, 0, 2)),
            new CheckpointEffect(Blocks.GateSpecialNoEngine, new(0, 0, 4)),
            new MapNameEdited(name => name + " Freewheel")
        ];
    }
}