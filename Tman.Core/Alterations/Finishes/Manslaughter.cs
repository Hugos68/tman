using System.Collections.Generic;
using Tman.Core.Alterations.Internal;
using Tman.Core.Inventory;
using Tman.Core.Maps;

namespace Tman.Core.Alterations.Finishes
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
            new RelativelyPlacedItems(new Dictionary<Selector, Item>
            {
                [Blocks.StandCornerIn] = Custom.FinishStandCornerIn,
                [Blocks.StandCornerInOnDecoWall] = Custom.FinishStandCornerIn,
                [Blocks.StandCornerOut] = Custom.FinishStandCornerOut,
                [Blocks.StandCornerOutOnDecoWall] = Custom.FinishStandCornerOut,
                [Blocks.StandDiagIn] = Custom.FinishStandDiagIn,
                [Blocks.StandDiagInOnDecoWall] = Custom.FinishStandDiagIn,
                [Blocks.StandDiagOut] = Custom.FinishStandDiagOut,
                [Blocks.StandDiagOutOnDecoWall] = Custom.FinishStandDiagOut,
                [Blocks.StandStraight] = Custom.FinishStandStraight,
                [Blocks.StandStraightOnDecoWall] = Custom.FinishStandStraight,
            })
        ];
    }
}
