using System;
using System.Collections.Generic;
using System.Linq;
using GBX.NET;
using GBX.NET.Engines.Plug;
using Tman.Paks;
using Tman.Core.Game;

namespace Tman.Core.Surfaces.Building
{
    public static class SurfaceBlockMesh
    {
        private const string MaterialExtension = ".Material.Gbx";

        private const float DefaultTextureSizeInMeters = 1;

        public static CPlugStaticObjectModel Merge(IReadOnlyList<PrefabMerge.Part> parts, DeckFamily family, SurfaceTarget target, MaterialDressing dressing)
        {
            var collidable = parts.First(part => Shape(part)?.Surf is CPlugSurface.Mesh);
            var model = (CPlugStaticObjectModel)collidable.Model.DeepClone();
            model.Mesh = (CPlugSolid2Model)Solid(collidable).DeepClone();
            model.Shape = (CPlugSurface)Shape(collidable)!.DeepClone();
            var solid = model.Mesh;
            var shape = model.Shape;
            var source = (CPlugSurface.Mesh)shape.Surf!;
            var mesh = new CPlugSurface.Mesh { Version = source.Version, OctreeVersion = source.OctreeVersion, GameplayMainDir = source.GameplayMainDir };
            shape.Surf = mesh;
            var visuals = new List<CPlugVisual>();
            var islands = new List<IReadOnlyList<CPlugVisual3D>>();
            var geoms = new List<CPlugSolid2Model.ShadedGeom>();
            var materials = new List<CPlugSolid2Model.Material>();
            var materialIndices = new Dictionary<(string Link, CPlugSurface.MaterialId Physics, CPlugSurface.GameplayId Gameplay), int>();
            var vertices = new List<Vec3>();
            var triangles = new List<CPlugSurface.Mesh.Triangle>();
            var surfaceSlots = 0;
            var lodDistances = LodLevels.Merge(parts.Select(part => Solid(part).LodMaxDistAtFov90 ?? []));

            foreach (var part in parts)
            {
                var partSolid = Solid(part);
                var partShape = Shape(part);
                var links = Links(part, partSolid, partShape);
                var materialMap = new int[links.Length];
                var overlayMap = new int?[links.Length];
                for (var slot = 0; slot < links.Length; slot++)
                {
                    var deck = IsDeck(links[slot], family);
                    var overlay = family.DeckOverlay(links[slot]);
                    var strip = dressing.IsGameplayStrip(links[slot]);
                    var link = deck || overlay is not null ? target.Link : dressing.Dress(links[slot]);
                    var (physics, gameplay) = strip ? (target.Physics, dressing.Gameplay(links[slot])) : SurfaceOf(link);
                    materialMap[slot] = MaterialIndex(materials, materialIndices, link, deck || overlay is not null ? target.Physics : physics, gameplay);
                    if (overlay is not null && MaterialDressing.OverlayTwin(overlay) is { } twin)
                    {
                        overlayMap[slot] = MaterialIndex(materials, materialIndices, twin, CPlugSurface.MaterialId.NotCollidable, CPlugSurface.GameplayId.None);
                    }
                }
                var visualBase = visuals.Count;
                var partVisuals = new List<CPlugVisual3D>();
                foreach (var visual in partSolid.Visuals ?? [])
                {
                    var copy = Transformed(Detached((CPlugVisual3D)visual.DeepClone()), part.Position, part.Rotation);
                    visuals.Add(copy);
                    partVisuals.Add(copy);
                }
                islands.Add(partVisuals);
                foreach (var geom in partSolid.ShadedGeoms ?? [])
                {
                    var lodMask = LodLevels.Remap(geom.LodMask, partSolid.LodMaxDistAtFov90 ?? [], lodDistances);
                    geoms.Add(new CPlugSolid2Model.ShadedGeom
                    {
                        VisualIndex = geom.VisualIndex + visualBase,
                        MaterialIndex = materialMap[geom.MaterialIndex],
                        U01 = geom.U01,
                        LodMask = lodMask,
                        U02 = geom.U02
                    });
                    if (overlayMap[geom.MaterialIndex] is { } overlayIndex)
                    {
                        var decal = DecalCopy(partVisuals[geom.VisualIndex]);
                        visuals.Add(decal);
                        partVisuals.Add(decal);
                        geoms.Add(new CPlugSolid2Model.ShadedGeom
                        {
                            VisualIndex = visuals.Count - 1,
                            MaterialIndex = overlayIndex,
                            U01 = geom.U01,
                            LodMask = lodMask,
                            U02 = geom.U02
                        });
                    }
                }
                if (partShape?.Surf is not CPlugSurface.Mesh partMesh)
                {
                    continue;
                }
                var slots = ShapeSlots(part, partShape, family, target, dressing);
                var vertexBase = vertices.Count;
                var surfaceBase = surfaceSlots;
                surfaceSlots += slots.Length;
                vertices.AddRange(partMesh.Vertices!.Select(vertex => PrefabMerge.Rotate(part.Rotation, vertex) + part.Position));
                foreach (var triangle in partMesh.Triangles!)
                {
                    var slot = slots[triangle.SurfaceIndex];
                    triangles.Add(new CPlugSurface.Mesh.Triangle
                    {
                        Indices = triangle.Indices + new Int3(vertexBase, vertexBase, vertexBase),
                        SurfaceIndex = (short)(triangle.SurfaceIndex + surfaceBase),
                        U02 = slot.Deck ? (byte)target.Physics : triangle.U02,
                        U03 = slot.Gameplay is { } gameplay ? (byte)gameplay : triangle.U03
                    });
                }

            }
            solid.LodMaxDistAtFov90 = lodDistances;
            if (LightmapAtlas.Repack(islands) is { } layout && solid.PreLightGenerator is { } lightmap)
            {
                lightmap.U02 = parts.Max(part => Solid(part).PreLightGenerator?.U02 ?? 0);
                lightmap.U06 = layout.Max.X;
                lightmap.U07 = layout.Max.Y;
            }
            solid.Visuals = [.. visuals];
            solid.ShadedGeoms = [.. geoms];
            solid.CustomMaterials = [.. materials];
            solid.Materials = null;
            solid.MaterialIds = [];
            solid.MaterialInsts = null;
            solid.Lights = [];
            solid.LightUserModels = [];
            solid.LightInsts = [];
            solid.FileWriteTime = null;
            solid.Boxes = [Bounds(vertices.Concat(visuals.SelectMany(visual => ((CPlugVisual3D)visual).VertexStreams![0].Positions!)))];
            mesh.Vertices = [.. vertices];
            mesh.Triangles = [.. triangles];
            shape.Materials = [];
            return model;
        }

