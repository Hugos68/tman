using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GBX.NET;
using GBX.NET.Engines.Game;
using GBX.NET.Engines.GameData;
using GBX.NET.Engines.Plug;

namespace tman.src.Inventory
{
    public static class FinishPlane
    {
        public readonly record struct Box(float X0, float X1, float Z0, float Z1, float Y0, float Y1);

        public enum Side { North, South, East, West }

        public readonly record struct Ramp(float X0, float X1, float Z0, float Z1, float YInner, float YOuter, float Below, float Above, Side Side);

        public static void Place(CGameCtnChallenge map, string templatePath, string ident, IReadOnlyList<Box> boxes) => Place(map, templatePath, ident, boxes, []);

        public static void Place(CGameCtnChallenge map, string templatePath, string ident, IReadOnlyList<Box> boxes, IReadOnlyList<Ramp> ramps)
        {
            var path = Path.Combine(Path.GetTempPath(), ident);
            File.WriteAllBytes(path, Build(templatePath, ident, boxes, ramps));
            EmbedUtility.Embed(map, path);
            map.PlaceAnchoredObject(EmbedUtility.ExtractIdent(path), Vec3.Zero, Vec3.Zero, Vec3.Zero);
        }

        public static byte[] Build(string templatePath, string ident, IReadOnlyList<Box> boxes) => Build(templatePath, ident, boxes, []);

        public static byte[] Build(string templatePath, string ident, IReadOnlyList<Box> boxes, IReadOnlyList<Ramp> ramps)
        {
            var vertices = new List<Vec3>();
            var triangles = new List<CPlugSurface.Mesh.Triangle>();
            int[][] quads = [[0, 3, 2, 1], [4, 5, 6, 7], [0, 1, 5, 4], [2, 3, 7, 6], [1, 2, 6, 5], [0, 4, 7, 3]];
            foreach (var (x0, x1, z0, z1, y0, y1) in Merge(boxes))
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
            }
            foreach (var r in ramps)
            {
                float T(float x, float z) => r.Side switch
                {
                    Side.North => (r.Z1 - z) / (r.Z1 - r.Z0),
                    Side.South => (z - r.Z0) / (r.Z1 - r.Z0),
                    Side.West => (r.X1 - x) / (r.X1 - r.X0),
                    _ => (x - r.X0) / (r.X1 - r.X0),
                };
                float Y(float x, float z, float offset) => r.YInner + (r.YOuter - r.YInner) * T(x, z) + offset;
                (float X, float Z)[] corners = [(r.X0, r.Z0), (r.X1, r.Z0), (r.X1, r.Z0), (r.X0, r.Z0), (r.X0, r.Z1), (r.X1, r.Z1), (r.X1, r.Z1), (r.X0, r.Z1)];
                var b = vertices.Count;
                for (var k = 0; k < corners.Length; k++)
                {
                    vertices.Add(new(corners[k].X, Y(corners[k].X, corners[k].Z, k is 2 or 3 or 6 or 7 ? r.Above : -r.Below), corners[k].Z));
                }
                foreach (var q in quads)
                {
                    triangles.Add(new(new Int3(b + q[0], b + q[1], b + q[2]), 0, 0, 0));
                    triangles.Add(new(new Int3(b + q[0], b + q[2], b + q[3]), 0, 0, 0));
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

        private static List<Box> Merge(IReadOnlyList<Box> boxes)
        {
            var list = boxes.ToList();
            while (true)
            {
                var count = list.Count;
                list.Sort((a, b) => (a.Y0, a.Y1, a.X0, a.X1, a.Z0).CompareTo((b.Y0, b.Y1, b.X0, b.X1, b.Z0)));
                list = MergeRuns(list, b => (b.Y0, b.Y1, b.X0, b.X1), b => b.Z0, b => b.Z1, (a, b) => a with { Z1 = b.Z1 });
                list.Sort((a, b) => (a.Y0, a.Y1, a.Z0, a.Z1, a.X0).CompareTo((b.Y0, b.Y1, b.Z0, b.Z1, b.X0)));
                list = MergeRuns(list, b => (b.Y0, b.Y1, b.Z0, b.Z1), b => b.X0, b => b.X1, (a, b) => a with { X1 = b.X1 });
                if (list.Count == count)
                {
                    return list;
                }
            }
        }

        private static List<Box> MergeRuns<TKey>(List<Box> sorted, Func<Box, TKey> key, Func<Box, float> start, Func<Box, float> end, Func<Box, Box, Box> join) where TKey : IEquatable<TKey>
        {
            var result = new List<Box>(sorted.Count);
            foreach (var box in sorted)
            {
                if (result.Count > 0 && key(result[^1]).Equals(key(box)) && end(result[^1]) == start(box))
                {
                    result[^1] = join(result[^1], box);
                    continue;
                }
                result.Add(box);
            }
            return result;
        }
    }
}
