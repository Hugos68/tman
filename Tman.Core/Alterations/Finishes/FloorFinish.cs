using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using GBX.NET;
using Tman.Core.Alterations.Steps;
using Tman.Core.Inventory;
using Tman.Core.Inventory.Builders;
using Tman.Core.Maps;

namespace Tman.Core.Alterations.Finishes
{
    public class FloorFinish : StandardAlteration
    {
        protected override string DisplayName => "Floor Finish";

        protected override IEnumerable<Alteration> Steps =>
        [
            new InventoryRemoved(placement => placement.IsFinish),
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

        protected override void Run(Map map)
        {
            var collection = map.Collection;
            var boxes = collection == Collections.Stadium
                ? GrassBoxes(map)
                : WaterBoxes(map, collection, Assets.Data("WaterFootprints.json"));
            if (boxes.Count == 0)
            {
                return;
            }
            FinishPlane.Place(map, Custom.FinishPlaneTemplate.FullPath(), $"FinishFloor_{map.Uid}.Item.Gbx", boxes, collection == Collections.Stadium);
            if (collection == Collections.Stadium && map.Decoration.StartsWith("48x48"))
            {
                map.Embed(Custom.FinishStadiumPenaltyGrass);
                map.Place(Custom.FinishStadiumPenaltyGrass, new Vec3(0, GrassLevel(map), 0));
            }
        }

        private static List<FinishPlane.Box> WaterBoxes(Map map, Collections collection, string footprintsPath)
        {
            var boxes = new List<FinishPlane.Box>();
            if (!OpenWater.TryGetValue(collection, out var water))
            {
                return boxes;
            }
            var footprints = LoadWaterFootprints(footprintsPath, collection.ToString());
            var cells = new Dictionary<int, List<int>>();
            var squares = new Dictionary<int, List<int>>();
            var seen = new HashSet<Int3>();
            foreach (var block in map.Terrain)
            {
                if (water.Tiles.Contains(block.BlockModel))
                {
                    if (seen.Add(block.Cell))
                    {
                        Add(cells, block.Cell.Z, block.Cell.X);
                    }
                }
                else if (footprints.TryGetValue((block.Id, block.Flags & 0x0FE01FFF), out var deep))
                {
                    for (var zi = 0; zi < 8; zi++)
                    {
                        for (var xi = 0; xi < 8; xi++)
                        {
                            if ((deep & (1UL << (zi * 8 + xi))) == 0)
                            {
                                continue;
                            }
                            var (x, z) = block.Direction switch
                            {
                                Direction.East => (7 - zi, xi),
                                Direction.South => (7 - xi, 7 - zi),
                                Direction.West => (zi, 7 - xi),
                                _ => (xi, zi),
                            };
                            Add(squares, block.Cell.Z * 8 + z, block.Cell.X * 8 + x);
                        }
                    }
                }
            }
            foreach (var (z, xs) in cells)
            {
                foreach (var (x0, x1) in Runs(xs))
                {
                    boxes.Add(new FinishPlane.Box(x0 * MapConstants.CellSize, x1 * MapConstants.CellSize, z * MapConstants.CellSize, (z + 1) * MapConstants.CellSize, water.Bottom, water.Top));
                }
            }
            foreach (var (z, xs) in squares)
            {
                foreach (var (x0, x1) in Runs(xs))
                {
                    boxes.Add(new FinishPlane.Box(x0 * MapConstants.SubcellSize, x1 * MapConstants.SubcellSize, z * MapConstants.SubcellSize, (z + 1) * MapConstants.SubcellSize, water.Bottom, water.Top));
                }
            }
            var width = map.Size.X * MapConstants.CellSize;
            var depth = map.Size.Z * MapConstants.CellSize;
            var margin = Ring * MapConstants.CellSize;
            boxes.Add(new FinishPlane.Box(-margin, width + margin, -margin, 0, water.Bottom, water.Top));
            boxes.Add(new FinishPlane.Box(-margin, width + margin, depth, depth + margin, water.Bottom, water.Top));
            boxes.Add(new FinishPlane.Box(-margin, 0, 0, depth, water.Bottom, water.Top));
            boxes.Add(new FinishPlane.Box(width, width + margin, 0, depth, water.Bottom, water.Top));
            return boxes;
        }

        private static List<FinishPlane.Box> GrassBoxes(Map map)
        {
            var boxes = new List<FinishPlane.Box>();
            var grass = map.Terrain.Of(Blocks.Grass).Select(placement => placement.Cell).Distinct().ToList();
            if (grass.Count == 0)
            {
                return boxes;
            }
            var hidden = new HashSet<(int X, int Z)>();
            foreach (var block in map.Placements.OnlyBlocks().Ground().Where(placement => !placement.IsFree && placement.BlockModel != Blocks.Grass))
            {
                var (sizeX, sizeZ, cells) = block.BlockModel is { } model ? (model.SizeX, model.SizeZ, model.Footprint()) : (1, 1, [(0, 0)]);
                foreach (var (ux, uz) in cells)
                {
                    var (x, z) = block.Direction switch
                    {
                        Direction.East => (sizeZ - 1 - uz, ux),
                        Direction.South => (sizeX - 1 - ux, sizeZ - 1 - uz),
                        Direction.West => (uz, sizeX - 1 - ux),
                        _ => (ux, uz),
                    };
                    hidden.Add((block.Cell.X + x, block.Cell.Z + z));
                }
            }
            var y = GrassLevel(map);
            var rows = new Dictionary<int, List<int>>();
            foreach (var (x, z) in grass.Select(c => (c.X, c.Z)).Distinct().Where(c => !hidden.Contains(c)))
            {
                Add(rows, z, x);
            }
            foreach (var (z, xs) in rows)
            {
                foreach (var (x0, x1) in Runs(xs))
                {
                    boxes.Add(new FinishPlane.Box(x0 * MapConstants.CellSize, x1 * MapConstants.CellSize, z * MapConstants.CellSize, (z + 1) * MapConstants.CellSize, y, y + Thickness));
                }
            }
            return boxes;
        }

        private static float GrassLevel(Map map) =>
            (map.Terrain.Of(Blocks.Grass).Min(placement => placement.Cell.Y) - map.GroundRow) * MapConstants.CellHeight;

        private static IEnumerable<(int X0, int X1)> Runs(List<int> xs)
        {
            var sorted = xs.Distinct().OrderBy(x => x).ToList();
            for (var i = 0; i < sorted.Count;)
            {
                var start = sorted[i];
                var end = start;
                while (i + 1 < sorted.Count && sorted[i + 1] == end + 1)
                {
                    i++;
                    end = sorted[i];
                }
                i++;
                yield return (start, end + 1);
            }
        }

        private static Dictionary<(string Tile, uint Flags), ulong> LoadWaterFootprints(string path, string environment)
        {
            var table = new Dictionary<(string, uint), ulong>();
            var footprints = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, Dictionary<string, string>>>>(File.ReadAllText(path)) ?? new();
            foreach (var (tile, masks) in footprints.TryGetValue(environment, out var tiles) ? tiles : new())
            {
                foreach (var (flags, mask) in masks)
                {
                    table[(tile, uint.Parse(flags, NumberStyles.HexNumber, CultureInfo.InvariantCulture) & 0x0FE01FFF)] = ulong.Parse(mask, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                }
            }
            return table;
        }

        private static void Add<TKey>(Dictionary<TKey, List<int>> rows, TKey key, int x) where TKey : notnull
        {
            if (!rows.TryGetValue(key, out var xs))
            {
                rows[key] = xs = [];
            }
            xs.Add(x);
        }
    }
}
