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

        public static void Place(CGameCtnChallenge map, string templatePath, string ident, IReadOnlyList<Box> boxes) => Place(map, templatePath, ident, boxes, [], true);

        public static void Place(CGameCtnChallenge map, string templatePath, string ident, IReadOnlyList<Box> boxes, bool visible) => Place(map, templatePath, ident, boxes, [], visible);

        public static void Place(CGameCtnChallenge map, string templatePath, string ident, IReadOnlyList<Box> boxes, IReadOnlyList<Ramp> ramps) => Place(map, templatePath, ident, boxes, ramps, true);

        public static void Place(CGameCtnChallenge map, string templatePath, string ident, IReadOnlyList<Box> boxes, IReadOnlyList<Ramp> ramps, bool visible)
        {
            var path = Path.Combine(Path.GetTempPath(), ident);
            File.WriteAllBytes(path, Build(templatePath, ident, map.MapInfo.Collection, boxes, ramps, visible));
            EmbedUtility.Embed(map, path);
            map.PlaceAnchoredObject(EmbedUtility.ExtractIdent(path), Vec3.Zero, Vec3.Zero, Vec3.Zero);
        }

        public static byte[] Build(string templatePath, string ident, Id collection, IReadOnlyList<Box> boxes, IReadOnlyList<Ramp> ramps) => Build(templatePath, ident, collection, boxes, ramps, true);

        public static byte[] Build(string templatePath, string ident, Id collection, IReadOnlyList<Box> boxes, IReadOnlyList<Ramp> ramps, bool visible)
        {
            var bodies = new List<Vec3[]>();
            foreach (var b in Merge(boxes))
            {
                bodies.Add(Body(b.X0, b.X1, b.Z0, b.Z1, (x, z) => b.Y0, (x, z) => b.Y1));
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
                bodies.Add(Body(r.X0, r.X1, r.Z0, r.Z1, (x, z) => Y(x, z, -r.Below), (x, z) => Y(x, z, r.Above)));
            }
            if (bodies.Count == 0)
            {
                throw new InvalidOperationException("no boxes");
            }

            var gbx = Gbx.Parse<CGameItemModel>(templatePath);
            var item = gbx.Node;
            if (item.EntityModelEdition is not CGameCommonItemEntityModelEdition edition || edition.MeshCrystal is not CPlugCrystal crystal)
            {
                throw new InvalidOperationException($"{templatePath} is not a mesh-modeler item");
            }
            var geometry = crystal.Layers.OfType<CPlugCrystal.GeometryLayer>().First();
            var trigger = crystal.Layers.OfType<CPlugCrystal.TriggerLayer>().First();
            if (geometry.Crystal is null || trigger.Crystal is null || crystal.Materials.Count < 2)
            {
                throw new InvalidOperationException($"{templatePath} has no geometry + trigger layers with two materials");
            }
            var glass = visible
                ? bodies.SelectMany(b => new[] { Face(b, 3, 7, 6, 2), Face(b, 0, 1, 5, 4) }).ToList()
                : [Speck(bodies[0])];
            var prism = bodies.SelectMany(b => new[] { Face(b, 3, 7, 6, 2), Face(b, 0, 1, 5, 4), Face(b, 4, 0, 3, 7), Face(b, 1, 5, 6, 2), Face(b, 4, 5, 1, 0), Face(b, 3, 2, 6, 7) }).ToList();
            Fill(geometry.Crystal, glass, crystal.Materials[0], true);
            Fill(trigger.Crystal, prism, crystal.Materials[1], false);
            geometry.IsVisible = true;
            geometry.Collidable = false;
            var smoothing = crystal.Chunks.Get<CPlugCrystal.Chunk09003007>();
            if (smoothing is not null)
            {
                smoothing.U01 = Enumerable.Repeat(2, geometry.Crystal.Faces.Length).ToArray();
            }

            item.Ident = new Ident(ident, collection, ident);
            using var ms = new MemoryStream();
            gbx.Save(ms);
            return ms.ToArray();
        }

        private static Vec3[] Body(float x0, float x1, float z0, float z1, Func<float, float, float> bottom, Func<float, float, float> top) =>
        [
            new Vec3(x0, bottom(x0, z0), z0), new Vec3(x1, bottom(x1, z0), z0), new Vec3(x1, top(x1, z0), z0), new Vec3(x0, top(x0, z0), z0),
            new Vec3(x0, bottom(x0, z1), z1), new Vec3(x1, bottom(x1, z1), z1), new Vec3(x1, top(x1, z1), z1), new Vec3(x0, top(x0, z1), z1),
        ];

        private static Vec3[] Speck(Vec3[] body)
        {
            const float size = 0.001f;
            var o = body[0];
            return [o, new Vec3(o.X, o.Y, o.Z + size), new Vec3(o.X + size, o.Y, o.Z + size), new Vec3(o.X + size, o.Y, o.Z)];
        }

        private static Vec3[] Face(Vec3[] body, int a, int b, int c, int d)
        {
            Vec3[] p = [body[a], body[b], body[c], body[d]];
            var centre = body.Aggregate((u, v) => u + v) / body.Length;
            var n = Vec3.GetCrossProduct(p[1] - p[0], p[2] - p[0]);
            return Vec3.GetDotProduct((p[0] + p[2]) / 2 - centre, n) < 0 ? [p[3], p[2], p[1], p[0]] : p;
        }

        private static void Fill(CPlugCrystal.Crystal crystal, List<Vec3[]> faces, CPlugCrystal.Material material, bool lightmap)
        {
            var group = crystal.Groups[0];
            var positions = new List<Vec3>();
            var up = faces.Where(f => Vec3.GetCrossProduct(f[1] - f[0], f[2] - f[0]).Y > 0).SelectMany(f => f).ToList();
            var down = faces.Where(f => Vec3.GetCrossProduct(f[1] - f[0], f[2] - f[0]).Y < 0).SelectMany(f => f).ToList();
            (Vec2 Min, float Scale) Island(List<Vec3> corners)
            {
                if (corners.Count == 0)
                {
                    return (Vec2.Zero, 1);
                }
                var min = new Vec2(corners.Min(v => v.X), corners.Min(v => v.Z));
                var extent = Math.Max(Math.Max(corners.Max(v => v.X) - min.X, corners.Max(v => v.Z) - min.Y), 1e-3f);
                return (min, 0.48f / extent);
            }
            var upIsland = Island(up);
            var downIsland = Island(down);
            Vec2[] texCoords = [new Vec2(4, 4), new Vec2(4, 0), new Vec2(0, 0), new Vec2(0, 4)];
            var result = new CPlugCrystal.Face[faces.Count];
            for (var i = 0; i < faces.Count; i++)
            {
                var f = faces[i];
                var facesUp = Vec3.GetCrossProduct(f[1] - f[0], f[2] - f[0]).Y > 0;
                var island = facesUp ? upIsland : downIsland;
                var origin = facesUp ? new Vec2(0.01f, 0.01f) : new Vec2(0.51f, 0.01f);
                var vertices = new CPlugCrystal.Vertex[f.Length];
                for (var k = 0; k < f.Length; k++)
                {
                    var lm = lightmap ? new Vec2(origin.X + (f[k].X - island.Min.X) * island.Scale, origin.Y + (f[k].Z - island.Min.Y) * island.Scale) : Vec2.Zero;
                    vertices[k] = new CPlugCrystal.Vertex(positions.Count, texCoords[k % 4], lm);
                    positions.Add(f[k]);
                }
                result[i] = new CPlugCrystal.Face(vertices, group, material, null);
            }
            crystal.Positions = positions.ToArray();
            crystal.Edges = [];
            crystal.Faces = result;
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
