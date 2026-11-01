using System.Collections.Generic;
using Tman.Core.Alterations.Steps.Waypoints;

namespace Tman.Core.Alterations.Checkpoints
{
    public class StraightToTheFinish : StandardAlteration
    {
        protected override string DisplayName => "Straight to the Finish";

        protected override IEnumerable<Alteration> Steps =>
        [
            new CheckpointsCleaned()
        ];
    }
}
