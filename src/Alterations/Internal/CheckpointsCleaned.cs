using System.Collections.Generic;
using System.Linq;
using GBX.NET.Engines.Game;
using tman.src.Core;
using tman.src.Inventory;
using tman.src.Positioning;

namespace tman.src.Alterations.Internal
{
    public class CheckpointsCleaned : Alteration
    {
        protected override IEnumerable<Alteration> PreappliedAlterations =>
        [
            new InventoryRemoved(
                blocks: block => InventoryUtility.IsCheckpoint(block) && block.BlockModel.Id == "GateCheckpoint",
                items: InventoryUtility.IsCheckpoint
            ),
        ];

        private static readonly Dictionary<string, (string Name, DirectionOffset Rotation)> BlockReplacements = new()
        {
            // Open Dirt Road
            ["OpenDirtRoadCheckpoint"] = ("OpenDirtRoadStraight", DirectionOffset.None),
            ["OpenDirtRoadCheckpointSlope2Down"] = ("OpenDirtRoadSlope2Straight", DirectionOffset.Half),
            ["OpenDirtRoadCheckpointSlope2Left"] = ("OpenDirtRoadStraightTilt2", DirectionOffset.Quarter),
            ["OpenDirtRoadCheckpointSlope2Right"] = ("OpenDirtRoadStraightTilt2", DirectionOffset.ThreeQuarters),
            ["OpenDirtRoadCheckpointSlope2Up"] = ("OpenDirtRoadSlope2Straight", DirectionOffset.None),

            // Open Grass Road
            ["OpenGrassRoadCheckpoint"] = ("OpenGrassRoadStraight", DirectionOffset.None),
            ["OpenGrassRoadCheckpointSlope2Down"] = ("OpenGrassRoadSlope2Straight", DirectionOffset.Half),
            ["OpenGrassRoadCheckpointSlope2Left"] = ("OpenGrassRoadStraightTilt2", DirectionOffset.Quarter),
            ["OpenGrassRoadCheckpointSlope2Right"] = ("OpenGrassRoadStraightTilt2", DirectionOffset.ThreeQuarters),
            ["OpenGrassRoadCheckpointSlope2Up"] = ("OpenGrassRoadSlope2Straight", DirectionOffset.None),

            // Open Ice Road
            ["OpenIceRoadCheckpoint"] = ("OpenIceRoadStraight", DirectionOffset.None),
            ["OpenIceRoadCheckpointSlope2Down"] = ("OpenIceRoadSlope2Straight", DirectionOffset.Half),
            ["OpenIceRoadCheckpointSlope2Left"] = ("OpenIceRoadStraightTilt2", DirectionOffset.Quarter),
            ["OpenIceRoadCheckpointSlope2Right"] = ("OpenIceRoadStraightTilt2", DirectionOffset.ThreeQuarters),
            ["OpenIceRoadCheckpointSlope2Up"] = ("OpenIceRoadSlope2Straight", DirectionOffset.None),

            // Open Tech Road
            ["OpenTechRoadCheckpoint"] = ("OpenTechRoadStraight", DirectionOffset.None),
            ["OpenTechRoadCheckpointSlope2Down"] = ("OpenTechRoadSlope2Straight", DirectionOffset.Half),
            ["OpenTechRoadCheckpointSlope2Left"] = ("OpenTechRoadStraightTilt2", DirectionOffset.Quarter),
            ["OpenTechRoadCheckpointSlope2Right"] = ("OpenTechRoadStraightTilt2", DirectionOffset.ThreeQuarters),
            ["OpenTechRoadCheckpointSlope2Up"] = ("OpenTechRoadSlope2Straight", DirectionOffset.None),

            // Platform Dirt
            ["PlatformDirtCheckpoint"] = ("PlatformDirtBase", DirectionOffset.None),
            ["PlatformDirtCheckpointSlope2Down"] = ("PlatformDirtSlope2Straight", DirectionOffset.Half),
            ["PlatformDirtCheckpointSlope2Left"] = ("PlatformDirtSlope2Straight", DirectionOffset.Quarter),
            ["PlatformDirtCheckpointSlope2Right"] = ("PlatformDirtSlope2Straight", DirectionOffset.ThreeQuarters),
            ["PlatformDirtCheckpointSlope2Up"] = ("PlatformDirtSlope2Straight", DirectionOffset.None),
            ["PlatformDirtWallCheckpointDown"] = ("PlatformDirtWallStraight4", DirectionOffset.Quarter),
            ["PlatformDirtWallCheckpointLeft"] = ("PlatformDirtWallStraight4", DirectionOffset.Quarter),
            ["PlatformDirtWallCheckpointRight"] = ("PlatformDirtWallStraight4", DirectionOffset.Quarter),
            ["PlatformDirtWallCheckpointUp"] = ("PlatformDirtWallStraight4", DirectionOffset.Quarter),

            // Platform Grass
            ["PlatformGrassCheckpoint"] = ("PlatformGrassBase", DirectionOffset.None),
            ["PlatformGrassCheckpointSlope2Down"] = ("PlatformGrassSlope2Straight", DirectionOffset.Half),
            ["PlatformGrassCheckpointSlope2Left"] = ("PlatformGrassSlope2Straight", DirectionOffset.Quarter),
            ["PlatformGrassCheckpointSlope2Right"] = ("PlatformGrassSlope2Straight", DirectionOffset.ThreeQuarters),
            ["PlatformGrassCheckpointSlope2Up"] = ("PlatformGrassSlope2Straight", DirectionOffset.None),
            ["PlatformGrassWallCheckpointDown"] = ("PlatformGrassWallStraight4", DirectionOffset.Quarter),
            ["PlatformGrassWallCheckpointLeft"] = ("PlatformGrassWallStraight4", DirectionOffset.Quarter),
            ["PlatformGrassWallCheckpointRight"] = ("PlatformGrassWallStraight4", DirectionOffset.Quarter),
            ["PlatformGrassWallCheckpointUp"] = ("PlatformGrassWallStraight4", DirectionOffset.Quarter),

            // Platform Ice
            ["PlatformIceCheckpoint"] = ("PlatformIceBase", DirectionOffset.None),
            ["PlatformIceCheckpointSlope2Down"] = ("PlatformIceSlope2Straight", DirectionOffset.Half),
            ["PlatformIceCheckpointSlope2Left"] = ("PlatformIceSlope2Straight", DirectionOffset.Quarter),
            ["PlatformIceCheckpointSlope2Right"] = ("PlatformIceSlope2Straight", DirectionOffset.ThreeQuarters),
            ["PlatformIceCheckpointSlope2Up"] = ("PlatformIceSlope2Straight", DirectionOffset.None),
            ["PlatformIceWallCheckpointDown"] = ("PlatformIceWallStraight4", DirectionOffset.Quarter),
            ["PlatformIceWallCheckpointLeft"] = ("PlatformIceWallStraight4", DirectionOffset.Quarter),
            ["PlatformIceWallCheckpointRight"] = ("PlatformIceWallStraight4", DirectionOffset.Quarter),
            ["PlatformIceWallCheckpointUp"] = ("PlatformIceWallStraight4", DirectionOffset.Quarter),

            // Platform Plastic
            ["PlatformPlasticCheckpoint"] = ("PlatformPlasticBase", DirectionOffset.None),
            ["PlatformPlasticCheckpointSlope2Down"] = ("PlatformPlasticSlope2Straight", DirectionOffset.Half),
            ["PlatformPlasticCheckpointSlope2Left"] = ("PlatformPlasticSlope2Straight", DirectionOffset.Quarter),
            ["PlatformPlasticCheckpointSlope2Right"] = ("PlatformPlasticSlope2Straight", DirectionOffset.ThreeQuarters),
            ["PlatformPlasticCheckpointSlope2Up"] = ("PlatformPlasticSlope2Straight", DirectionOffset.None),
            ["PlatformPlasticWallCheckpointDown"] = ("PlatformPlasticWallStraight4", DirectionOffset.Quarter),
            ["PlatformPlasticWallCheckpointLeft"] = ("PlatformPlasticWallStraight4", DirectionOffset.Quarter),
            ["PlatformPlasticWallCheckpointRight"] = ("PlatformPlasticWallStraight4", DirectionOffset.Quarter),
            ["PlatformPlasticWallCheckpointUp"] = ("PlatformPlasticWallStraight4", DirectionOffset.Quarter),

            // Platform Tech
            ["PlatformTechCheckpoint"] = ("PlatformTechBase", DirectionOffset.None),
            ["PlatformTechCheckpointSlope2Down"] = ("PlatformTechSlope2Straight", DirectionOffset.Half),
            ["PlatformTechCheckpointSlope2Left"] = ("PlatformTechSlope2Straight", DirectionOffset.Quarter),
            ["PlatformTechCheckpointSlope2Right"] = ("PlatformTechSlope2Straight", DirectionOffset.ThreeQuarters),
            ["PlatformTechCheckpointSlope2Up"] = ("PlatformTechSlope2Straight", DirectionOffset.None),
            ["PlatformTechWallCheckpointDown"] = ("PlatformTechWallStraight4", DirectionOffset.Quarter),
            ["PlatformTechWallCheckpointLeft"] = ("PlatformTechWallStraight4", DirectionOffset.Quarter),
            ["PlatformTechWallCheckpointRight"] = ("PlatformTechWallStraight4", DirectionOffset.Quarter),
            ["PlatformTechWallCheckpointUp"] = ("PlatformTechWallStraight4", DirectionOffset.Quarter),

            // Platform Water
            ["PlatformWaterCheckpoint"] = ("PlatformWaterBase", DirectionOffset.None),

            // Road Bump
            ["RoadBumpCheckpoint"] = ("RoadBumpStraight", DirectionOffset.None),
            ["RoadBumpCheckpointSlopeDown"] = ("RoadBumpSlopeStraight", DirectionOffset.Half),
            ["RoadBumpCheckpointSlopeUp"] = ("RoadBumpSlopeStraight", DirectionOffset.None),
            ["RoadBumpCheckpointTiltLeft"] = ("RoadBumpTiltStraight", DirectionOffset.Half),
            ["RoadBumpCheckpointTiltRight"] = ("RoadBumpTiltStraight", DirectionOffset.None),
            ["RoadBumpDiagLeftCheckpoint"] = ("RoadBumpDiagLeftStraightX2", DirectionOffset.None),
            ["RoadBumpDiagRightCheckpoint"] = ("RoadBumpDiagRightStraightX2", DirectionOffset.None),

            // Road Dirt
            ["RoadDirtCheckpoint"] = ("RoadDirtStraight", DirectionOffset.None),
            ["RoadDirtCheckpointSlopeDown"] = ("RoadDirtSlopeStraight", DirectionOffset.Half),
            ["RoadDirtCheckpointSlopeUp"] = ("RoadDirtSlopeStraight", DirectionOffset.None),
            ["RoadDirtCheckpointTiltLeft"] = ("RoadDirtTiltStraight", DirectionOffset.Half),
            ["RoadDirtCheckpointTiltRight"] = ("RoadDirtTiltStraight", DirectionOffset.None),
            ["RoadDirtDiagLeftCheckpoint"] = ("RoadDirtDiagLeftStraightX2", DirectionOffset.None),
            ["RoadDirtDiagRightCheckpoint"] = ("RoadDirtDiagRightStraightX2", DirectionOffset.None),

            // Road Ice
            ["RoadIceCheckpoint"] = ("RoadIceStraight", DirectionOffset.None),
            ["RoadIceCheckpointSlopeDown"] = ("RoadIceSlopeStraight", DirectionOffset.Half),
            ["RoadIceCheckpointSlopeUp"] = ("RoadIceSlopeStraight", DirectionOffset.None),

            // Road Ice With Wall
            ["RoadIceWithWallCheckpointLeft"] = ("RoadIceWithWallStraight", DirectionOffset.None),
            ["RoadIceWithWallCheckpointRight"] = ("RoadIceWithWallStraight", DirectionOffset.Half),
            ["RoadIceWithWallDiagLeftCheckpointLeft"] = ("RoadIceWithWallDiagLeftSwitchLeft", DirectionOffset.None),
            ["RoadIceWithWallDiagLeftCheckpointRight"] = ("RoadIceWithWallDiagLeftSwitchLeft", DirectionOffset.None),
            ["RoadIceWithWallDiagRightCheckpointLeft"] = ("RoadIceWithWallDiagRightSwitchLeft", DirectionOffset.None),
            ["RoadIceWithWallDiagRightCheckpointRight"] = ("RoadIceWithWallDiagRightSwitchLeft", DirectionOffset.None),

            // Road Tech
            ["RoadTechCheckpoint"] = ("RoadTechStraight", DirectionOffset.None),
            ["RoadTechCheckpointSlopeDown"] = ("RoadTechSlopeStraight", DirectionOffset.Half),
            ["RoadTechCheckpointSlopeUp"] = ("RoadTechSlopeStraight", DirectionOffset.None),
            ["RoadTechCheckpointTiltLeft"] = ("RoadTechTiltStraight", DirectionOffset.Half),
            ["RoadTechCheckpointTiltRight"] = ("RoadTechTiltStraight", DirectionOffset.None),
            ["RoadTechDiagLeftCheckpoint"] = ("RoadTechDiagLeftStraightX2", DirectionOffset.None),
            ["RoadTechDiagRightCheckpoint"] = ("RoadTechDiagRightStraightX2", DirectionOffset.None),

            // Road Water
            ["RoadWaterCheckpoint"] = ("RoadWaterStraight", DirectionOffset.None),
        };

        protected override void Run(CGameCtnChallenge map)
        {
            var checkpointBlocks = map.Blocks?
                .Where(block => block.BlockModel.Id.Contains("Checkpoint"))
                .ToList() ?? [];
            foreach (var block in checkpointBlocks)
            {
                if (!BlockReplacements.TryGetValue(block.BlockModel.Id, out var blockReplacement)) throw new KeyNotFoundException($"No replacement found for block ID '{block.BlockModel.Id}'");

                map.RemoveBlock(block);
                map.PlaceBlock(new()
                {
                    Name = blockReplacement.Name,
                    Coord = block.Coord,
                    Direction = DirectionUtility.CalculateDirection(block.Direction, blockReplacement.Rotation),
                    IsGround = block.IsGround,
                    Variant = block.Variant,
                    SubVariant = block.SubVariant,
                    YawPitchRoll = block.YawPitchRoll,
                    Color = block.Color,
                    IsClip = block.IsClip,
                    IsFree = block.IsFree,
                    IsPillar = block.IsPillar,
                    IsGhost = block.IsGhost,
                    IsReplacement = block.IsReplacement
                });
            }
        }
    }
}