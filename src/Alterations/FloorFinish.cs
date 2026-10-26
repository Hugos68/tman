using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using GBX.NET;
using GBX.NET.Engines.Game;
using tman.src.Alterations.Internal;
using tman.src.Application;
using tman.src.Core;
using tman.src.Inventory;
using tman.src.Positioning;

namespace tman.src.Alterations
{
    public class FloorFinish : Alteration
    {
        protected override IEnumerable<Alteration> PreappliedAlterations =>
        [
            new ValidationRemoved(),
            new LightmapRemoved(),
            new InventoryRemoved(InventoryUtility.IsFinish, InventoryUtility.IsFinish),
            new CheckpointsCleaned(),
            new ConditionalAlteration(map => map.GetEnvironment() == "Stadium2020",
                new ItemFinishes(new Dictionary<string, string>
                    {
                        ["DecoPlatformCurve3In"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "floor-finish", "FloorFinishDecoPlatformCurve3In.Item.Gbx"),
                        ["DecoPlatformSlopeBaseCurve1InFullV0"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "floor-finish", "FloorFinishDecoPlatformSlopeBaseCurve1InFullV0.Item.Gbx"),
                        ["DecoPlatformSlopeBaseCurve1OutV128"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "floor-finish", "FloorFinishDecoPlatformSlopeBaseCurve1OutV128.Item.Gbx"),
                        ["DecoWallCurve1Pillar"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "floor-finish", "FloorFinishDecoWallCurve1Pillar.Item.Gbx"),
                        ["DecoWallCurve2InPillar"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "floor-finish", "FloorFinishDecoWallCurve2InPillar.Item.Gbx"),
                        ["DecoWallCurve2Pillar"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "floor-finish", "FloorFinishDecoWallCurve2Pillar.Item.Gbx"),
                        ["DecoWallCurve3InPillar"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "floor-finish", "FloorFinishDecoWallCurve3InPillar.Item.Gbx"),
                        ["DecoWallCurve3OutPillar"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "floor-finish", "FloorFinishDecoWallCurve3OutPillar.Item.Gbx"),
                        ["DecoWallDiag1"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "floor-finish", "FloorFinishDecoWallDiag1.Item.Gbx"),
                        ["DecoWallDiag1Pillar"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "floor-finish", "FloorFinishDecoWallDiag1Pillar.Item.Gbx"),
                        ["SnowRoadPillarStraight"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "floor-finish", "FloorFinishSnowRoadPillarStraight.Item.Gbx"),
                        ["StructureBase"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "floor-finish", "FloorFinishStructureBase.Item.Gbx"),
                        ["StructurePillar"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "floor-finish", "FloorFinishStructurePillar.Item.Gbx"),
                        ["TrackWallBranchYShaped2X3PillarV64"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "floor-finish", "FloorFinishTrackWallBranchYShaped2X3PillarV64.Item.Gbx"),
                        ["TrackWallChicaneX2LeftPillarV64"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "floor-finish", "FloorFinishTrackWallChicaneX2LeftPillarV64.Item.Gbx"),
                        ["TrackWallChicaneX3LeftPillarV64"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "floor-finish", "FloorFinishTrackWallChicaneX3LeftPillarV64.Item.Gbx"),
                        ["TrackWallChicaneX3RightPillarV64"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "floor-finish", "FloorFinishTrackWallChicaneX3RightPillarV64.Item.Gbx"),
                        ["TrackWallCurve1PillarV64"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "floor-finish", "FloorFinishTrackWallCurve1PillarV64.Item.Gbx"),
                        ["TrackWallCurve2PillarV64"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "floor-finish", "FloorFinishTrackWallCurve2PillarV64.Item.Gbx"),
                        ["TrackWallCurve3PillarV64"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "floor-finish", "FloorFinishTrackWallCurve3PillarV64.Item.Gbx"),
                        ["TrackWallDeadendRoundPillarV64"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "floor-finish", "FloorFinishTrackWallDeadendRoundPillarV64.Item.Gbx"),
                        ["TrackWallStraightPillar"] = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items", "floor-finish", "FloorFinishTrackWallStraightPillar.Item.Gbx"),
                    },
                    blockPredicate: block => !block.IsFree && block.IsGround
                )),
            new MapNameEdited(name => name + " Floor Finish"),
        ];

        private static readonly Dictionary<string, (string[] Tiles, float Bottom, float Top)> OpenWater = new()
        {
            ["RedIsland"] = (["Water"], -13.6f, -3.7f),
            ["WhiteShore"] = (["Water"], -13.6f, -3.7f),
            ["BlueBay"] = (["Sea"], -10.1f, 5.1f),
            ["GreenCoast"] = (["Lake"], -13.6f, -3.7f),
        };

        private const float Thickness = 0.3f;

        protected override void Run(CGameCtnChallenge map)
        {
            var inventory = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "items");
            var data = Path.Combine(ApplicationConstants.ProjectDirectory, "assets", "data");
            var environment = map.GetEnvironment();
            var boxes = environment == "Stadium2020"
                ? GrassBoxes(map, Path.Combine(data, "StadiumBlockFootprints.tsv"))
                : WaterBoxes(map, environment, Path.Combine(data, "WaterFootprints.tsv"));
            OuterRing(map, environment, Path.Combine(data, "OuterTerrain.tsv"), boxes);
            if (boxes.Count == 0)
            {
                return;
            }
            FinishPlane.Place(map, Path.Combine(inventory, "FinishPlaneTemplate.Item.Gbx"), $"FinishFloor_{map.MapUid}.Item.Gbx", boxes, environment == "Stadium2020");
        }