        public static CPlugSurface Standalone(CPlugSurface surface, Gbx owner, string ownerPath)
        {
            var copy = (CPlugSurface)surface.DeepClone();
            copy.Materials = [.. (surface.Materials ?? []).Select(material => new CPlugSurface.SurfMaterial
            {
                SurfaceId = material.MaterialFile is { } file ? PhysicsOf(ResolveLink(ownerPath, owner, file.FilePath)) : material.Material?.SurfaceId ?? material.SurfaceId ?? CPlugSurface.MaterialId.Asphalt
            })];
            return copy;
        }

        private static CPlugVisual3D Detached(CPlugVisual3D visual)
        {
            visual.VertexStreams = [.. (visual.VertexStreams ?? []).Select(stream =>
            {
                var copy = (CPlugVertexStream)stream.DeepClone();
                copy.UVs = new SortedDictionary<int, Vec2[]>((stream.UVs ?? []).ToDictionary(set => set.Key, set => set.Value.ToArray()));
                return copy;
            })];
            return visual;
        }

        private static CPlugVisual3D Transformed(CPlugVisual3D visual, Vec3 position, Quat rotation)
        {
            if (PrefabMerge.IsIdentity(position, rotation))
            {
                return visual;
            }
            foreach (var stream in visual.VertexStreams!)
            {
                if (stream.Positions is { } positions)
                {
                    stream.Positions = [.. positions.Select(vertex => PrefabMerge.Rotate(rotation, vertex) + position)];
                }
                if (stream.Normals is { } normals)
                {
                    stream.Normals = [.. normals.Select(normal => PrefabMerge.Rotate(rotation, normal))];
                }
            }
            if (visual.Tangents is { } tangents)
            {
                visual.Tangents = [.. tangents.Select(tangent => PrefabMerge.Rotate(rotation, tangent))];
            }
            if (visual.BiTangents is { } biTangents)
            {
                visual.BiTangents = [.. biTangents.Select(biTangent => PrefabMerge.Rotate(rotation, biTangent))];
            }
            visual.BoundingBox = Bounds(visual.VertexStreams![0].Positions!);
            return visual;
        }

