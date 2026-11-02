using System.Collections.Generic;
using System.Linq;
using GBX.NET;
using Microsoft.Extensions.Logging;
using Tman.Core.Alterations.Steps;
using Tman.Core.Alterations.Steps.Waypoints;
using Tman.Core.Catalog;
using Tman.Core.Maps;
using Tman.Core.Maps.Geometry;
using Tman.Core.Maps.Placements;

namespace Tman.Core.Alterations.Finishes
{
    internal sealed class FloorFinish : StandardAlteration
    {
        protected override string DisplayName => "Floor Finish";

        protected override IEnumerable<Alteration> Steps =>
        [
            new PlacementsRemoved(placement => placement.IsFinish),
            new CheckpointsCleaned(),
            new ConditionalAlteration(
                map => map.Collection == Collections.Stadium,
                new RelativelyPlacedItems(new Dictionary<Selector, Item>
                    {
                        [Blocks.DecoPlatformCurve3In] = Custom.FloorFinishDecoPlatformCurve3In,
                        [Blocks.DecoPlatformSlopeBaseCurve1InFull.Variant(0)] = Custom.FloorFinishDecoPlatformSlopeBaseCurve1InFullV0,
                        [Blocks.DecoPlatformSlopeBaseCurve1Out.Variant(128)] = Custom.FloorFinishDecoPlatformSlopeBaseCurve1OutV128,
                        [Blocks.DecoWallCurve1Pillar] = Custom.FloorFinishDecoWallCurve1Pillar,
                        [Blocks.DecoWallCurve2InPillar] = Custom.FloorFinishDecoWallCurve2InPillar,
                        [Blocks.DecoWallCurve2Pillar] = Custom.FloorFinishDecoWallCurve2Pillar,
                        [Blocks.DecoWallCurve3InPillar] = Custom.FloorFinishDecoWallCurve3InPillar,
                        [Blocks.DecoWallCurve3OutPillar] = Custom.FloorFinishDecoWallCurve3OutPillar,
                        [Blocks.DecoWallDiag1] = Custom.FloorFinishDecoWallDiag1,
                        [Blocks.DecoWallDiag1Pillar] = Custom.FloorFinishDecoWallDiag1Pillar,
                        [Blocks.SnowRoadPillarStraight] = Custom.FloorFinishSnowRoadPillarStraight,
                        [Blocks.StructureBase] = Custom.FloorFinishStructureBase,
                        [Blocks.StructurePillar] = Custom.FloorFinishStructurePillar,
                        [Blocks.TrackWallBranchYShaped2X3Pillar.Variant(64)] = Custom.FloorFinishTrackWallBranchYShaped2X3PillarV64,
                        [Blocks.TrackWallChicaneX2LeftPillar.Variant(64)] = Custom.FloorFinishTrackWallChicaneX2LeftPillarV64,
                        [Blocks.TrackWallChicaneX3LeftPillar.Variant(64)] = Custom.FloorFinishTrackWallChicaneX3LeftPillarV64,
                        [Blocks.TrackWallChicaneX3RightPillar.Variant(64)] = Custom.FloorFinishTrackWallChicaneX3RightPillarV64,
                        [Blocks.TrackWallCurve1Pillar.Variant(64)] = Custom.FloorFinishTrackWallCurve1PillarV64,
                        [Blocks.TrackWallCurve2Pillar.Variant(64)] = Custom.FloorFinishTrackWallCurve2PillarV64,
                        [Blocks.TrackWallCurve3Pillar.Variant(64)] = Custom.FloorFinishTrackWallCurve3PillarV64,
                        [Blocks.TrackWallDeadendRoundPillar.Variant(64)] = Custom.FloorFinishTrackWallDeadendRoundPillarV64,
                        [Blocks.TrackWallStraightPillar] = Custom.FloorFinishTrackWallStraightPillar,
                    },
                    placement => !placement.IsFree && placement.IsGround))
        ];

        private static readonly Dictionary<Collections, (Block[] Tiles, float Bottom, float Top)> OpenWater = new()
        {
            [Collections.RedIsland] = ([Blocks.Water], -13.6f, -3.7f),
            [Collections.WhiteShore] = ([Blocks.Water], -13.6f, -3.7f),
            [Collections.BlueBay] = ([Blocks.Sea], -10.1f, 5.1f),
            [Collections.GreenCoast] = ([Blocks.Lake], -13.6f, -3.7f),
        };

        private const float Thickness = 0.3f;

        private const int Ring = 16;

        protected override void Run(Map map, AlterationContext context)
        {
            var collection = map.Collection;
            var boxes = collection == Collections.Stadium
                ? GrassBoxes(map)
                : WaterBoxes(map, collection);
            if (boxes.Count == 0)
            {
                context.Logger.LogWarning("No floor found: nothing to finish on");
                return;
            }
            FinishPlane.Place(map, context, $"FinishFloor_{map.Uid}.Item.Gbx", boxes, collection == Collections.Stadium);
            context.Logger.LogInformation("Finish plane over {Count} floor boxes", boxes.Count);
            if (collection == Collections.Stadium && map.Decoration.StartsWith("48x48"))
            {
                context.Embed(map, Custom.FinishStadiumPenaltyGrass);
                map.Place(Custom.FinishStadiumPenaltyGrass, new Vec3(0, GrassLevel(map), 0));
            }
        }

        private static List<Box> WaterBoxes(Map map, Collections collection)
        {
            if (!OpenWater.TryGetValue(collection, out var water))
            {
                return [];
            }
            var terrain = map.Terrain.ToLookup(block => water.Tiles.Contains(block.BlockModel));
            var cells = terrain[true].Select(block => (block.Cell.X, block.Cell.Z));
            var subcells = terrain[false].SelectMany(block => WaterFootprints.Subcells(collection, block));
            var boxes = Boxes.OfCells(cells, MapConstants.CellSize, water.Bottom, water.Top);
            boxes.AddRange(Boxes.OfCells(subcells, MapConstants.SubcellSize, water.Bottom, water.Top));
            var width = map.Size.X * MapConstants.CellSize;
            var depth = map.Size.Z * MapConstants.CellSize;
            var margin = Ring * MapConstants.CellSize;
            boxes.Add(new Box(-margin, width + margin, -margin, 0, water.Bottom, water.Top));
            boxes.Add(new Box(-margin, width + margin, depth, depth + margin, water.Bottom, water.Top));
            boxes.Add(new Box(-margin, 0, 0, depth, water.Bottom, water.Top));
            boxes.Add(new Box(width, width + margin, 0, depth, water.Bottom, water.Top));
            return boxes;
        }

        private static List<Box> GrassBoxes(Map map)
        {
            var grass = map.Terrain.Of(Blocks.Grass).Select(placement => (placement.Cell.X, placement.Cell.Z)).Distinct().ToList();
            if (grass.Count == 0)
            {
                return [];
            }
            var hidden = map.Placements.OnlyBlocks().Ground()
                .Where(placement => !placement.IsFree && placement.BlockModel != Blocks.Grass)
                .SelectMany(block => block.Cells())
                .ToHashSet();
            var level = GrassLevel(map);
            return Boxes.OfCells(grass.Where(cell => !hidden.Contains(cell)), MapConstants.CellSize, level, level + Thickness);
        }

        private static float GrassLevel(Map map) =>
            (map.Terrain.Of(Blocks.Grass).Min(placement => placement.Cell.Y) - map.GroundRow) * MapConstants.CellHeight;
    }
}