        private static List<FinishPlane.Box> WaterBoxes(CGameCtnChallenge map, string environment, string footprintsPath)
        {
            var boxes = new List<FinishPlane.Box>();
            if (!OpenWater.TryGetValue(environment, out var water))
            {
                return boxes;
            }
            var footprints = LoadWaterFootprints(footprintsPath, environment);
            var cells = new Dictionary<int, List<int>>();
            var squares = new Dictionary<int, List<int>>();
            var seen = new HashSet<Int3>();
            foreach (var block in (map.Blocks ?? []).Concat(map.BakedBlocks ?? []).Where(b => !b.IsFree))
            {
                if (water.Tiles.Contains(block.BlockModel.Id))
                {
                    if (seen.Add(block.Coord))
                    {
                        Add(cells, block.Coord.Z, block.Coord.X);
                    }
                }
                else if (footprints.TryGetValue((block.BlockModel.Id, (uint)block.Flags & 0x0FE01FFF), out var deep))
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
                            Add(squares, block.Coord.Z * 8 + z, block.Coord.X * 8 + x);
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
            return boxes;
        }

        private static List<FinishPlane.Box> GrassBoxes(CGameCtnChallenge map, string footprintsPath)
        {
            var boxes = new List<FinishPlane.Box>();
            var grass = (map.Blocks ?? []).Concat(map.BakedBlocks ?? []).Where(b => !b.IsFree && b.BlockModel.Id == "Grass").Select(b => b.Coord).Distinct().ToList();
            if (grass.Count == 0)
            {
                return boxes;
            }
            var footprints = LoadBlockFootprints(footprintsPath);
            var hidden = new HashSet<(int X, int Z)>();
            foreach (var block in (map.Blocks ?? []).Where(b => !b.IsFree && b.IsGround && b.BlockModel.Id != "Grass"))
            {
                var (sizeX, sizeZ, columns) = footprints.TryGetValue(block.BlockModel.Id, out var footprint) ? footprint : (1, 1, [(0, 0)]);
                foreach (var (ux, uz) in columns)
                {
                    var (x, z) = block.Direction switch
                    {
                        Direction.East => (sizeZ - 1 - uz, ux),
                        Direction.South => (sizeX - 1 - ux, sizeZ - 1 - uz),
                        Direction.West => (uz, sizeX - 1 - ux),
                        _ => (ux, uz),
                    };
                    hidden.Add((block.Coord.X + x, block.Coord.Z + z));
                }
            }
            var y = (grass.Min(c => c.Y) - MapUtility.GroundRow(map)) * MapConstants.CellHeight;
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

        private static void OuterRing(CGameCtnChallenge map, string environment, string path, List<FinishPlane.Box> boxes)
        {
            var collection = environment == "Stadium2020" ? "Stadium" : environment;
            var decoration = environment == "Stadium2020" ? Regex.Replace(map.Decoration.Id, "(Day|Night|Sunrise|Sunset)$", "") : "Base64x64";
            var band = OpenWater.TryGetValue(environment, out var water) ? (water.Bottom, water.Top) : ((float, float)?)null;
            foreach (var f in File.ReadLines(path).Skip(1).Select(line => line.Split('\t')).Where(f => f[0] == collection && f[1] == decoration))
            {
                var x0 = float.Parse(f[2], CultureInfo.InvariantCulture);
                var x1 = float.Parse(f[3], CultureInfo.InvariantCulture);
                var z0 = float.Parse(f[4], CultureInfo.InvariantCulture);
                var z1 = float.Parse(f[5], CultureInfo.InvariantCulture);
                var floor = float.Parse(f[6], CultureInfo.InvariantCulture);
                var (y0, y1) = band ?? (floor, floor + Thickness);
                boxes.Add(new FinishPlane.Box(x0, x1, z0, z1, y0, y1));
            }
        }

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
            foreach (var f in File.ReadLines(path).Skip(1).Select(line => line.Split('\t')).Where(f => f[0] == environment))
            {
                var flags = uint.Parse(f[2], NumberStyles.HexNumber, CultureInfo.InvariantCulture) & 0x0FE01FFF;
                var mask = ulong.Parse(f[8], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                table[(f[1], flags)] = mask;
            }
            return table;
        }

        private static Dictionary<string, (int SizeX, int SizeZ, (int, int)[] Columns)> LoadBlockFootprints(string path)
        {
            var table = new Dictionary<string, (int, int, (int, int)[])>(StringComparer.OrdinalIgnoreCase);
            foreach (var f in File.ReadLines(path).Skip(1).Select(line => line.Split('\t')))
            {
                var columns = f[6].Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Select(c => c.Split(','))
                    .Select(c => (int.Parse(c[0], CultureInfo.InvariantCulture), int.Parse(c[1], CultureInfo.InvariantCulture)))
                    .ToArray();
                table[f[0]] = (int.Parse(f[1], CultureInfo.InvariantCulture), int.Parse(f[2], CultureInfo.InvariantCulture), columns);
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
