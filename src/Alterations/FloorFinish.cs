using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using GBX.NET;
using GBX.NET.Engines.Game;
using GBX.NET.Engines.GameData;
using GBX.NET.Engines.Plug;
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

        protected override void Run(CGameCtnChallenge map)
        {
            var floorDirectory = Path.Combine(ApplicationConstants.ProjectDirectory, "data", "items", "floor-finish");
            var footprintsDirectory = Path.Combine(ApplicationConstants.ProjectDirectory, "data", "footprints");
            var ident = $"FinishFloor_{map.MapUid}.Item.Gbx";

            var bytes = FloorFinishItem.Build(map, Path.Combine(floorDirectory, "FinishFloorTemplate.Item.Gbx"), footprintsDirectory, ident);
            if (bytes is null)
            {
                return;
            }

            var generatedPath = Path.Combine(Path.GetTempPath(), ident);
            File.WriteAllBytes(generatedPath, bytes);
            EmbedUtility.Embed(map, generatedPath);

            map.PlaceAnchoredObject(
                EmbedUtility.ExtractIdent(generatedPath),
                Vec3.Zero,
                Vec3.Zero,
                Vec3.Zero);

            if (map.GetEnvironment() != "Stadium2020")
            {
                return;
            }

            var embedded = new HashSet<string>();
            foreach (var block in (map.Blocks ?? []).Where(b => !b.IsFree && b.IsGround && b.BlockModel.Id != "Grass"))
            {
                if (!Cutouts.TryGetValue((block.BlockModel.Id, FloorFinishItem.Variant(block)), out var file))
                {
                    continue;
                }
                var path = Path.Combine(floorDirectory, "cutouts", file);
                if (embedded.Add(file))
                {
                    EmbedUtility.Embed(map, path);
                }
                MapUtility.PlaceRelative(map, block, new CGameCtnAnchoredObject { ItemModel = EmbedUtility.ExtractIdent(path) });
                }
        }
    }

    public static class FloorFinishItem
    {
        private static readonly Dictionary<string, (string[] Tiles, float Bed)> OpenWater = new()
        {
            ["RedIsland"] = (["Water"], 2.0f),
            ["WhiteShore"] = (["Water"], 2.0f),
            ["BlueBay"] = (["Sea"], 4.0f),
            ["GreenCoast"] = (["Lake"], 2.0f),
        };

        private const float Cell = 32.0f;
        private const float Square = 4.0f;
        private const float BedBelow = 0.1f;
        private const float BedAbove = 0.4f;
        private const float GrassAbove = 1.0f;
        private const float IndicatorLift = 0.02f;

        private readonly record struct Box(float X0, float X1, float Z0, float Z1, float Y0, float Y1, float Anchor);

        public static int Variant(CGameCtnBlock block) => (block.Flags & 0x3F) + 64 * ((block.Flags >> 21) & 0x7F);

        public static byte[]? Build(CGameCtnChallenge map, string templatePath, string footprintsDirectory, string ident)
        {
            var environment = map.GetEnvironment();
            var boxes = environment == "Stadium2020"
                ? GrassBoxes(map, Path.Combine(footprintsDirectory, "StadiumBlockFootprints.tsv"))
                : WaterBoxes(map, environment, Path.Combine(footprintsDirectory, "WaterFootprints.tsv"));
            if (boxes.Count == 0)
            {
                return null;
            }

            var vertices = new List<Vec3>();
            var triangles = new List<CPlugSurface.Mesh.Triangle>();
            var positions = new List<Vec3>();
            var normals = new List<Vec3>();
            var uvs = new List<Vec2>();
            var indices = new List<int>();
            int[][] quads = [[0, 3, 2, 1], [4, 5, 6, 7], [0, 1, 5, 4], [2, 3, 7, 6], [1, 2, 6, 5], [0, 4, 7, 3]];
            foreach (var (x0, x1, z0, z1, y0, y1, anchor) in boxes)
            {
                var b = vertices.Count;
                vertices.AddRange(
                [
                    new(x0, y0, z0), new(x1, y0, z0), new(x1, y1, z0), new(x0, y1, z0),
                    new(x0, y0, z1), new(x1, y0, z1), new(x1, y1, z1), new(x0, y1, z1),
                ]);
                foreach (var q in quads)
                {
                    triangles.Add(new(new Int3(b + q[0], b + q[1], b + q[2]), 0, 0, 0));
                    triangles.Add(new(new Int3(b + q[0], b + q[2], b + q[3]), 0, 0, 0));
                }
                var p = positions.Count;
                var y = anchor + IndicatorLift;
                positions.AddRange([new(x0, y, z0), new(x0, y, z1), new(x1, y, z1), new(x1, y, z0)]);
                normals.AddRange([new(0, 1, 0), new(0, 1, 0), new(0, 1, 0), new(0, 1, 0)]);
                uvs.AddRange([new(x0 / Square, z0 / Square), new(x0 / Square, z1 / Square), new(x1 / Square, z1 / Square), new(x1 / Square, z0 / Square)]);
                indices.AddRange([p, p + 1, p + 2, p, p + 2, p + 3]);
            }

            var gbx = Gbx.Parse<CGameItemModel>(templatePath);
            var item = gbx.Node;
            if (item.EntityModel is not CGameCommonItemEntityModel entity || entity.TriggerShape is not CPlugSurface trigger)
            {
                throw new InvalidOperationException("the template has no trigger surface");
            }
            trigger.Surf = new CPlugSurface.Mesh
            {
                Version = 7,
                Vertices = vertices.ToArray(),
                Triangles = triangles.ToArray(),
                GameplayMainDir = (trigger.Surf as CPlugSurface.Mesh)?.GameplayMainDir,
            };

            if (entity.StaticObject?.Mesh is not CPlugSolid2Model solid || solid.Visuals?.FirstOrDefault() is not CPlugVisualIndexedTriangles visual || visual.VertexStreams.FirstOrDefault() is not CPlugVertexStream stream)
            {
                throw new InvalidOperationException("the template has no indexed visual with a vertex stream");
            }
            stream.Positions = positions.ToArray();
            stream.Normals = normals.ToArray();
            stream.UVs = new SortedDictionary<int, Vec2[]> { [0] = uvs.ToArray() };
            stream.Colors = new SortedDictionary<int, int[]> { [0] = Enumerable.Repeat(unchecked((int)0xFFFFFFFF), positions.Count).ToArray() };
            SetCount(stream, positions.Count);
            SetCount(visual, positions.Count);
            visual.IndexBuffer!.Indices = indices.ToArray();
            var (min, max) = (positions.Aggregate((a, b) => new Vec3(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Min(a.Z, b.Z))), positions.Aggregate((a, b) => new Vec3(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y), Math.Max(a.Z, b.Z))));
            visual.BoundingBox = new BoxAligned(min.X, min.Y, min.Z, max.X, max.Y, max.Z);
            item.Ident = new Ident(ident, item.Ident.Collection, ident);

            using var ms = new MemoryStream();
            gbx.Save(ms);
            return ms.ToArray();
        }

        private static void SetCount(object node, int count)
        {
            var type = node.GetType();
            var property = type.GetProperty("Count", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            if (property is not null && property.CanWrite)
            {
                property.SetValue(node, count);
                return;
            }
            var field = type.GetField("count", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic) ?? throw new InvalidOperationException($"{type.Name} has no settable Count");
            field.SetValue(node, count);
        }

        private static List<Box> WaterBoxes(CGameCtnChallenge map, string environment, string footprintsPath)
        {
            var boxes = new List<Box>();
            if (!OpenWater.TryGetValue(environment, out var water))
            {
                return boxes;
            }
            var groundRow = MapUtility.GroundRow(map);
            var rows = new Dictionary<(float Y, int Z), List<int>>();
            var seen = new HashSet<Int3>();
            var footprints = LoadWaterFootprints(footprintsPath, environment);
            var squares = new Dictionary<(float Bed, float Floor, int Z), List<int>>();
            foreach (var block in (map.Blocks ?? []).Concat(map.BakedBlocks ?? []).Where(b => !b.IsFree))
            {
                var floor = (block.Coord.Y - groundRow) * GridConstants.VerticalBlockSize;
                if (water.Tiles.Contains(block.BlockModel.Id))
                {
                    if (seen.Add(block.Coord))
                    {
                        Add(rows, (floor, block.Coord.Z), block.Coord.X);
                    }
                }
                else if (footprints.TryGetValue((block.BlockModel.Id, (uint)block.Flags & 0x0FE01FFF), out var deep))
                {
                    var k = 0;
                    for (var zi = 0; zi < 8; zi++)
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
                        Add(squares, (floor + bed, floor, block.Coord.Z * 8 + z), block.Coord.X * 8 + x);
                    }
                }
            }
            foreach (var ((floor, z), xs) in rows)
            {
                foreach (var (x0, x1) in Runs(xs))
                {
                    boxes.Add(new Box(x0 * Cell, x1 * Cell, z * Cell, (z + 1) * Cell, floor + water.Bed - BedBelow, floor + water.Bed + BedAbove, floor + water.Bed));
                }
            }
            foreach (var ((bed, _, z), xs) in squares)
            {
                foreach (var (x0, x1) in Runs(xs))
                {
                    boxes.Add(new Box(x0 * Square, x1 * Square, z * Square, (z + 1) * Square, bed - BedBelow, bed + BedAbove, bed));
                }
            }
            return boxes;
        }

        private static List<Box> GrassBoxes(CGameCtnChallenge map, string footprintsPath)
        {
            var boxes = new List<Box>();
            var grass = (map.Blocks ?? []).Concat(map.BakedBlocks ?? []).Where(b => !b.IsFree && b.BlockModel.Id == "Grass").Select(b => b.Coord).Distinct().ToList();
            if (grass.Count == 0)
            {
                return boxes;
            }
            var baseRow = grass.Min(c => c.Y);
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
            var y = (baseRow - MapUtility.GroundRow(map)) * GridConstants.VerticalBlockSize;
            var rows = new Dictionary<(float Y, int Z), List<int>>();
            foreach (var cell in grass.Select(c => (c.X, c.Z)).Distinct().Where(c => !hidden.Contains(c)))
            {
                Add(rows, (y, cell.Z), cell.X);
            }
            foreach (var ((_, z), xs) in rows)
            {
                foreach (var (x0, x1) in Runs(xs))
                {
                    boxes.Add(new Box(x0 * Cell, x1 * Cell, z * Cell, (z + 1) * Cell, y, y + GrassAbove, y));
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
            foreach (var line in File.ReadLines(path).Skip(1))
            {
                var f = line.Split('\t');
                if (f[0] != environment)
                {
                    continue;
                }
                var beds = f[10].Split(',', StringSplitOptions.RemoveEmptyEntries).Select(v => float.Parse(v, CultureInfo.InvariantCulture)).ToArray();
                table[(f[1], uint.Parse(f[2], NumberStyles.HexNumber, CultureInfo.InvariantCulture) & 0x0FE01FFF)] = (ulong.Parse(f[8], NumberStyles.HexNumber, CultureInfo.InvariantCulture), beds);
            }
            return table;
        }

        private static Dictionary<string, (int SizeX, int SizeZ, (int, int)[] Columns)> LoadBlockFootprints(string path)
        {
            var table = new Dictionary<string, (int, int, (int, int)[])>(StringComparer.OrdinalIgnoreCase);
            foreach (var line in File.ReadLines(path).Skip(1))
            {
                var f = line.Split('\t');
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
