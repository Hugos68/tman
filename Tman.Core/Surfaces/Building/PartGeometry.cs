using System;
using System.Linq;
using GBX.NET.Engines.Plug;
using Tman.Core.Game;
using Tman.Paks;

namespace Tman.Core.Surfaces.Building
{
    internal sealed class PartGeometry(IPakStore paks, GameMaterials materials)
    {
        public CPlugSolid2Model Solid(PrefabParts.Part part) =>
            part.Model.Mesh ?? (part.Model.MeshFile is { } file ? Referenced(part, file.FilePath) as CPlugSolid2Model : null)
            ?? throw new InvalidOperationException($"{part.OwnerPath} has a static object without a mesh");

        public CPlugSurface? Shape(PrefabParts.Part part) =>
            part.Model.Shape ?? (part.Model.ShapeFile is { } file ? Referenced(part, file.FilePath) as CPlugSurface : null);

        public CPlugSurface.Mesh? CollisionMesh(PrefabParts.Part part) => Shape(part)?.Surf as CPlugSurface.Mesh;

        public string[] MaterialLinks(PrefabParts.Part part, CPlugSolid2Model solid, CPlugSurface? shape)
        {
            if (solid.Materials is { } externals)
            {
                return [.. externals.Select((material, index) => material.File is { } file ? Link(part, file.FilePath) : ShapeLink(part, shape, index))];
            }
            return [.. (solid.MaterialIds ?? []).Select(id => $"{solid.MaterialsFolderName}{id}")];
        }

        public string Link(PrefabParts.Part part, string filePath) => materials.Link(part.OwnerPath, part.Owner, filePath);

        private string ShapeLink(PrefabParts.Part part, CPlugSurface? shape, int index) =>
            shape?.Materials?.ElementAtOrDefault(index)?.MaterialFile is { } file
                ? Link(part, file.FilePath)
                : throw new InvalidOperationException($"Material {index} of {part.OwnerPath} has no file");

        private object? Referenced(PrefabParts.Part part, string filePath) =>
            paks.Open(PakStore.Logical(part.OwnerPath, part.Owner.RefTable!.AncestorLevel, filePath))?.Node;
    }
}
