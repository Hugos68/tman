using System.Collections.Generic;
using System.Linq;
using GBX.NET.Engines.Game;
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
            // Road Tech
            ["RoadTechCheckpoint"] = ("RoadTechStraight", DirectionOffset.None),
            ["RoadTechCheckpointSlopeUp"] = ("RoadTechSlopeStraight", DirectionOffset.None),
            ["RoadTechCheckpointSlopeDown"] = ("RoadTechSlopeStraight", DirectionOffset.Half),
            ["RoadTechCheckpointTiltLeft"] = ("RoadTechTiltStraight", DirectionOffset.Half),
            ["RoadTechCheckpointTiltRight"] = ("RoadTechTiltStraight", DirectionOffset.None),
            ["RoadTechDiagRightCheckpoint"] = ("RoadTechDiagRightStraightX2", DirectionOffset.None),
            ["RoadTechDiagLeftCheckpoint"] = ("RoadTechDiagLeftStraightX2", DirectionOffset.None),

            // Road Dirt
            ["RoadDirtCheckpoint"] = ("RoadDirtStraight", DirectionOffset.None),
            ["RoadDirtCheckpointSlopeUp"] = ("RoadDirtSlopeStraight", DirectionOffset.None),
            ["RoadDirtCheckpointSlopeDown"] = ("RoadDirtSlopeStraight", DirectionOffset.Half),
            ["RoadDirtCheckpointTiltLeft"] = ("RoadDirtTiltStraight", DirectionOffset.Half),
            ["RoadDirtCheckpointTiltRight"] = ("RoadDirtTiltStraight", DirectionOffset.None),

            // Road Bump
            ["RoadBumpCheckpoint"] = ("RoadBumpStraight", DirectionOffset.None),
            ["RoadBumpCheckpointSlopeUp"] = ("RoadBumpSlopeStraight", DirectionOffset.None),
            ["RoadBumpCheckpointSlopeDown"] = ("RoadBumpSlopeStraight", DirectionOffset.Half),
            ["RoadBumpCheckpointTiltLeft"] = ("RoadBumpTiltStraight", DirectionOffset.Half),
            ["RoadBumpCheckpointTiltRight"] = ("RoadBumpTiltStraight", DirectionOffset.None),

            // Road Ice
            ["RoadIceCheckpoint"] = ("RoadIceStraight", DirectionOffset.None),
            ["RoadIceCheckpointSlopeUp"] = ("RoadIceSlopeStraight", DirectionOffset.None),
            ["RoadIceCheckpointSlopeDown"] = ("RoadIceSlopeStraight", DirectionOffset.Half),
            ["RoadIceCheckpointTiltLeft"] = ("RoadIceTiltStraight", DirectionOffset.Half),
            ["RoadIceCheckpointTiltRight"] = ("RoadIceTiltStraight", DirectionOffset.None),

            // Road Water
            ["RoadWaterCheckpoint"] = ("RoadWaterStraight", DirectionOffset.None),

            // Platform Tech
            ["PlatformTechCheckpoint"] = ("PlatformTechBase", DirectionOffset.None),
            ["PlatformTechCheckpointSlope2Down"] = ("PlatformTechSlope2Straight", DirectionOffset.Half),
            ["PlatformTechCheckpointSlope2Right"] = ("PlatformTechSlope2Straight", DirectionOffset.ThreeQuarters),

            // Platform Dirt
            ["PlatformDirtCheckpoint"] = ("PlatformDirtBase", DirectionOffset.None),
            ["PlatformDirtCheckpointSlope2Down"] = ("PlatformDirtSlope2Straight", DirectionOffset.Half),
            ["PlatformDirtCheckpointSlope2Right"] = ("PlatformDirtSlope2Straight", DirectionOffset.ThreeQuarters),

            // Platform Ice
            ["PlatformIceCheckpoint"] = ("PlatformIceBase", DirectionOffset.None),
            ["PlatformIceCheckpointSlope2Down"] = ("PlatformIceSlope2Straight", DirectionOffset.Half),
            ["PlatformIceCheckpointSlope2Right"] = ("PlatformIceSlope2Straight", DirectionOffset.ThreeQuarters),

            // Platform Grass
            ["PlatformGrassCheckpoint"] = ("PlatformGrassBase", DirectionOffset.None),
            ["PlatformGrassCheckpointSlope2Down"] = ("PlatformGrassSlope2Straight", DirectionOffset.Half),
            ["PlatformGrassCheckpointSlope2Right"] = ("PlatformGrassSlope2Straight", DirectionOffset.ThreeQuarters),

            // Platform Plastic
            ["PlatformPlasticCheckpoint"] = ("PlatformPlasticBase", DirectionOffset.None),
            ["PlatformPlasticCheckpointSlope2Down"] = ("PlatformPlasticSlope2Straight", DirectionOffset.Half),
            ["PlatformPlasticCheckpointSlope2Right"] = ("PlatformPlasticSlope2Straight", DirectionOffset.ThreeQuarters),

            // Open Tech Road
            ["OpenTechRoadCheckpoint"] = ("OpenTechRoadStraight", DirectionOffset.None),

            // Open Dirt Road
            ["OpenDirtRoadCheckpoint"] = ("OpenDirtRoadStraight", DirectionOffset.None),

            // Open Ice Road
            ["OpenIceRoadCheckpoint"] = ("OpenIceRoadStraight", DirectionOffset.None),

            // Open Grass Road
            ["OpenGrassRoadCheckpoint"] = ("OpenGrassRoadStraight", DirectionOffset.None),
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