using System.Collections.Generic;
using tman.src.Alterations.Internal;
using tman.src.Core;
using tman.Maps;

namespace tman.src.Alterations
{
    public class Manslaughter : Alteration
    {
        protected override IEnumerable<Alteration> PreappliedAlterations =>
        [
            new ValidationRemoved(),
            new LightmapRemoved(),
            new InventoryRemoved(placement => placement.IsFinish),
            new CheckpointsCleaned(),
            new MapNameEdited(name => name + " Manslaughter"),
            new ItemFinishes(new Dictionary<Selector, Item>
            {
                [Blocks.StandCornerIn] = Custom.FinishStandCornerIn,
                [Blocks.StandCornerOut] = Custom.FinishStandCornerOut,
                [Blocks.StandDiagIn] = Custom.FinishStandDiagIn,
                [Blocks.StandDiagOut] = Custom.FinishStandDiagOut,
                [Blocks.StandStraight] = Custom.FinishStandStraight,
            })
        ];
    }
}
