using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GBX.NET;
using GBX.NET.Engines.Game;
using GBX.NET.Engines.GameData;
using GBX.NET.Engines.Plug;

namespace tman.src.Positioning
{
    public static class FloorFinishItem
    {
        private static readonly Dictionary<string, (string Tile, float Bed)> OpenWater = new()
        {
            ["RedIsland"] = ("Water", 2.0f),
            ["WhiteShore"] = ("Water", 2.0f),
            ["BlueBay"] = ("Sea", 4.0f),
            ["GreenCoast"] = ("Lake", 2.0f),
        };

        private const float Below = 0.1f;
        private const float Above = 0.4f;
        private const float Cell = GridConstants.BlockSize;

        public static byte[]? Build(CGameCtnChallenge map, string templatePath, string ident)
        {
            if (!OpenWater.TryGetValue(map.GetEnvironment(), out var water))
            {
                return null;
            }

            var groundRow = MapUtility.GroundRow(map);
            var rows = new Dictionary<(int Y, int Z), SortedSet<int>>();
            var blocks = (map.Blocks ?? []).Concat(map.BakedBlocks ?? []);
            foreach (var block in blocks)
            {
                if (block.IsFree || block.BlockModel.Id != water.Tile)
                {
                    continue;
                }
                var key = (block.Coord.Y, block.Coord.Z);
                if (!rows.TryGetValue(key, out var xs))
                {
                    rows[key] = xs = [];
                }
                xs.Add(block.Coord.X);
            }
            if (rows.Count == 0)
            {
                return null;
            }

            var vertices = new List<Vec3>();
            var triangles = new List<CPlugSurface.Mesh.Triangle>();
            int[][] quads = [[0, 3, 2, 1], [4, 5, 6, 7], [0, 1, 5, 4], [2, 3, 7, 6], [1, 2, 6, 5], [0, 4, 7, 3]];
            foreach (var ((cy, cz), xs) in rows)
            {
                var bed = (cy - groundRow) * GridConstants.VerticalBlockSize + water.Bed;
                var (y0, y1) = (bed - Below, bed + Above);
                var (z0, z1) = (cz * Cell, (cz + 1) * Cell);
                foreach (var (first, last) in Runs(xs))
                {
                    var (x0, x1) = (first * Cell, (last + 1) * Cell);
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
                throw new InvalidOperationException($"{templatePath} has no trigger surface");
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

        private static IEnumerable<(int First, int Last)> Runs(SortedSet<int> xs)
        {
            int? first = null;
            var last = 0;
            foreach (var x in xs)
            {
                if (first is null || x != last + 1)
                {
                    if (first is not null)
                    {
                        yield return (first.Value, last);
                    }
                    first = x;
                }
                last = x;
            }
            if (first is not null)
            {
                yield return (first.Value, last);
            }
        }
    }
}
