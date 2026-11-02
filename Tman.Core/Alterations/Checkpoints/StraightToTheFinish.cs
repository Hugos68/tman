using System.Collections.Generic;
using Tman.Core.Alterations.Steps.Waypoints;

namespace Tman.Core.Alterations.Checkpoints
{
    internal sealed class StraightToTheFinish : StandardAlteration
    {
        protected override string DisplayName => "Straight to the Finish";

        protected override IEnumerable<Alteration> Steps =>
        [
            new CheckpointsCleaned()
        ];
    }
}
