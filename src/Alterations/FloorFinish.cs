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

        private static readonly string FloorFinishDirectory = Path.Combine(ApplicationConstants.ProjectDirectory, "data", "items", "floor-finish");
        private static readonly string StadiumBlockFootprintsPath = Path.Combine(ApplicationConstants.ProjectDirectory, "data", "StadiumBlockFootprints.tsv");

        protected override void Run(CGameCtnChallenge map)
        {
            var ident = $"FinishFloor_{map.MapUid}.Item.Gbx";

            var bytes = FloorFinishItem.Build(
                map,
                Path.Combine(FloorFinishDirectory, "FinishFloorTemplate.Item.Gbx"),
                StadiumBlockFootprintsPath,
                ident);
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
            map.AnchoredObjects![^1].WaypointSpecialProperty = new() { Tag = "Goal", Order = 0 };

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
                var path = Path.Combine(FloorFinishDirectory, "cutouts", file);
                if (embedded.Add(file))
                {
                    EmbedUtility.Embed(map, path);
                }
                MapUtility.PlaceRelative(map, block, new CGameCtnAnchoredObject { ItemModel = EmbedUtility.ExtractIdent(path) });
                map.AnchoredObjects![^1].WaypointSpecialProperty = new() { Tag = "Goal", Order = 0 };
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

        public static int Variant(CGameCtnBlock block) => (block.Flags & 0x3F) + 64 * ((block.Flags >> 21) & 0x7F);

        public static byte[]? Build(CGameCtnChallenge map, string templatePath, string footprintsPath, string ident)
        {
            var environment = map.GetEnvironment();
            var (rows, below, above) = environment == "Stadium2020"
                ? (KeptGrassCells(map, footprintsPath), 0.0f, 1.0f)
                : (OpenWaterCells(map, environment), 0.1f, 0.4f);
            if (rows.Count == 0)
            {
                return null;
            }

            var vertices = new List<Vec3>();
            var triangles = new List<CPlugSurface.Mesh.Triangle>();
            int[][] quads = [[0, 3, 2, 1], [4, 5, 6, 7], [0, 1, 5, 4], [2, 3, 7, 6], [1, 2, 6, 5], [0, 4, 7, 3]];
            foreach (var ((y, z), xs) in rows.OrderBy(r => r.Key))
            {
                var sorted = xs.Distinct().OrderBy(x => x).ToList();
                var (y0, y1) = (y - below, y + above);
                var (z0, z1) = (z * Cell, (z + 1) * Cell);
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
                    var (x0, x1) = (start * Cell, (end + 1) * Cell);
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
                }
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
            item.Ident = new Ident(ident, item.Ident.Collection, ident);

            using var ms = new MemoryStream();
            gbx.Save(ms);
            return ms.ToArray();
        }

        private static Dictionary<(float Y, int Z), List<int>> OpenWaterCells(CGameCtnChallenge map, string environment)
        {
            var rows = new Dictionary<(float, int), List<int>>();
            if (!OpenWater.TryGetValue(environment, out var water))
            {
                return rows;
            }
            var groundRow = MapUtility.GroundRow(map);
            var seen = new HashSet<Int3>();
            foreach (var block in (map.Blocks ?? []).Concat(map.BakedBlocks ?? []))
            {
                if (block.IsFree || !water.Tiles.Contains(block.BlockModel.Id) || !seen.Add(block.Coord))
                {
                    continue;
                }
                Add(rows, ((block.Coord.Y - groundRow) * GridConstants.VerticalBlockSize + water.Bed, block.Coord.Z), block.Coord.X);
            }
            return rows;
        }

        private static Dictionary<(float Y, int Z), List<int>> KeptGrassCells(CGameCtnChallenge map, string footprintsPath)
        {
            var rows = new Dictionary<(float, int), List<int>>();
            var grass = (map.Blocks ?? []).Concat(map.BakedBlocks ?? []).Where(b => !b.IsFree && b.BlockModel.Id == "Grass").Select(b => b.Coord).Distinct().ToList();
            if (grass.Count == 0)
            {
                return rows;
            }
            var baseRow = grass.Min(c => c.Y);
            var footprints = LoadFootprints(footprintsPath);
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
            foreach (var cell in grass.Select(c => (c.X, c.Z)).Distinct().Where(c => !hidden.Contains(c)))
            {
                Add(rows, (y, cell.Z), cell.X);
            }
            return rows;
        }

        private static Dictionary<string, (int SizeX, int SizeZ, (int, int)[] Columns)> LoadFootprints(string path)
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

        private static void Add(Dictionary<(float, int), List<int>> rows, (float, int) key, int x)
        {
            if (!rows.TryGetValue(key, out var xs))
            {
                rows[key] = xs = [];
            }
            xs.Add(x);
        }
    }
}
