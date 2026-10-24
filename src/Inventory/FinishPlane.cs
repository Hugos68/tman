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
        private const float IndicatorTile = 4.0f;

        public readonly record struct Box(float X0, float X1, float Z0, float Z1, float Y0, float Y1, float Indicator);

        public static void Place(CGameCtnChallenge map, string templatePath, string ident, IReadOnlyList<Box> boxes)
        {
            var path = Path.Combine(Path.GetTempPath(), ident);
            File.WriteAllBytes(path, Build(templatePath, ident, boxes));
            EmbedUtility.Embed(map, path);
            map.PlaceAnchoredObject(EmbedUtility.ExtractIdent(path), Vec3.Zero, Vec3.Zero, Vec3.Zero);
        }

        public static byte[] Build(string templatePath, string ident, IReadOnlyList<Box> boxes)
        {
            var vertices = new List<Vec3>();
            var triangles = new List<CPlugSurface.Mesh.Triangle>();
            var positions = new List<Vec3>();
            var normals = new List<Vec3>();
            var uvs = new List<Vec2>();
            var indices = new List<int>();
            int[][] quads = [[0, 3, 2, 1], [4, 5, 6, 7], [0, 1, 5, 4], [2, 3, 7, 6], [1, 2, 6, 5], [0, 4, 7, 3]];
            foreach (var (x0, x1, z0, z1, y0, y1, indicator) in boxes)
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
            foreach (var set in stream.UVs.Keys.DefaultIfEmpty(0).ToList())
            {
                stream.UVs[set] = uvs.ToArray();
            }
            foreach (var set in stream.Colors.Keys.DefaultIfEmpty(0).ToList())
            {
                stream.Colors[set] = Enumerable.Repeat(unchecked((int)0xFFFFFFFF), positions.Count).ToArray();
            }
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
