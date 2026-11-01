using System;
using System.Collections.Generic;
using System.Linq;
using GBX.NET;
using GBX.NET.Engines.Plug;

namespace Tman.Core.Surfaces.Building
{
    public static class LightmapAtlas
    {
        private static int LightmapSet(CPlugVisual3D visual) => visual.VertexStreams![0].UVs!.Keys.Max();

        private const float Margin = 0.002f;

        private const float ShrinkStep = 0.97f;

        private sealed record Island(IReadOnlyList<CPlugVisual3D> Visuals, Vec2 Min, Vec2 Max, float Width, float Height)
        {
            public Vec2 Origin { get; set; }

            public float Scale { get; set; }
        }

        public sealed record Layout(Vec2 Min, Vec2 Max, float MetersPerUv);

        public static Layout? Repack(IReadOnlyList<IReadOnlyList<CPlugVisual3D>> parts)
        {
            var islands = parts.Select(Measure).Where(island => island is not null).Select(island => island!).OrderByDescending(island => island.Height).ToList();
            if (islands.Count == 0)
            {
                return null;
            }
            Pack(islands);
            foreach (var island in islands)
            {
                Remap(island);
            }
            var visuals = islands.SelectMany(island => island.Visuals).ToList();
            var uvs = visuals.SelectMany(visual => visual.VertexStreams![0].UVs![LightmapSet(visual)]).ToList();
            var min = new Vec2(uvs.Min(uv => uv.X), uvs.Min(uv => uv.Y));
            var max = new Vec2(uvs.Max(uv => uv.X), uvs.Max(uv => uv.Y));
            return new Layout(min, max, MathF.Sqrt(visuals.Sum(Area) / MathF.Max(visuals.Sum(UvArea), 1e-6f)));
        }

        private static Island? Measure(IReadOnlyList<CPlugVisual3D> visuals)
        {
            var mapped = visuals.Where(visual => visual.VertexStreams is [{ UVs.Count: > 1 }, ..]).ToList();
            if (mapped.Count == 0)
            {
                return null;
            }
            var uvs = mapped.SelectMany(visual => visual.VertexStreams![0].UVs![LightmapSet(visual)]).ToList();
            var min = new Vec2(uvs.Min(uv => uv.X), uvs.Min(uv => uv.Y));
            var max = new Vec2(uvs.Max(uv => uv.X), uvs.Max(uv => uv.Y));
            var side = MathF.Sqrt(Math.Max(mapped.Sum(Area), 1e-3f));
            var aspect = MathF.Sqrt(Math.Clamp((max.X - min.X) / MathF.Max(max.Y - min.Y, 1e-6f), 0.1f, 10f));
            return new Island(mapped, min, max, side * aspect, side / aspect);
        }

        private static float UvArea(CPlugVisual3D visual)
        {
            if (visual is not CPlugVisualIndexed { IndexBuffer.Indices: { } indices })
            {
                return 0;
            }
            var uvs = visual.VertexStreams![0].UVs![LightmapSet(visual)];
            var area = 0f;
            for (var i = 0; i + 2 < indices.Length; i += 3)
            {
                var a = uvs[indices[i]];
                var b = uvs[indices[i + 1]];
                var c = uvs[indices[i + 2]];
                area += MathF.Abs((b.X - a.X) * (c.Y - a.Y) - (c.X - a.X) * (b.Y - a.Y)) * 0.5f;
            }
            return area;
        }

        private static float Area(CPlugVisual3D visual)
        {
            if (visual is not CPlugVisualIndexed { IndexBuffer.Indices: { } indices } || visual.VertexStreams![0].Positions is not { } positions)
            {
                return 0;
            }
            var area = 0f;
            for (var i = 0; i + 2 < indices.Length; i += 3)
            {
                var a = positions[indices[i]];
                var b = positions[indices[i + 1]];
                var c = positions[indices[i + 2]];
                area += Cross(b - a, c - a) * 0.5f;
            }
            return area;
        }

        private static float Cross(Vec3 u, Vec3 v) => MathF.Sqrt(MathF.Pow(u.Y * v.Z - u.Z * v.Y, 2) + MathF.Pow(u.Z * v.X - u.X * v.Z, 2) + MathF.Pow(u.X * v.Y - u.Y * v.X, 2));

        private static float Pack(List<Island> islands)
        {
            var total = islands.Sum(island => island.Width * island.Height);
            var scale = 1 / MathF.Sqrt(total);
            while (Shelves(islands, scale) > 1)
            {
                scale *= ShrinkStep;
            }
            while (Shelves(islands, scale / ShrinkStep) <= 1)
            {
                scale /= ShrinkStep;
            }
            return Shelves(islands, scale);
        }

        private static float Shelves(List<Island> islands, float scale)
        {
            var x = Margin;
            var y = Margin;
            var shelf = 0f;
            foreach (var island in islands)
            {
                var width = island.Width * scale;
                var height = island.Height * scale;
                if (width + 2 * Margin > 1)
                {
                    return float.MaxValue;
                }
                if (x + width + Margin > 1 && x > Margin)
                {
                    x = Margin;
                    y += shelf + Margin;
                    shelf = 0;
                }
                island.Origin = new Vec2(x, y);
                island.Scale = scale;
                x += width + Margin;
                shelf = MathF.Max(shelf, height);
            }
            return y + shelf + Margin;
        }

        private static void Remap(Island island)
        {
            var size = island.Max - island.Min;
            var scale = new Vec2(island.Width * island.Scale / MathF.Max(size.X, 1e-6f), island.Height * island.Scale / MathF.Max(size.Y, 1e-6f));
            foreach (var visual in island.Visuals)
            {
                var stream = visual.VertexStreams![0];
                var set = LightmapSet(visual);
                stream.UVs![set] = [.. stream.UVs[set].Select(uv => island.Origin + (uv - island.Min) * scale)];
            }
        }
    }
}
