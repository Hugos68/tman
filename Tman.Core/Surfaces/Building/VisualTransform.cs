using System.Collections.Generic;
using System.Linq;
using GBX.NET;
using GBX.NET.Engines.Plug;
using Tman.Core.Game;

namespace Tman.Core.Surfaces.Building
{
    internal static class VisualTransform
    {
        public static CPlugVisual3D Copy(CPlugVisual3D visual, Vec3 position, Quat rotation) => Transformed(Detached((CPlugVisual3D)visual.DeepClone()), position, rotation);

        public static CPlugVisual3D Detached(CPlugVisual3D visual)
        {
            visual.VertexStreams = [.. (visual.VertexStreams ?? []).Select(stream =>
            {
                var copy = (CPlugVertexStream)stream.DeepClone();
                copy.UVs = new SortedDictionary<int, Vec2[]>((stream.UVs ?? []).ToDictionary(set => set.Key, set => set.Value.ToArray()));
                return copy;
            })];
            return visual;
        }

        public static BoxAligned Bounds(IEnumerable<Vec3> points)
        {
            var list = points.ToList();
            var min = new Vec3(list.Min(p => p.X), list.Min(p => p.Y), list.Min(p => p.Z));
            var max = new Vec3(list.Max(p => p.X), list.Max(p => p.Y), list.Max(p => p.Z));
            var center = (min + max) / 2;
            var half = (max - min) / 2;
            return new BoxAligned(center.X, center.Y, center.Z, half.X, half.Y, half.Z);
        }

        private static CPlugVisual3D Transformed(CPlugVisual3D visual, Vec3 position, Quat rotation)
        {
            if (Transforms.IsIdentity(position, rotation))
            {
                return visual;
            }
            foreach (var stream in visual.VertexStreams!)
            {
                if (stream.Positions is { } positions)
                {
                    stream.Positions = [.. positions.Select(vertex => Transforms.Rotate(rotation, vertex) + position)];
                }
                if (stream.Normals is { } normals)
                {
                    stream.Normals = [.. normals.Select(normal => Transforms.Rotate(rotation, normal))];
                }
            }
            if (visual.Tangents is { } tangents)
            {
                visual.Tangents = [.. tangents.Select(tangent => Transforms.Rotate(rotation, tangent))];
            }
            if (visual.BiTangents is { } biTangents)
            {
                visual.BiTangents = [.. biTangents.Select(biTangent => Transforms.Rotate(rotation, biTangent))];
            }
            visual.BoundingBox = Bounds(visual.VertexStreams![0].Positions!);
            return visual;
        }
    }
}
