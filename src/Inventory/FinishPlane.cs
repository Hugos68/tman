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
        private const float IndicatorTile = 8.0f;

        public readonly record struct Box(float X0, float X1, float Z0, float Z1, float Y0, float Y1, float Indicator);

        public enum Side { North, South, East, West }

        public readonly record struct Ramp(float X0, float X1, float Z0, float Z1, float YInner, float YOuter, float Below, float Above, float Lift, Side Side);

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
            var positions = new List<Vec3>();
            var normals = new List<Vec3>();
            var uvs = new List<Vec2>();
            var indices = new List<int>();
            int[][] quads = [[0, 3, 2, 1], [4, 5, 6, 7], [0, 1, 5, 4], [2, 3, 7, 6], [1, 2, 6, 5], [0, 4, 7, 3]];
            foreach (var (x0, x1, z0, z1, y0, y1, indicator) in Merge(boxes))
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
                positions.AddRange([new(x0, indicator, z0), new(x0, indicator, z1), new(x1, indicator, z1), new(x1, indicator, z0)]);
                normals.AddRange([new(0, 1, 0), new(0, 1, 0), new(0, 1, 0), new(0, 1, 0)]);
                uvs.AddRange([new(x0 / IndicatorTile, z0 / IndicatorTile), new(x0 / IndicatorTile, z1 / IndicatorTile), new(x1 / IndicatorTile, z1 / IndicatorTile), new(x1 / IndicatorTile, z0 / IndicatorTile)]);
                indices.AddRange([p, p + 1, p + 2, p, p + 2, p + 3]);
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
                var p = positions.Count;
                positions.AddRange([new(r.X0, Y(r.X0, r.Z0, r.Lift), r.Z0), new(r.X0, Y(r.X0, r.Z1, r.Lift), r.Z1), new(r.X1, Y(r.X1, r.Z1, r.Lift), r.Z1), new(r.X1, Y(r.X1, r.Z0, r.Lift), r.Z0)]);
                normals.AddRange([new(0, 1, 0), new(0, 1, 0), new(0, 1, 0), new(0, 1, 0)]);
                uvs.AddRange([new(r.X0 / IndicatorTile, r.Z0 / IndicatorTile), new(r.X0 / IndicatorTile, r.Z1 / IndicatorTile), new(r.X1 / IndicatorTile, r.Z1 / IndicatorTile), new(r.X1 / IndicatorTile, r.Z0 / IndicatorTile)]);
                indices.AddRange([p, p + 1, p + 2, p, p + 2, p + 3]);
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

            if (entity.StaticObject?.Mesh is not CPlugSolid2Model solid || solid.Visuals?.FirstOrDefault() is not CPlugVisualIndexedTriangles visual || visual.VertexStreams.FirstOrDefault() is not CPlugVertexStream stream)
            {
                throw new InvalidOperationException($"{templatePath} has no indexed visual with a vertex stream");
            }
            stream.Positions = positions.ToArray();
            stream.Normals = normals.ToArray();
            stream.UVs = new SortedDictionary<int, Vec2[]> { [0] = uvs.ToArray() };
            stream.Colors = new SortedDictionary<int, int[]> { [0] = Enumerable.Repeat(unchecked((int)0xFFFFFFFF), positions.Count).ToArray() };
            SetCount(stream, positions.Count);
            SetCount(visual, positions.Count);
            visual.IndexBuffer!.Indices = indices.ToArray();
            var min = positions.Aggregate((a, b) => new Vec3(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Min(a.Z, b.Z)));
            var max = positions.Aggregate((a, b) => new Vec3(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y), Math.Max(a.Z, b.Z)));
            visual.BoundingBox = new BoxAligned(min.X, min.Y, min.Z, max.X, max.Y, max.Z);

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
                list.Sort((a, b) => (a.Y0, a.Y1, a.Indicator, a.X0, a.X1, a.Z0).CompareTo((b.Y0, b.Y1, b.Indicator, b.X0, b.X1, b.Z0)));
                list = MergeRuns(list, b => (b.Y0, b.Y1, b.Indicator, b.X0, b.X1), b => b.Z0, b => b.Z1, (a, b) => a with { Z1 = b.Z1 });
                list.Sort((a, b) => (a.Y0, a.Y1, a.Indicator, a.Z0, a.Z1, a.X0).CompareTo((b.Y0, b.Y1, b.Indicator, b.Z0, b.Z1, b.X0)));
                list = MergeRuns(list, b => (b.Y0, b.Y1, b.Indicator, b.Z0, b.Z1), b => b.X0, b => b.X1, (a, b) => a with { X1 = b.X1 });
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
    }
}
