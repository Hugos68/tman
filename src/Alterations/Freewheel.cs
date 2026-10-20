using System.Linq;
using GBX.NET.Engines.Game;
using tman.src.Positioning;

namespace tman.src.Alterations
{
    public class Freewheel : IAlteration
    {
        public void Apply(CGameCtnChallenge map)
        {
            var checkpointBlocks = map.Blocks
                .Where(block => block.BlockModel.Id.Contains("Checkpoint"))
                .ToList();
            foreach (var checkpointBlock in checkpointBlocks)
            {
                BlockPlacementUtility.PlaceRelative(map, checkpointBlock, new(){ Name = "GateSpecialNoEngine" }, new(0,0,0));
                BlockPlacementUtility.PlaceRelative(map, checkpointBlock, new(){ Name = "GateSpecialTurbo" }, new(0,0,0));
            }
            map.MapName += " Freewheel";
        }
    }
}