        private static BoxAligned Bounds(IEnumerable<Vec3> points)
        {
            var list = points.ToList();
            var min = new Vec3(list.Min(p => p.X), list.Min(p => p.Y), list.Min(p => p.Z));
            var max = new Vec3(list.Max(p => p.X), list.Max(p => p.Y), list.Max(p => p.Z));
            var center = (min + max) * 0.5f;
            var half = (max - min) * 0.5f;
            return new BoxAligned(center.X, center.Y, center.Z, half.X, half.Y, half.Z);
        }

        private static CPlugSolid2Model Solid(PrefabMerge.Part part) =>
            part.Model.Mesh ?? (part.Model.MeshFile is { } file ? GamePaks.Store.Open(PakStore.Logical(part.OwnerPath, part.Owner.RefTable!.AncestorLevel, file.FilePath))?.Node as CPlugSolid2Model : null)
            ?? throw new InvalidOperationException($"{part.OwnerPath} has a static object without a mesh");

        private static CPlugSurface? Shape(PrefabMerge.Part part) =>
            part.Model.Shape ?? (part.Model.ShapeFile is { } file ? GamePaks.Store.Open(PakStore.Logical(part.OwnerPath, part.Owner.RefTable!.AncestorLevel, file.FilePath))?.Node as CPlugSurface : null);

        private static string[] Links(PrefabMerge.Part part, CPlugSolid2Model solid, CPlugSurface? shape)
        {
            if (solid.Materials is { } externals)
            {
                return [.. externals.Select((material, index) => material.File is { } file ? Resolve(part, file.FilePath) : ShapeLink(part, shape, index))];
            }
            return [.. (solid.MaterialIds ?? []).Select(id => $@"{solid.MaterialsFolderName}{id}")];
        }

        private static string ShapeLink(PrefabMerge.Part part, CPlugSurface? shape, int index) =>
            shape?.Materials?.ElementAtOrDefault(index)?.MaterialFile is { } file
                ? Resolve(part, file.FilePath)
                : throw new InvalidOperationException($"Material {index} of {part.OwnerPath} has no file");

        private static (bool Deck, CPlugSurface.MaterialId Physics, CPlugSurface.GameplayId? Gameplay)[] ShapeSlots(PrefabMerge.Part part, CPlugSurface shape, DeckFamily family, SurfaceTarget target, MaterialDressing dressing) =>
            [.. shape.Materials!.Select<CPlugSurface.SurfMaterial, (bool, CPlugSurface.MaterialId, CPlugSurface.GameplayId?)>(material =>
            {
                if (material.MaterialFile is { } file)
                {
                    var link = Resolve(part, file.FilePath);
                    return dressing.IsGameplayStrip(link) ? (true, target.Physics, dressing.Gameplay(link))
                        : IsDeck(link, family) || family.DeckOverlay(link) is not null ? (true, target.Physics, null)
                        : (false, PhysicsOf(dressing.Dress(link)), null);
                }
                return material.Material is { } node ? (false, node.SurfaceId, null) : (false, material.SurfaceId ?? CPlugSurface.MaterialId.Asphalt, null);
            })];

        private static string Resolve(PrefabMerge.Part part, string filePath) => ResolveLink(part.OwnerPath, part.Owner, filePath);

        private static string ResolveLink(string ownerPath, Gbx owner, string filePath)
        {
            var logical = PakStore.Logical(ownerPath, owner.RefTable!.AncestorLevel, filePath);
            var link = logical.EndsWith(MaterialExtension, StringComparison.OrdinalIgnoreCase) ? logical[..^MaterialExtension.Length] : logical;
            if (GamePaks.Store.Resolve(link + MaterialExtension) is not null)
            {
                return link;
            }
            var environmentMaterial = $@"{link[..link.IndexOf('\\')]}{EnvironmentMaterials}{link[(link.LastIndexOf('\\') + 1)..]}";
            return GamePaks.Store.Resolve(environmentMaterial + MaterialExtension) is null ? link : environmentMaterial;
        }

