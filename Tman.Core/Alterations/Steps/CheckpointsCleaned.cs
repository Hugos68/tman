using System.Collections.Generic;
using System.Linq;
using Tman.Core.Inventory;
using Tman.Core.Maps;

namespace Tman.Core.Alterations.Steps
{
    public class CheckpointsCleaned : Alteration
    {
        protected override IEnumerable<Alteration> PreappliedAlterations =>
        [
            new InventoryRemoved(
                placement => placement.IsCheckpoint && (!placement.IsBlock || placement.BlockModel == Blocks.GateCheckpoint)
            ),
        ];

        private static readonly Dictionary<Block, (Block Block, DirectionOffset Rotation)> BlockReplacements = new()
        {
            // Open Dirt Road
            [Blocks.OpenDirtRoadCheckpoint] = (Blocks.OpenDirtRoadStraight, DirectionOffset.None),
            [Blocks.OpenDirtRoadCheckpointSlope2Down] = (Blocks.OpenDirtRoadSlope2Straight, DirectionOffset.Half),
            [Blocks.OpenDirtRoadCheckpointSlope2Left] = (Blocks.OpenDirtRoadStraightTilt2, DirectionOffset.Quarter),
            [Blocks.OpenDirtRoadCheckpointSlope2Right] = (Blocks.OpenDirtRoadStraightTilt2, DirectionOffset.ThreeQuarters),
            [Blocks.OpenDirtRoadCheckpointSlope2Up] = (Blocks.OpenDirtRoadSlope2Straight, DirectionOffset.None),

            // Open Grass Road
            [Blocks.OpenGrassRoadCheckpoint] = (Blocks.OpenGrassRoadStraight, DirectionOffset.None),
            [Blocks.OpenGrassRoadCheckpointSlope2Down] = (Blocks.OpenGrassRoadSlope2Straight, DirectionOffset.Half),
            [Blocks.OpenGrassRoadCheckpointSlope2Left] = (Blocks.OpenGrassRoadStraightTilt2, DirectionOffset.Quarter),
            [Blocks.OpenGrassRoadCheckpointSlope2Right] = (Blocks.OpenGrassRoadStraightTilt2, DirectionOffset.ThreeQuarters),
            [Blocks.OpenGrassRoadCheckpointSlope2Up] = (Blocks.OpenGrassRoadSlope2Straight, DirectionOffset.None),

            // Open Ice Road
            [Blocks.OpenIceRoadCheckpoint] = (Blocks.OpenIceRoadStraight, DirectionOffset.None),
            [Blocks.OpenIceRoadCheckpointSlope2Down] = (Blocks.OpenIceRoadSlope2Straight, DirectionOffset.Half),
            [Blocks.OpenIceRoadCheckpointSlope2Left] = (Blocks.OpenIceRoadStraightTilt2, DirectionOffset.Quarter),
            [Blocks.OpenIceRoadCheckpointSlope2Right] = (Blocks.OpenIceRoadStraightTilt2, DirectionOffset.ThreeQuarters),
            [Blocks.OpenIceRoadCheckpointSlope2Up] = (Blocks.OpenIceRoadSlope2Straight, DirectionOffset.None),

            // Open Tech Road
            [Blocks.OpenTechRoadCheckpoint] = (Blocks.OpenTechRoadStraight, DirectionOffset.None),
            [Blocks.OpenTechRoadCheckpointSlope2Down] = (Blocks.OpenTechRoadSlope2Straight, DirectionOffset.Half),
            [Blocks.OpenTechRoadCheckpointSlope2Left] = (Blocks.OpenTechRoadStraightTilt2, DirectionOffset.Quarter),
            [Blocks.OpenTechRoadCheckpointSlope2Right] = (Blocks.OpenTechRoadStraightTilt2, DirectionOffset.ThreeQuarters),
            [Blocks.OpenTechRoadCheckpointSlope2Up] = (Blocks.OpenTechRoadSlope2Straight, DirectionOffset.None),

            // Platform Dirt
            [Blocks.PlatformDirtCheckpoint] = (Blocks.PlatformDirtBase, DirectionOffset.None),
            [Blocks.PlatformDirtCheckpointSlope2Down] = (Blocks.PlatformDirtSlope2Straight, DirectionOffset.Half),
            [Blocks.PlatformDirtCheckpointSlope2Left] = (Blocks.PlatformDirtSlope2Straight, DirectionOffset.Quarter),
            [Blocks.PlatformDirtCheckpointSlope2Right] = (Blocks.PlatformDirtSlope2Straight, DirectionOffset.ThreeQuarters),
            [Blocks.PlatformDirtCheckpointSlope2Up] = (Blocks.PlatformDirtSlope2Straight, DirectionOffset.None),
            [Blocks.PlatformDirtWallCheckpointDown] = (Blocks.PlatformDirtWallStraight4, DirectionOffset.Quarter),
            [Blocks.PlatformDirtWallCheckpointLeft] = (Blocks.PlatformDirtWallStraight4, DirectionOffset.Quarter),
            [Blocks.PlatformDirtWallCheckpointRight] = (Blocks.PlatformDirtWallStraight4, DirectionOffset.Quarter),
            [Blocks.PlatformDirtWallCheckpointUp] = (Blocks.PlatformDirtWallStraight4, DirectionOffset.Quarter),

            // Platform Grass
            [Blocks.PlatformGrassCheckpoint] = (Blocks.PlatformGrassBase, DirectionOffset.None),
            [Blocks.PlatformGrassCheckpointSlope2Down] = (Blocks.PlatformGrassSlope2Straight, DirectionOffset.Half),
            [Blocks.PlatformGrassCheckpointSlope2Left] = (Blocks.PlatformGrassSlope2Straight, DirectionOffset.Quarter),
            [Blocks.PlatformGrassCheckpointSlope2Right] = (Blocks.PlatformGrassSlope2Straight, DirectionOffset.ThreeQuarters),
            [Blocks.PlatformGrassCheckpointSlope2Up] = (Blocks.PlatformGrassSlope2Straight, DirectionOffset.None),
            [Blocks.PlatformGrassWallCheckpointDown] = (Blocks.PlatformGrassWallStraight4, DirectionOffset.Quarter),
            [Blocks.PlatformGrassWallCheckpointLeft] = (Blocks.PlatformGrassWallStraight4, DirectionOffset.Quarter),
            [Blocks.PlatformGrassWallCheckpointRight] = (Blocks.PlatformGrassWallStraight4, DirectionOffset.Quarter),
            [Blocks.PlatformGrassWallCheckpointUp] = (Blocks.PlatformGrassWallStraight4, DirectionOffset.Quarter),

            // Platform Ice
            [Blocks.PlatformIceCheckpoint] = (Blocks.PlatformIceBase, DirectionOffset.None),
            [Blocks.PlatformIceCheckpointSlope2Down] = (Blocks.PlatformIceSlope2Straight, DirectionOffset.Half),
            [Blocks.PlatformIceCheckpointSlope2Left] = (Blocks.PlatformIceSlope2Straight, DirectionOffset.Quarter),
            [Blocks.PlatformIceCheckpointSlope2Right] = (Blocks.PlatformIceSlope2Straight, DirectionOffset.ThreeQuarters),
            [Blocks.PlatformIceCheckpointSlope2Up] = (Blocks.PlatformIceSlope2Straight, DirectionOffset.None),
            [Blocks.PlatformIceWallCheckpointDown] = (Blocks.PlatformIceWallStraight4, DirectionOffset.Quarter),
            [Blocks.PlatformIceWallCheckpointLeft] = (Blocks.PlatformIceWallStraight4, DirectionOffset.Quarter),
            [Blocks.PlatformIceWallCheckpointRight] = (Blocks.PlatformIceWallStraight4, DirectionOffset.Quarter),
            [Blocks.PlatformIceWallCheckpointUp] = (Blocks.PlatformIceWallStraight4, DirectionOffset.Quarter),

            // Platform Plastic
            [Blocks.PlatformPlasticCheckpoint] = (Blocks.PlatformPlasticBase, DirectionOffset.None),
            [Blocks.PlatformPlasticCheckpointSlope2Down] = (Blocks.PlatformPlasticSlope2Straight, DirectionOffset.Half),
            [Blocks.PlatformPlasticCheckpointSlope2Left] = (Blocks.PlatformPlasticSlope2Straight, DirectionOffset.Quarter),
            [Blocks.PlatformPlasticCheckpointSlope2Right] = (Blocks.PlatformPlasticSlope2Straight, DirectionOffset.ThreeQuarters),
            [Blocks.PlatformPlasticCheckpointSlope2Up] = (Blocks.PlatformPlasticSlope2Straight, DirectionOffset.None),
            [Blocks.PlatformPlasticWallCheckpointDown] = (Blocks.PlatformPlasticWallStraight4, DirectionOffset.Quarter),
            [Blocks.PlatformPlasticWallCheckpointLeft] = (Blocks.PlatformPlasticWallStraight4, DirectionOffset.Quarter),
            [Blocks.PlatformPlasticWallCheckpointRight] = (Blocks.PlatformPlasticWallStraight4, DirectionOffset.Quarter),
            [Blocks.PlatformPlasticWallCheckpointUp] = (Blocks.PlatformPlasticWallStraight4, DirectionOffset.Quarter),

            // Platform Tech
            [Blocks.PlatformTechCheckpoint] = (Blocks.PlatformTechBase, DirectionOffset.None),
            [Blocks.PlatformTechCheckpointSlope2Down] = (Blocks.PlatformTechSlope2Straight, DirectionOffset.Half),
            [Blocks.PlatformTechCheckpointSlope2Left] = (Blocks.PlatformTechSlope2Straight, DirectionOffset.Quarter),
            [Blocks.PlatformTechCheckpointSlope2Right] = (Blocks.PlatformTechSlope2Straight, DirectionOffset.ThreeQuarters),
            [Blocks.PlatformTechCheckpointSlope2Up] = (Blocks.PlatformTechSlope2Straight, DirectionOffset.None),
            [Blocks.PlatformTechWallCheckpointDown] = (Blocks.PlatformTechWallStraight4, DirectionOffset.Quarter),
            [Blocks.PlatformTechWallCheckpointLeft] = (Blocks.PlatformTechWallStraight4, DirectionOffset.Quarter),
            [Blocks.PlatformTechWallCheckpointRight] = (Blocks.PlatformTechWallStraight4, DirectionOffset.Quarter),
            [Blocks.PlatformTechWallCheckpointUp] = (Blocks.PlatformTechWallStraight4, DirectionOffset.Quarter),

            // Platform Water
            [Blocks.PlatformWaterCheckpoint] = (Blocks.PlatformWaterBase, DirectionOffset.None),

            // Road Bump
            [Blocks.RoadBumpCheckpoint] = (Blocks.RoadBumpStraight, DirectionOffset.None),
            [Blocks.RoadBumpCheckpointSlopeDown] = (Blocks.RoadBumpSlopeStraight, DirectionOffset.Half),
            [Blocks.RoadBumpCheckpointSlopeUp] = (Blocks.RoadBumpSlopeStraight, DirectionOffset.None),
            [Blocks.RoadBumpCheckpointTiltLeft] = (Blocks.RoadBumpTiltStraight, DirectionOffset.Half),
            [Blocks.RoadBumpCheckpointTiltRight] = (Blocks.RoadBumpTiltStraight, DirectionOffset.None),
            [Blocks.RoadBumpDiagLeftCheckpoint] = (Blocks.RoadBumpDiagLeftStraightX2, DirectionOffset.None),
            [Blocks.RoadBumpDiagRightCheckpoint] = (Blocks.RoadBumpDiagRightStraightX2, DirectionOffset.None),

            // Road Dirt
            [Blocks.RoadDirtCheckpoint] = (Blocks.RoadDirtStraight, DirectionOffset.None),
            [Blocks.RoadDirtCheckpointSlopeDown] = (Blocks.RoadDirtSlopeStraight, DirectionOffset.Half),
            [Blocks.RoadDirtCheckpointSlopeUp] = (Blocks.RoadDirtSlopeStraight, DirectionOffset.None),
            [Blocks.RoadDirtCheckpointTiltLeft] = (Blocks.RoadDirtTiltStraight, DirectionOffset.Half),
            [Blocks.RoadDirtCheckpointTiltRight] = (Blocks.RoadDirtTiltStraight, DirectionOffset.None),
            [Blocks.RoadDirtDiagLeftCheckpoint] = (Blocks.RoadDirtDiagLeftStraightX2, DirectionOffset.None),
            [Blocks.RoadDirtDiagRightCheckpoint] = (Blocks.RoadDirtDiagRightStraightX2, DirectionOffset.None),

            // Road Ice
            [Blocks.RoadIceCheckpoint] = (Blocks.RoadIceStraight, DirectionOffset.None),
            [Blocks.RoadIceCheckpointSlopeDown] = (Blocks.RoadIceSlopeStraight, DirectionOffset.Half),
            [Blocks.RoadIceCheckpointSlopeUp] = (Blocks.RoadIceSlopeStraight, DirectionOffset.None),

            // Road Ice With Wall
            [Blocks.RoadIceWithWallCheckpointLeft] = (Blocks.RoadIceWithWallStraight, DirectionOffset.None),
            [Blocks.RoadIceWithWallCheckpointRight] = (Blocks.RoadIceWithWallStraight, DirectionOffset.Half),
            [Blocks.RoadIceWithWallDiagLeftCheckpointLeft] = (Blocks.RoadIceWithWallDiagLeftSwitchLeft, DirectionOffset.None),
            [Blocks.RoadIceWithWallDiagLeftCheckpointRight] = (Blocks.RoadIceWithWallDiagLeftSwitchLeft, DirectionOffset.None),
            [Blocks.RoadIceWithWallDiagRightCheckpointLeft] = (Blocks.RoadIceWithWallDiagRightSwitchLeft, DirectionOffset.None),
            [Blocks.RoadIceWithWallDiagRightCheckpointRight] = (Blocks.RoadIceWithWallDiagRightSwitchLeft, DirectionOffset.None),

            // Road Tech
            [Blocks.RoadTechCheckpoint] = (Blocks.RoadTechStraight, DirectionOffset.None),
            [Blocks.RoadTechCheckpointSlopeDown] = (Blocks.RoadTechSlopeStraight, DirectionOffset.Half),
            [Blocks.RoadTechCheckpointSlopeUp] = (Blocks.RoadTechSlopeStraight, DirectionOffset.None),
            [Blocks.RoadTechCheckpointTiltLeft] = (Blocks.RoadTechTiltStraight, DirectionOffset.Half),
            [Blocks.RoadTechCheckpointTiltRight] = (Blocks.RoadTechTiltStraight, DirectionOffset.None),
            [Blocks.RoadTechDiagLeftCheckpoint] = (Blocks.RoadTechDiagLeftStraightX2, DirectionOffset.None),
            [Blocks.RoadTechDiagRightCheckpoint] = (Blocks.RoadTechDiagRightStraightX2, DirectionOffset.None),

            // Road Water
            [Blocks.RoadWaterCheckpoint] = (Blocks.RoadWaterStraight, DirectionOffset.None),
        };

        protected override void Run(Map map)
        {
            foreach (var placement in map.Placements.OnlyBlocks().Where(placement => placement.BlockModel?.Waypoint == Waypoint.Checkpoint).ToList())
            {
                if (!BlockReplacements.TryGetValue(placement.BlockModel!, out var replacement))
                {
                    throw new KeyNotFoundException($"No replacement found for block '{placement.Id}'");
                }
                map.Replace(placement, replacement.Block, replacement.Rotation);
            }
        }
    }
}
