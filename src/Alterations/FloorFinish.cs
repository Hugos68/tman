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

        protected override void Run(CGameCtnChallenge map)
        {
            var floorDirectory = Path.Combine(ApplicationConstants.ProjectDirectory, "data", "items", "floor-finish");
            var ident = $"FinishFloor_{map.MapUid}.Item.Gbx";

            var bytes = FloorFinishItem.Build(
                map,
                Path.Combine(floorDirectory, "FinishFloorTemplate.Item.Gbx"),
                Path.Combine(floorDirectory, "StadiumBlockFootprints.tsv"),
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

        private const float Below = 0.1f;
        private const float Above = 0.4f;
        private const float Cell = 32.0f;
        private const float Grass = 2.0f;

        public static byte[]? Build(CGameCtnChallenge map, string templatePath, string footprintsPath, string ident)
        {
            var collection = map.GetEnvironment();
            var cells = collection == "Stadium2020"
                ? FreeGrassCells(map, footprintsPath)
                : OpenWaterCells(map, collection);
            if (cells.Count == 0)
            {
                return null;
            }

            var vertices = new List<Vec3>();
            var triangles = new List<CPlugSurface.Mesh.Triangle>();
            int[][] quads = [[0, 3, 2, 1], [4, 5, 6, 7], [0, 1, 5, 4], [2, 3, 7, 6], [1, 2, 6, 5], [0, 4, 7, 3]];
            foreach (var ((y, z), xs) in cells.OrderBy(c => c.Key))
            {
                var sorted = xs.Distinct().OrderBy(x => x).ToList();
                var (y0, y1) = (y - Below, y + Above);
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

        private static Dictionary<(float Y, int Z), List<int>> OpenWaterCells(CGameCtnChallenge map, string collection)
        {
            var rows = new Dictionary<(float, int), List<int>>();
            if (!OpenWater.TryGetValue(collection, out var water))
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
                var y = (block.Coord.Y - groundRow) * GridConstants.VerticalBlockSize + water.Bed;
                Add(rows, (y, block.Coord.Z), block.Coord.X);
            }
            return rows;
        }

        private static Dictionary<(float Y, int Z), List<int>> FreeGrassCells(CGameCtnChallenge map, string footprintsPath)
        {
            var rows = new Dictionary<(float, int), List<int>>();
            var all = (map.Blocks ?? []).Concat(map.BakedBlocks ?? []).Where(b => !b.IsFree).ToList();
            var grass = all.Where(b => b.BlockModel.Id == "Grass").ToList();
            if (grass.Count == 0)
            {
                return rows;
            }
            var baseRow = grass.Min(b => b.Coord.Y);
            var footprints = LoadFootprints(footprintsPath);
            var occupied = new HashSet<(int X, int Z)>();
            foreach (var block in map.Blocks ?? [])
            {
                if (block.IsFree || block.IsGhost || block.BlockModel.Id == "Grass" || !(block.IsGround || block.Coord.Y == baseRow))
                {
                    continue;
                }
                if (!footprints.TryGetValue(block.BlockModel.Id, out var footprint))
                {
                    footprint = (1, 1, [(0, 0)]);
                }
                foreach (var (ux, uz) in footprint.Cells)
                {
                    occupied.Add(Turn(block, ux, uz, footprint.SizeX, footprint.SizeZ));
                }
            }
            var y = (baseRow - MapUtility.GroundRow(map)) * GridConstants.VerticalBlockSize + Grass;
            foreach (var cell in grass.Select(b => (b.Coord.X, b.Coord.Z)).Distinct())
            {
                if (!occupied.Contains(cell))
                {
                    Add(rows, (y, cell.Z), cell.X);
                }
            }
            return rows;
        }

        private static (int X, int Z) Turn(CGameCtnBlock block, int ux, int uz, int sizeX, int sizeZ)
        {
            var (x, z) = block.Direction switch
            {
                Direction.East => (sizeZ - 1 - uz, ux),
                Direction.South => (sizeX - 1 - ux, sizeZ - 1 - uz),
                Direction.West => (uz, sizeX - 1 - ux),
                _ => (ux, uz),
            };
            return (block.Coord.X + x, block.Coord.Z + z);
        }

        private static Dictionary<string, (int SizeX, int SizeZ, (int, int)[] Cells)> LoadFootprints(string path)
        {
            var table = new Dictionary<string, (int, int, (int, int)[])>(StringComparer.OrdinalIgnoreCase);
            foreach (var line in File.ReadLines(path).Skip(1))
            {
                var f = line.Split('\t');
                var cells = f[3].Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Select(c => c.Split(','))
                    .Select(c => (int.Parse(c[0], CultureInfo.InvariantCulture), int.Parse(c[1], CultureInfo.InvariantCulture)))
                    .ToArray();
                table[f[0]] = (int.Parse(f[1], CultureInfo.InvariantCulture), int.Parse(f[2], CultureInfo.InvariantCulture), cells);
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
