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
            new InventoryRemoved(InventoryUtility.IsFinish, InventoryUtility.IsFinish),
            new CheckpointsCleaned(),
            new MapNameEdited(name => name + " Floor Finish")
        ];

        private static readonly Dictionary<string, (string[] Tiles, float Bed)> OpenWater = new()
        {
            ["RedIsland"] = (["Water"], 2.0f),
            ["WhiteShore"] = (["Water"], 2.0f),
            ["BlueBay"] = (["Sea"], 4.0f),
            ["GreenCoast"] = (["Lake"], 2.0f),
        };

        private static readonly Dictionary<string, string> Cutouts = new()
        {
            ["DecoPlatformCurve3In"] = "FloorFinishDecoPlatformCurve3In.Item.Gbx",
            ["DecoPlatformSlopeBaseCurve1InFull"] = "FloorFinishDecoPlatformSlopeBaseCurve1InFull.Item.Gbx",
            ["DecoPlatformSlopeBaseCurve1OutV128"] = "FloorFinishDecoPlatformSlopeBaseCurve1OutV128.Item.Gbx",
            ["DecoWallCurve1Pillar"] = "FloorFinishDecoWallCurve1Pillar.Item.Gbx",
            ["DecoWallCurve2InPillar"] = "FloorFinishDecoWallCurve2InPillar.Item.Gbx",
            ["DecoWallCurve2Pillar"] = "FloorFinishDecoWallCurve2Pillar.Item.Gbx",
            ["DecoWallCurve3InPillar"] = "FloorFinishDecoWallCurve3InPillar.Item.Gbx",
            ["DecoWallCurve3OutPillar"] = "FloorFinishDecoWallCurve3OutPillar.Item.Gbx",
            ["DecoWallDiag1"] = "FloorFinishDecoWallDiag1.Item.Gbx",
            ["DecoWallDiag1Pillar"] = "FloorFinishDecoWallDiag1Pillar.Item.Gbx",
            ["SnowRoadPillarStraight"] = "FloorFinishSnowRoadPillarStraight.Item.Gbx",
            ["StructureBase"] = "FloorFinishStructureBase.Item.Gbx",
            ["StructurePillar"] = "FloorFinishStructurePillar.Item.Gbx",
            ["TrackWallBranchYShaped2X3PillarV64"] = "FloorFinishTrackWallBranchYShaped2X3PillarV64.Item.Gbx",
            ["TrackWallChicaneX2LeftPillarV64"] = "FloorFinishTrackWallChicaneX2LeftPillarV64.Item.Gbx",
            ["TrackWallChicaneX3LeftPillarV64"] = "FloorFinishTrackWallChicaneX3LeftPillarV64.Item.Gbx",
            ["TrackWallChicaneX3RightPillarV64"] = "FloorFinishTrackWallChicaneX3RightPillarV64.Item.Gbx",
            ["TrackWallCurve1PillarV64"] = "FloorFinishTrackWallCurve1PillarV64.Item.Gbx",
            ["TrackWallCurve2PillarV64"] = "FloorFinishTrackWallCurve2PillarV64.Item.Gbx",
            ["TrackWallCurve3PillarV64"] = "FloorFinishTrackWallCurve3PillarV64.Item.Gbx",
            ["TrackWallDeadendRoundPillarV64"] = "FloorFinishTrackWallDeadendRoundPillarV64.Item.Gbx",
        };

        private static readonly Regex VariantRegex = new(@"V(\d+)$", RegexOptions.Compiled);

        private const float BedBelow = 0.1f;
        private const float BedAbove = 1.15f;
        private const float GrassAbove = 1.0f;
        private const float IndicatorLift = 0.02f;

        protected override void Run(CGameCtnChallenge map)
        {
            var inventory = Path.Combine(ApplicationConstants.ProjectDirectory, "data", "inventory");
            var information = Path.Combine(ApplicationConstants.ProjectDirectory, "data", "information");
            var environment = map.GetEnvironment();
            var boxes = environment == "Stadium2020"
                ? GrassBoxes(map, Path.Combine(information, "StadiumBlockFootprints.tsv"))
                : WaterBoxes(map, environment, Path.Combine(information, "WaterFootprints.tsv"));
            if (boxes.Count == 0)
            {
                return;
            }
            FinishPlane.Place(map, Path.Combine(inventory, "FinishPlaneTemplate.Item.Gbx"), $"FinishFloor_{map.MapUid}.Item.Gbx", boxes);
            if (environment == "Stadium2020")
            {
                PlaceCutouts(map, Path.Combine(inventory, "floor-finish"));
            }
        }

        private static void PlaceCutouts(CGameCtnChallenge map, string directory)
        {
            foreach (var (blockId, cutoutFile) in Cutouts)
            {
                var match = VariantRegex.Match(blockId);
                var model = match.Success
                    ? blockId[..match.Index]
                    : blockId;
                int? variant = match.Success
                    ? int.Parse(match.Groups[1].Value)
                    : null;

                var sources = map.Blocks?
                    .Where(block =>
                        !block.IsFree &&
                        block.IsGround &&
                        block.BlockModel.Id == model &&
                        (variant is null || Variant(block) == variant))
                    .ToList() ?? [];

                if (sources.Count == 0)
                {
                    continue;
                }

                var cutoutPath = Path.Combine(directory, cutoutFile);
                EmbedUtility.Embed(map, cutoutPath);

                var cutout = new CGameCtnAnchoredObject
                {
                    ItemModel = EmbedUtility.ExtractIdent(cutoutPath)
                };

                foreach (var source in sources)
                {
                    MapUtility.PlaceRelative(map, source, cutout);
                }
            }
        }

        private static int Variant(CGameCtnBlock block) => (block.Flags & 0x3F) + 64 * ((block.Flags >> 21) & 0x7F);

        private static List<FinishPlane.Box> WaterBoxes(CGameCtnChallenge map, string environment, string footprintsPath)
        {
            var boxes = new List<FinishPlane.Box>();
            if (!OpenWater.TryGetValue(environment, out var water))
            {
                return boxes;
            }
            var groundRow = MapUtility.GroundRow(map);
            var footprints = LoadWaterFootprints(footprintsPath, environment);
            var cells = new Dictionary<(float Floor, int Z), List<int>>();
            var squares = new Dictionary<(float Floor, int Z), List<int>>();
            var seen = new HashSet<Int3>();
            foreach (var block in (map.Blocks ?? []).Concat(map.BakedBlocks ?? []).Where(b => !b.IsFree))
            {
                var floor = (block.Coord.Y - groundRow) * MapConstants.CellHeight;
                if (water.Tiles.Contains(block.BlockModel.Id))
                {
                    if (seen.Add(block.Coord))
                    {
                        Add(cells, (floor, block.Coord.Z), block.Coord.X);
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
                            Add(squares, (floor, block.Coord.Z * 8 + z), block.Coord.X * 8 + x);
                        }
                    }
                }
            }
            foreach (var ((floor, z), xs) in cells)
            {
                var bed = floor + water.Bed;
                foreach (var (x0, x1) in Runs(xs))
                {
                    boxes.Add(new FinishPlane.Box(x0 * MapConstants.CellSize, x1 * MapConstants.CellSize, z * MapConstants.CellSize, (z + 1) * MapConstants.CellSize, bed - BedBelow, bed + BedAbove, bed + IndicatorLift));
                }
            }
            foreach (var ((floor, z), xs) in squares)
            {
                var bed = floor + water.Bed;
                foreach (var (x0, x1) in Runs(xs))
                {
                    boxes.Add(new FinishPlane.Box(x0 * MapConstants.SubcellSize, x1 * MapConstants.SubcellSize, z * MapConstants.SubcellSize, (z + 1) * MapConstants.SubcellSize, bed - BedBelow, bed + BedAbove, bed + IndicatorLift));
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
                    boxes.Add(new FinishPlane.Box(x0 * MapConstants.CellSize, x1 * MapConstants.CellSize, z * MapConstants.CellSize, (z + 1) * MapConstants.CellSize, y, y + GrassAbove, y + IndicatorLift));
                }
            }
            return boxes;
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
