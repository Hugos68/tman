using System.Collections.Generic;
using Tman.Core.Alterations.Steps;
using Tman.Core.Inventory;

namespace Tman.Core.Alterations.Effects
{
    public class SlowMotion : StandardAlteration
    {
        protected override string DisplayName => "SlowMotion";

        protected override IEnumerable<Alteration> Steps =>
        [
            new StartEffect(Blocks.GateSpecialSlowMotion, new(0,0,0)),
            new CheckpointEffect(Blocks.GateSpecialSlowMotion, new(0, 0, 0))
        ];
    }
}
