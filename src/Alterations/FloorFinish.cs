using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using GBX.NET;
using GBX.NET.Engines.Game;
using tman.src.Alterations.Internal;
using tman.src.Application;
using tman.src.Inventory;
using tman.src.Positioning;

namespace tman.src.Alterations
{
    public class FloorFinish : Alteration
    {
        protected override IEnumerable<Alteration> PreappliedAlterations =>
        [
            new WithoutValidation(),
            new WithoutInventory(InventoryUtility.IsFinish, InventoryUtility.IsFinish),
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

        private static readonly Dictionary<(string Block, int Variant), string> Cutouts = new()
        {
            [("DecoPlatformCurve3In", 0)] = "FloorFinishDecoPlatformCurve3In.Item.Gbx",
            [("DecoPlatformSlopeBaseCurve1InFull", 0)] = "FloorFinishDecoPlatformSlopeBaseCurve1InFull.Item.Gbx",
            [("DecoPlatformSlopeBaseCurve1Out", 128)] = "FloorFinishDecoPlatformSlopeBaseCurve1OutV128.Item.Gbx",
            [("DecoWallCurve1Pillar", 0)] = "FloorFinishDecoWallCurve1Pillar.Item.Gbx",
            [("DecoWallCurve2InPillar", 0)] = "FloorFinishDecoWallCurve2InPillar.Item.Gbx",
            [("DecoWallCurve2Pillar", 0)] = "FloorFinishDecoWallCurve2Pillar.Item.Gbx",
            [("DecoWallCurve3InPillar", 0)] = "FloorFinishDecoWallCurve3InPillar.Item.Gbx",
            [("DecoWallCurve3OutPillar", 0)] = "FloorFinishDecoWallCurve3OutPillar.Item.Gbx",
            [("DecoWallDiag1", 0)] = "FloorFinishDecoWallDiag1.Item.Gbx",
            [("DecoWallDiag1Pillar", 0)] = "FloorFinishDecoWallDiag1Pillar.Item.Gbx",
            [("SnowRoadPillarStraight", 0)] = "FloorFinishSnowRoadPillarStraight.Item.Gbx",
            [("StructureBase", 0)] = "FloorFinishStructureBase.Item.Gbx",
            [("StructurePillar", 0)] = "FloorFinishStructurePillar.Item.Gbx",
            [("TrackWallBranchYShaped2X3Pillar", 64)] = "FloorFinishTrackWallBranchYShaped2X3PillarV64.Item.Gbx",
            [("TrackWallChicaneX2LeftPillar", 64)] = "FloorFinishTrackWallChicaneX2LeftPillarV64.Item.Gbx",
            [("TrackWallChicaneX3LeftPillar", 64)] = "FloorFinishTrackWallChicaneX3LeftPillarV64.Item.Gbx",
            [("TrackWallChicaneX3RightPillar", 64)] = "FloorFinishTrackWallChicaneX3RightPillarV64.Item.Gbx",
            [("TrackWallCurve1Pillar", 64)] = "FloorFinishTrackWallCurve1PillarV64.Item.Gbx",
            [("TrackWallCurve2Pillar", 64)] = "FloorFinishTrackWallCurve2PillarV64.Item.Gbx",
            [("TrackWallCurve3Pillar", 64)] = "FloorFinishTrackWallCurve3PillarV64.Item.Gbx",
            [("TrackWallDeadendRoundPillar", 64)] = "FloorFinishTrackWallDeadendRoundPillarV64.Item.Gbx",
        };

        private const float BedBelow = 0.1f;
        private const float BedAbove = 0.4f;
        private const float GrassAbove = 1.0f;
        private const float IndicatorLift = 0.02f;

        protected override void Run(CGameCtnChallenge map)
        {
            var inventory = Path.Combine(ApplicationConstants.ProjectDirectory, "data", "inventory");
            var footprints = Path.Combine(ApplicationConstants.ProjectDirectory, "data", "footprints");
            var environment = map.GetEnvironment();
            var boxes = environment == "Stadium2020"
                ? GrassBoxes(map, Path.Combine(footprints, "StadiumBlockFootprints.tsv"))
                : WaterBoxes(map, environment, Path.Combine(footprints, "WaterFootprints.tsv"));
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
            var embedded = new HashSet<string>();
            foreach (var block in (map.Blocks ?? []).Where(b => !b.IsFree && b.IsGround && b.BlockModel.Id != "Grass"))
            {
                if (!Cutouts.TryGetValue((block.BlockModel.Id, Variant(block)), out var file))
                {
                    continue;
                }
                var path = Path.Combine(directory, file);
                if (embedded.Add(file))
                {
                    EmbedUtility.Embed(map, path);
                }
                MapUtility.PlaceRelative(map, block, new CGameCtnAnchoredObject { ItemModel = EmbedUtility.ExtractIdent(path) });
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
            var squares = new Dictionary<(float Bed, int Z), List<int>>();
            var seen = new HashSet<Int3>();
            foreach (var block in (map.Blocks ?? []).Concat(map.BakedBlocks ?? []).Where(b => !b.IsFree))
            {
                var floor = (block.Coord.Y - groundRow) * GridConstants.CellHeight;
                if (water.Tiles.Contains(block.BlockModel.Id))
                {
                    if (seen.Add(block.Coord))
                    {
                        Add(cells, (floor, block.Coord.Z), block.Coord.X);
                    }
                }
                else if (footprints.TryGetValue((block.BlockModel.Id, (uint)block.Flags & 0x0FE01FFF), out var deep))
                {
                    var k = 0;
                    for (var zi = 0; zi < 8; zi++)
                    {
                        for (var xi = 0; xi < 8; xi++)
                        {
                            if ((deep.Mask & (1UL << (zi * 8 + xi))) == 0)
                            {
                                continue;
                            }
                            var bed = deep.Beds[k++];
                            var (x, z) = block.Direction switch
                            {
                                Direction.East => (7 - zi, xi),
                                Direction.South => (7 - xi, 7 - zi),
                                Direction.West => (zi, 7 - xi),
                                _ => (xi, zi),
                            };
                            Add(squares, (floor + bed, block.Coord.Z * 8 + z), block.Coord.X * 8 + x);
                        }
                    }
                }
            }
            foreach (var ((floor, z), xs) in cells)
            {
                var bed = floor + water.Bed;
                foreach (var (x0, x1) in Runs(xs))
                {
                    boxes.Add(new FinishPlane.Box(x0 * GridConstants.CellSize, x1 * GridConstants.CellSize, z * GridConstants.CellSize, (z + 1) * GridConstants.CellSize, bed - BedBelow, bed + BedAbove, bed + IndicatorLift));
                }
            }
            foreach (var ((bed, z), xs) in squares)
            {
                foreach (var (x0, x1) in Runs(xs))
                {
                    boxes.Add(new FinishPlane.Box(x0 * GridConstants.SubcellSize, x1 * GridConstants.SubcellSize, z * GridConstants.SubcellSize, (z + 1) * GridConstants.SubcellSize, bed - BedBelow, bed + BedAbove, bed + IndicatorLift));
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
            var y = (grass.Min(c => c.Y) - MapUtility.GroundRow(map)) * GridConstants.CellHeight;
            var rows = new Dictionary<int, List<int>>();
            foreach (var (x, z) in grass.Select(c => (c.X, c.Z)).Distinct().Where(c => !hidden.Contains(c)))
            {
                Add(rows, z, x);
            }
            foreach (var (z, xs) in rows)
            {
                foreach (var (x0, x1) in Runs(xs))
                {
                    boxes.Add(new FinishPlane.Box(x0 * GridConstants.CellSize, x1 * GridConstants.CellSize, z * GridConstants.CellSize, (z + 1) * GridConstants.CellSize, y, y + GrassAbove, y + IndicatorLift));
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

        private static Dictionary<(string Tile, uint Flags), (ulong Mask, float[] Beds)> LoadWaterFootprints(string path, string environment)
        {
            var table = new Dictionary<(string, uint), (ulong, float[])>();
            foreach (var f in File.ReadLines(path).Skip(1).Select(line => line.Split('\t')).Where(f => f[0] == environment))
            {
                var flags = uint.Parse(f[2], NumberStyles.HexNumber, CultureInfo.InvariantCulture) & 0x0FE01FFF;
                var mask = ulong.Parse(f[8], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                var beds = f[10].Split(',', StringSplitOptions.RemoveEmptyEntries).Select(v => float.Parse(v, CultureInfo.InvariantCulture)).ToArray();
                table[(f[1], flags)] = (mask, beds);
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
