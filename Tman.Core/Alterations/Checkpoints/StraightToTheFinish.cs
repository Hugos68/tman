using System.Collections.Generic;
using Tman.Core.Alterations.Internal;

namespace Tman.Core.Alterations.Checkpoints
{
    public class StraightToTheFinish : Alteration
    {
        protected override IEnumerable<Alteration> PreappliedAlterations =>
        [
            new ValidationRemoved(),
            new LightmapRemoved(),
            new CheckpointsCleaned(),
            new MapNameEdited(name => name + " Straight to the Finish")
        ];
    }
}