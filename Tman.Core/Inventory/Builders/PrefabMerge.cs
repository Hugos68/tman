using System;
using System.Collections.Generic;
using System.Linq;
using GBX.NET;
using GBX.NET.Engines.Plug;
using Tman.Paks;

namespace Tman.Core.Inventory.Builders
{
    public static class PrefabMerge
    {
        public sealed record Part(CPlugStaticObjectModel Model, Gbx Owner, string OwnerPath, Vec3 Position, Quat Rotation);

        public static List<Part> Collect(PakStore store, string prefabPath, Vec3 position, Quat rotation)
        {
            var parts = new List<Part>();
            Collect(store, prefabPath, position, rotation, parts);
            return parts;
        }

        private static void Collect(PakStore store, string path, Vec3 position, Quat rotation, List<Part> parts)
        {
            var gbx = store.Open(path) ?? throw new InvalidOperationException($"Missing prefab {path}");
            switch (gbx.Node)
            {
                case CPlugStaticObjectModel model:
                    parts.Add(new Part(model, gbx, path, position, rotation));
                    return;
                case CPlugPrefab prefab:
                    foreach (var entity in prefab.Ents)
                    {
                        var entityPosition = position + Rotate(rotation, entity.Position);
                        var entityRotation = Multiply(rotation, entity.Rotation);
                        if (entity.Model is CPlugStaticObjectModel inline)
                        {
                            parts.Add(new Part(inline, gbx, path, entityPosition, entityRotation));
                        }
                        else if (entity.ModelFile is { } file)
                        {
                            Collect(store, PakStore.Logical(path, gbx.RefTable!.AncestorLevel, file.FilePath), entityPosition, entityRotation, parts);
                        }
                    }
                    return;
                default:
                    return;
            }
        }

        public static Vec3 Rotate(Quat q, Vec3 v)
        {
            var u = new Vec3(q.X, q.Y, q.Z);
            var s = q.W;
            return u * (2 * Dot(u, v)) + v * (s * s - Dot(u, u)) + Cross(u, v) * (2 * s);
        }

        public static Quat Multiply(Quat a, Quat b) => new(
            a.W * b.X + a.X * b.W + a.Y * b.Z - a.Z * b.Y,
            a.W * b.Y - a.X * b.Z + a.Y * b.W + a.Z * b.X,
            a.W * b.Z + a.X * b.Y - a.Y * b.X + a.Z * b.W,
            a.W * b.W - a.X * b.X - a.Y * b.Y - a.Z * b.Z);

        public static bool IsIdentity(Vec3 position, Quat rotation) =>
            position == Vec3.Zero && MathF.Abs(rotation.X) < 1e-5f && MathF.Abs(rotation.Y) < 1e-5f && MathF.Abs(rotation.Z) < 1e-5f && MathF.Abs(MathF.Abs(rotation.W) - 1) < 1e-5f;

        private static float Dot(Vec3 a, Vec3 b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;

        private static Vec3 Cross(Vec3 a, Vec3 b) => new(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);
    }
}