        private static bool IsDeck(string link, DeckFamily family) => family.IsDeck(link);

        private static CPlugVisual3D DecalCopy(CPlugVisual3D visual)
        {
            var copy = Detached((CPlugVisual3D)visual.DeepClone());
            var sets = copy.VertexStreams![0].UVs!;
            if (sets.Count > 2)
            {
                sets[0] = [.. sets[sets.Keys.Max() - 1]];
            }
            return copy;
        }

        private static int MaterialIndex(List<CPlugSolid2Model.Material> materials, Dictionary<(string Link, CPlugSurface.MaterialId Physics, CPlugSurface.GameplayId Gameplay), int> indices, string link, CPlugSurface.MaterialId physics, CPlugSurface.GameplayId gameplay)
        {
            var key = (link, physics, gameplay);
            if (!indices.TryGetValue(key, out var index))
            {
                index = materials.Count;
                materials.Add(UserMaterial(link, physics, gameplay));
                indices[key] = index;
            }
            return index;
        }


        private const string GameplayMaterials = @"Effects\Media\Material\Collision";

        private const string EnvironmentMaterials = @"\Media\Material\";

        private static readonly Dictionary<string, (CPlugSurface.MaterialId Physics, CPlugSurface.GameplayId Gameplay)> SurfaceByLink = new(StringComparer.OrdinalIgnoreCase);

        private static (CPlugSurface.MaterialId Physics, CPlugSurface.GameplayId Gameplay) SurfaceOf(string link)
        {
            if (!SurfaceByLink.TryGetValue(link, out var surface))
            {
                surface = link.StartsWith(GameplayMaterials, StringComparison.OrdinalIgnoreCase)
                    ? CollisionMaterial(link[GameplayMaterials.Length..])
                    : GamePaks.Store.Open(link + MaterialExtension)?.Node is CPlugMaterial material
                        ? (material.SurfaceId, material.GameplayId)
                        : OutsideTheEnvironmentPaks;
                SurfaceByLink[link] = surface;
            }
            return surface;
        }

        private static readonly (CPlugSurface.MaterialId Physics, CPlugSurface.GameplayId Gameplay) OutsideTheEnvironmentPaks = (CPlugSurface.MaterialId.Asphalt, CPlugSurface.GameplayId.None);

        private static (CPlugSurface.MaterialId Physics, CPlugSurface.GameplayId Gameplay) CollisionMaterial(string name)
        {
            var gameplay = MaterialDressing.GameplayOf(name);
            var surface = gameplay == CPlugSurface.GameplayId.None ? name : name[gameplay.ToString().Length..];
            return (Enum.TryParse<CPlugSurface.MaterialId>(surface, out var physics) ? physics : CPlugSurface.MaterialId.Asphalt, gameplay);
        }

        private static CPlugSurface.MaterialId PhysicsOf(string link) => SurfaceOf(link).Physics;

        private static CPlugSolid2Model.Material UserMaterial(string link, CPlugSurface.MaterialId physics, CPlugSurface.GameplayId gameplay)
        {
            var instance = new CPlugMaterialUserInst();
            instance.CreateChunk<CPlugMaterialUserInst.Chunk090FD000>().Version = 11;
            instance.CreateChunk<CPlugMaterialUserInst.Chunk090FD001>().Version = 5;
            instance.CreateChunk<CPlugMaterialUserInst.Chunk090FD002>();
            instance.IsUsingGameMaterial = true;
            instance.MaterialName = "";
            instance.Model = "";
            instance.BaseTexture = "";
            instance.Link = link;
            instance.SurfacePhysicId = physics;
            instance.SurfaceGameplayId = gameplay;
            instance.TextureSizeInMeters = DefaultTextureSizeInMeters;
            instance.Csts = [];
            instance.Color = [];
            instance.UvAnims = [];
            instance.UserTextures = [];
            instance.HidingGroup = "";
            return new CPlugSolid2Model.Material { MaterialName = "", MaterialUserInst = instance };
        }
    }
}
