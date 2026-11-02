using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GBX.NET;
using GBX.NET.Engines.GameData;
using GBX.NET.Engines.Plug;

namespace Tman.Core.Game
{
    internal static class ModelerItem
    {
        private const float LightmapHalf = 0.5f;

        private const float LightmapMargin = 0.01f;

        private const float LightmapIslandSize = LightmapHalf - 2 * LightmapMargin;

        private const float MinimumExtent = 1e-3f;

        private const float SpeckSize = 0.001f;

        private const int TextureRepeats = 4;

        private const int SmoothingGroup = 2;

        private static readonly int[][] TopAndBottom = [[3, 7, 6, 2], [0, 1, 5, 4]];

        private static readonly int[][] AllSides = [[3, 7, 6, 2], [0, 1, 5, 4], [4, 0, 3, 7], [1, 5, 6, 2], [4, 5, 1, 0], [3, 2, 6, 7]];

        public static byte[] Build(Stream template, Ident ident, IReadOnlyList<Vec3[]> bodies, bool visible)
        {
            if (bodies.Count == 0)
            {
                throw new InvalidOperationException($"{ident.Id}: no bodies");
            }
            var gbx = Gbx.Parse<CGameItemModel>(template);
            var item = gbx.Node;
            if (item.EntityModelEdition is not CGameCommonItemEntityModelEdition edition || edition.MeshCrystal is not CPlugCrystal crystal)
            {
                throw new InvalidOperationException($"{ident.Id}: the template is not a mesh-modeler item");
            }
            var geometry = crystal.Layers.OfType<CPlugCrystal.GeometryLayer>().First();
            var trigger = crystal.Layers.OfType<CPlugCrystal.TriggerLayer>().First();
            if (geometry.Crystal is null || trigger.Crystal is null || crystal.Materials.Count < 2)
            {
                throw new InvalidOperationException($"{ident.Id}: the template has no geometry + trigger layers with two materials");
            }
            var glass = visible ? bodies.SelectMany(body => TopAndBottom.Select(corners => Face(body, corners))).ToList() : [Speck(bodies[0])];
            var prism = bodies.SelectMany(body => AllSides.Select(corners => Face(body, corners))).ToList();
            Fill(geometry.Crystal, glass, crystal.Materials[0], true);
            Fill(trigger.Crystal, prism, crystal.Materials[1], false);
            geometry.IsVisible = true;
            geometry.Collidable = false;
            if (crystal.Chunks.Get<CPlugCrystal.Chunk09003007>() is { } smoothing)
            {
                smoothing.U01 = [.. Enumerable.Repeat(SmoothingGroup, geometry.Crystal.Faces.Length)];
            }
            item.Ident = ident;
            using var stream = new MemoryStream();
            gbx.Save(stream);
            return stream.ToArray();
        }

        private static Vec3[] Speck(Vec3[] body)
        {
            var origin = body[0];
            return [origin, new Vec3(origin.X, origin.Y, origin.Z + SpeckSize), new Vec3(origin.X + SpeckSize, origin.Y, origin.Z + SpeckSize), new Vec3(origin.X + SpeckSize, origin.Y, origin.Z)];
        }

        private static Vec3[] Face(Vec3[] body, int[] corners)
        {
            Vec3[] face = [body[corners[0]], body[corners[1]], body[corners[2]], body[corners[3]]];
            var centre = body.Aggregate((u, v) => u + v) / body.Length;
            var normal = Vec3.GetCrossProduct(face[1] - face[0], face[2] - face[0]);
            return Vec3.GetDotProduct((face[0] + face[2]) / 2 - centre, normal) < 0 ? [face[3], face[2], face[1], face[0]] : face;
        }

        private static bool FacesUp(Vec3[] face) => Vec3.GetCrossProduct(face[1] - face[0], face[2] - face[0]).Y > 0;

        private static bool FacesDown(Vec3[] face) => Vec3.GetCrossProduct(face[1] - face[0], face[2] - face[0]).Y < 0;

        private static void Fill(CPlugCrystal.Crystal crystal, List<Vec3[]> faces, CPlugCrystal.Material material, bool lightmap)
        {
            var group = crystal.Groups[0];
            var positions = new List<Vec3>();
            var upIsland = Island(faces.Where(FacesUp).SelectMany(face => face).ToList());
            var downIsland = Island(faces.Where(FacesDown).SelectMany(face => face).ToList());
            Vec2[] texCoords = [new Vec2(TextureRepeats, TextureRepeats), new Vec2(TextureRepeats, 0), new Vec2(0, 0), new Vec2(0, TextureRepeats)];
            var result = new CPlugCrystal.Face[faces.Count];
            for (var i = 0; i < faces.Count; i++)
            {
                var face = faces[i];
                var island = FacesUp(face) ? upIsland : downIsland;
                var origin = FacesUp(face) ? new Vec2(LightmapMargin, LightmapMargin) : new Vec2(LightmapHalf + LightmapMargin, LightmapMargin);
                var vertices = new CPlugCrystal.Vertex[face.Length];
                for (var k = 0; k < face.Length; k++)
                {
                    var lightmapCoords = lightmap ? new Vec2(origin.X + (face[k].X - island.Min.X) * island.Scale, origin.Y + (face[k].Z - island.Min.Y) * island.Scale) : Vec2.Zero;
                    vertices[k] = new CPlugCrystal.Vertex(positions.Count, texCoords[k % texCoords.Length], lightmapCoords);
                    positions.Add(face[k]);
                }
                result[i] = new CPlugCrystal.Face(vertices, group, material, null);
            }
            crystal.Positions = [.. positions];
            crystal.Edges = [];
            crystal.Faces = result;
        }

        private static (Vec2 Min, float Scale) Island(List<Vec3> corners)
        {
            if (corners.Count == 0)
            {
                return (Vec2.Zero, 1);
            }
            var min = new Vec2(corners.Min(v => v.X), corners.Min(v => v.Z));
            var extent = Math.Max(Math.Max(corners.Max(v => v.X) - min.X, corners.Max(v => v.Z) - min.Y), MinimumExtent);
            return (min, LightmapIslandSize / extent);
        }
    }
}
