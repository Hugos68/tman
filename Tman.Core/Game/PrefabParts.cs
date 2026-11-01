using System;
using System.Collections.Generic;
using GBX.NET;
using GBX.NET.Engines.Plug;
using Tman.Paks;

namespace Tman.Core.Game
{
    internal static class PrefabParts
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
                        var entityPosition = position + Transforms.Rotate(rotation, entity.Position);
                        var entityRotation = Transforms.Multiply(rotation, entity.Rotation);
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
    }
}
