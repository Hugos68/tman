using System.Collections.Generic;
using Tman.Core.Alterations.Steps.Waypoints;
using Tman.Core.Catalog;

namespace Tman.Core.Alterations.Effects
{
    public class NoSteer : StandardAlteration
    {
        protected override string DisplayName => "NoSteer";

        protected override IEnumerable<Alteration> Steps =>
        [
            new StartEffect(Blocks.GateSpecialNoSteering, new(0,0,0)),
            new CheckpointEffect(Blocks.GateSpecialNoSteering, new(0, 0, 0))
        ];
    }
}
