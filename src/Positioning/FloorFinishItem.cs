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
        private static readonly Dictionary<string, (string[] Tiles, float Bed)> Water = new()
        {
            ["RedIsland"] = (["Water", "WaterHill"], 2.0f),
            ["WhiteShore"] = (["Water"], 2.0f),
            ["BlueBay"] = (["Sea"], 4.0f),
            ["GreenCoast"] = (["Lake"], 2.0f),
        };

        private const float Below = 0.1f;
        private const float Above = 0.4f;

        public static byte[]? Build(CGameCtnChallenge map, string templatePath, string ident)
        {
            var collection = map.GetEnvironment();
            if (!Water.TryGetValue(collection, out var water))
            {
                return null;
            }
            var groundRow = MapUtility.GroundRow(map);

            // (cy, cz) -> the cx list of open-water cells
            var rows = new Dictionary<(int Y, int Z), List<int>>();
            var cells = 0;
            foreach (var block in map.Blocks ?? [])
            {
                if (block.IsFree || !water.Tiles.Contains(block.BlockModel.Id))
                {
                    continue;
                }
                var key = (block.Coord.Y, block.Coord.Z);
                if (!rows.TryGetValue(key, out var xs))
                {
                    rows[key] = xs = [];
                }
                xs.Add(block.Coord.X);
                cells++;
            }
            if (cells == 0)
            {
                return null;
            }
            var vertices = new List<Vec3>();
            var triangles = new List<CPlugSurface.Mesh.Triangle>();
            int[][] quads = [[0, 3, 2, 1], [4, 5, 6, 7], [0, 1, 5, 4], [2, 3, 7, 6], [1, 2, 6, 5], [0, 4, 7, 3]];
            foreach (var ((cy, cz), xs) in rows.OrderBy(r => r.Key.Y).ThenBy(r => r.Key.Z))
            {
                var sorted = xs.Distinct().OrderBy(x => x).ToList();
                var yBed = (cy - groundRow) * GridConstants.VerticalBlockSize + water.Bed;
                var (y0, y1) = (yBed - Below, yBed + Above);
                var (z0, z1) = (cz * GridConstants.BlockSize, (cz + 1) * GridConstants.BlockSize);
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
                    var (x0, x1) = (start * GridConstants.BlockSize, (end + 1) * GridConstants.BlockSize);
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
                throw new InvalidOperationException("the template has no CGameCommonItemEntityModel.TriggerShape surface");
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
    }
}
