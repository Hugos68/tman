using System.Collections.Generic;
using Tman.Core.Alterations.Steps;
using Tman.Core.Inventory;
using Tman.Core.Maps;

namespace Tman.Core.Alterations.Finishes
{
    public class Manslaughter : StandardAlteration
    {
        protected override string DisplayName => "Manslaughter";

        protected override IEnumerable<Alteration> Steps =>
        [
            new InventoryRemoved(placement => placement.IsFinish),
            new CheckpointsCleaned(),
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
