using System.Collections.Generic;
using Tman.Core.Alterations.Steps.Waypoints;
using Tman.Core.Catalog;

namespace Tman.Core.Alterations.Effects
{
    public sealed class Freewheel : StandardAlteration
    {
        protected override string DisplayName => "Freewheel";

        protected override IEnumerable<Alteration> Steps =>
        [
            new StartEffect(Blocks.GateSpecialTurbo, new(0, 0, 2)),
            new StartEffect(Blocks.GateSpecialNoEngine, new(0, 0, 4)),
            new CheckpointEffect(Blocks.GateSpecialNoEngine, new(0, 0,-2)),
            new CheckpointEffect(Blocks.GateSpecialTurbo, new(0, 0, 2)),
            new CheckpointEffect(Blocks.GateSpecialNoEngine, new(0, 0, 4))
        ];
    }
}
