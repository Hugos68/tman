using System.Collections.Generic;
using System.Linq;
using GBX.NET.Engines.Game;
using tman.src.Positioning;

namespace tman.src.Alterations
{
    public class CheckpointsCleaned : IAlteration
    {
        private static readonly Dictionary<string, (string Name, DirectionOffset Rotation)> BlockReplacements = new()
        {
            // Road Tech
            ["RoadTechCheckpointSlopeUp"] = ("RoadTechSlopeStraight", DirectionOffset.None),
            ["RoadTechCheckpointSlopeDown"] = ("RoadTechSlopeStraight", DirectionOffset.Half),
            ["RoadTechCheckpointTiltLeft"] = ("RoadTechTiltStraight", DirectionOffset.Half),
            ["RoadTechCheckpointTiltRight"] = ("RoadTechTiltStraight", DirectionOffset.None),

            // Road Dirt
            ["RoadDirtCheckpointSlopeUp"] = ("RoadDirtSlopeStraight", DirectionOffset.None),
            ["RoadDirtCheckpointSlopeDown"] = ("RoadDirtSlopeStraight", DirectionOffset.Half),
            ["RoadDirtCheckpointTiltLeft"] = ("RoadDirtTiltStraight", DirectionOffset.Half),
            ["RoadDirtCheckpointTiltRight"] = ("RoadDirtTiltStraight", DirectionOffset.None),

            // Road Bump
            ["RoadBumpCheckpointSlopeUp"] = ("RoadBumpSlopeStraight", DirectionOffset.None),
            ["RoadBumpCheckpointSlopeDown"] = ("RoadBumpSlopeStraight", DirectionOffset.Half),
            ["RoadBumpCheckpointTiltLeft"] = ("RoadBumpTiltStraight", DirectionOffset.Half),
            ["RoadBumpCheckpointTiltRight"] = ("RoadBumpTiltStraight", DirectionOffset.None),

            // Road Ice
            ["RoadIceCheckpointSlopeUp"] = ("RoadIceSlopeStraight", DirectionOffset.None),
            ["RoadIceCheckpointSlopeDown"] = ("RoadIceSlopeStraight", DirectionOffset.Half),
            ["RoadIceCheckpointTiltLeft"] = ("RoadIceTiltStraight", DirectionOffset.Half),
            ["RoadIceCheckpointTiltRight"] = ("RoadIceTiltStraight", DirectionOffset.None),

            // Platform Tech
            ["PlatformTechCheckpoint"] = ("PlatformTechBase", DirectionOffset.None),

            // Platform Dirt
            ["PlatformDirtCheckpoint"] = ("PlatformDirtBase", DirectionOffset.None),

            // Platform Ice
            ["PlatformIceCheckpoint"] = ("PlatformIceBase", DirectionOffset.None),

            // Platform Grass
            ["PlatformGrassCheckpoint"] = ("PlatformGrassBase", DirectionOffset.None),

            // Platform Plastic
            ["PlatformPlasticCheckpoint"] = ("PlatformPlasticBase", DirectionOffset.None),

            // Open Tech Road
            ["OpenTechRoadCheckpoint"] = ("OpenTechRoadStraight", DirectionOffset.None),

            // Open Dirt Road
            ["OpenDirtRoadCheckpoint"] = ("OpenDirtRoadStraight", DirectionOffset.None),

            // Open Ice Road
            ["OpenIceRoadCheckpoint"] = ("OpenIceRoadStraight", DirectionOffset.None),

            // Open Grass Road
            ["OpenGrassRoadCheckpoint"] = ("OpenGrassRoadStraight", DirectionOffset.None),
        };

        public void Apply(CGameCtnChallenge map)
        {
            RemoveCheckpointItems(map);
            CleanCheckpointBlocks(map);
        }

        private static void RemoveCheckpointItems(CGameCtnChallenge map)
        {
            if (map.AnchoredObjects is null) return;
            map.AnchoredObjects.RemoveAll(
                obj => obj.ItemModel.Id.Contains("Checkpoint"));
        }

        private static void CleanCheckpointBlocks(CGameCtnChallenge map)
        {
            if (map.Blocks is null) return;
            map.Blocks.RemoveAll(block => block.BlockModel.Id == "GateCheckpoint");
            var checkpointBlocks = map.Blocks
                .Where(block => block.BlockModel.Id.Contains("Checkpoint"))
                .ToList();
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