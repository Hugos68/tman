using System;
using System.Linq;
using GBX.NET.Engines.Plug;
using Tman.Core.Game;
using Tman.Paks;

namespace Tman.Core.Surfaces.Building
{
    internal static class PartGeometry
    {
        public static CPlugSolid2Model Solid(PrefabParts.Part part) =>
            part.Model.Mesh ?? (part.Model.MeshFile is { } file ? Referenced(part, file.FilePath) as CPlugSolid2Model : null)
            ?? throw new InvalidOperationException($"{part.OwnerPath} has a static object without a mesh");

        public static CPlugSurface? Shape(PrefabParts.Part part) =>
            part.Model.Shape ?? (part.Model.ShapeFile is { } file ? Referenced(part, file.FilePath) as CPlugSurface : null);

        public static CPlugSurface.Mesh? CollisionMesh(PrefabParts.Part part) => Shape(part)?.Surf as CPlugSurface.Mesh;

        public static string[] MaterialLinks(PrefabParts.Part part, CPlugSolid2Model solid, CPlugSurface? shape)
        {
            if (solid.Materials is { } externals)
            {
                return [.. externals.Select((material, index) => material.File is { } file ? Link(part, file.FilePath) : ShapeLink(part, shape, index))];
            }
            return [.. (solid.MaterialIds ?? []).Select(id => $"{solid.MaterialsFolderName}{id}")];
        }

        public static string Link(PrefabParts.Part part, string filePath) => GameMaterials.Link(part.OwnerPath, part.Owner, filePath);

        private static string ShapeLink(PrefabParts.Part part, CPlugSurface? shape, int index) =>
            shape?.Materials?.ElementAtOrDefault(index)?.MaterialFile is { } file
                ? Link(part, file.FilePath)
                : throw new InvalidOperationException($"Material {index} of {part.OwnerPath} has no file");

        private static object? Referenced(PrefabParts.Part part, string filePath) =>
            GamePaks.Store.Open(PakStore.Logical(part.OwnerPath, part.Owner.RefTable!.AncestorLevel, filePath))?.Node;
    }
}
