using System;
using System.Collections.Generic;
using System.Linq;
using GBX.NET;
using GBX.NET.Engines.MwFoundations;
using GBX.NET.Engines.Plug;
using GBX.NET.Serialization.Chunking;
using Tman.Core.Catalog;
using Tman.Core.Game;
using Tman.Paks;

namespace Tman.Core.Surfaces.Building
{
    internal sealed class SurfaceBlockMesh(PartGeometry geometry, GameMaterials gameMaterials)
    {
        private const string PlaceholderMaterial = @"Stadium\Media\Material\TechnicsTrims";

        private const string EnvironmentMaterialsFolder = @"Media\Material";

        private static readonly string[] TerrainLookMaterials = [@"Stadium\Media\Material\DecoHill", @"Stadium\Media\Material\DecoHill2", @"Stadium\Media\Material\Deco"];

        public CPlugStaticObjectModel Merge(IReadOnlyList<PrefabParts.Part> parts, DeckFamily family, SurfaceTarget target, MaterialDressing dressing)
        {
            var collidable = parts.First(part => geometry.CollisionMesh(part) is not null);
            var model = (CPlugStaticObjectModel)collidable.Model.DeepClone();
            model.Mesh = (CPlugSolid2Model)geometry.Solid(collidable).DeepClone();
            model.Shape = (CPlugSurface)geometry.Shape(collidable)!.DeepClone();
            var solid = model.Mesh;
            var shape = model.Shape;
            var source = (CPlugSurface.Mesh)shape.Surf!;
            var mesh = new CPlugSurface.Mesh { Version = source.Version, OctreeVersion = source.OctreeVersion, GameplayMainDir = source.GameplayMainDir };
            shape.Surf = mesh;
            var visuals = new List<CPlugVisual3D>();
            var islands = new List<IReadOnlyList<CPlugVisual3D>>();
            var geoms = new List<CPlugSolid2Model.ShadedGeom>();
            var merged = new MergedMaterials();
            var vertices = new List<Vec3>();
            var triangles = new List<CPlugSurface.Mesh.Triangle>();
            var surfaceSlots = 0;
            var lodDistances = LodLevels.Merge(parts.Select(part => geometry.Solid(part).LodMaxDistAtFov90 ?? []));

            foreach (var part in parts)
            {
                var partSolid = geometry.Solid(part);
                var partShape = geometry.Shape(part);
                var links = geometry.MaterialLinks(part, partSolid, partShape);
                var materialMap = new int[links.Length];
                var overlayMap = new int?[links.Length];
                for (var slot = 0; slot < links.Length; slot++)
                {
                    var deck = family.IsDeck(links[slot]);
                    var overlay = family.DeckOverlay(links[slot]);
                    var strip = GameMaterials.IsGameplayCollision(links[slot]);
                    var border = DeckFamily.IsOpenBorder(links[slot]);
                    var link = deck || overlay is not null ? target.Link : border ? OpenBorder(target) : dressing.Dress(links[slot]);
                    var (physics, gameplay) = strip ? (target.Physics, dressing.StripGameplay(links[slot])) : gameMaterials.SurfaceOf(link);
                    materialMap[slot] = deck || overlay is not null || border ? merged.IndexOf(link, target.Physics, gameplay) : merged.IndexOf(link, physics, gameplay);
                    if (overlay is not null && dressing.OverlayTwin(overlay) is { } twin)
                    {
                        overlayMap[slot] = merged.IndexOf(twin, CPlugSurface.MaterialId.NotCollidable, CPlugSurface.GameplayId.None);
                    }
                }
                var visualBase = visuals.Count;
                var partVisuals = new List<CPlugVisual3D>();
                foreach (var visual in partSolid.Visuals ?? [])
                {
                    var copy = VisualTransform.Copy((CPlugVisual3D)visual, part.Position, part.Rotation);
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
                if (geometry.CollisionMesh(part) is not { } partMesh)
                {
                    continue;
                }
                var slots = ShapeSlots(part, partShape!, family, target, dressing);
                var vertexBase = vertices.Count;
                var surfaceBase = surfaceSlots;
                surfaceSlots += slots.Length;
                vertices.AddRange(partMesh.Vertices!.Select(vertex => Transforms.Rotate(part.Rotation, vertex) + part.Position));
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
                solid.PreLightGenerator = PreLight(lightmap, parts.Max(part => geometry.Solid(part).PreLightGenerator?.U02 ?? 0), layout);
            }
            solid.Visuals = [.. visuals];
            solid.ShadedGeoms = [.. geoms];
            solid.CustomMaterials = merged.ToArray();
            solid.Materials = null;
            solid.MaterialIds = [];
            solid.MaterialInsts = null;
            solid.Lights = [];
            solid.LightUserModels = [];
            solid.LightInsts = [];
            solid.FileWriteTime = null;
            solid.Boxes = [VisualTransform.Bounds(vertices.Concat(visuals.SelectMany(visual => visual.VertexStreams![0].Positions!)))];
            mesh.Vertices = [.. vertices];
            mesh.Triangles = [.. triangles];
            shape.Materials = [];
            return model;
        }

        public CPlugStaticObjectModel? Terrain(IReadOnlyList<PrefabParts.Part> parts, DeckFamily family, MaterialDressing dressing, Collections collection)
        {
            if (Collision(parts, dressing) is not { Shape: not null } collision)
            {
                return null;
            }
            var looks = TerrainLookMaterials.Select(dressing.Dress).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var environment = PakPath.Join(collection.ToString(), EnvironmentMaterialsFolder) + PakPath.Separator;
            var model = Merge(parts, family, SurfaceTarget.Of(family.NativeSurface ?? Surface.Road, family), dressing);
            var solid = model.Mesh!;
            var materials = solid.CustomMaterials ?? [];
            solid.ShadedGeoms = [.. (solid.ShadedGeoms ?? []).Where(geom => materials.ElementAtOrDefault(geom.MaterialIndex)?.MaterialUserInst?.Link is { } link
                && (looks.Contains(link) || (collection != Collections.Stadium && link.StartsWith(environment, StringComparison.OrdinalIgnoreCase))))];
            model.Shape = collision.Shape;
            return solid.ShadedGeoms.Length == 0 ? collision : model;
        }

        public CPlugStaticObjectModel? Collision(IReadOnlyList<PrefabParts.Part> parts, MaterialDressing? dressing)
        {
            if (parts.FirstOrDefault(part => geometry.Solid(part).ShadedGeoms is { Length: > 0 }) is not { } shown)
            {
                return null;
            }
            var collidable = parts.FirstOrDefault(part => geometry.CollisionMesh(part) is not null);
            var model = (CPlugStaticObjectModel)(collidable ?? parts[0]).Model.DeepClone();
            model.Mesh = BodyOnly((CPlugSolid2Model)geometry.Solid(collidable ?? parts[0]).DeepClone());
            model.Shape = collidable is null ? null : BodyOnly((CPlugSurface)geometry.Shape(collidable)!.DeepClone());
            model.MeshFile = null;
            model.ShapeFile = null;
            var vertices = new List<Vec3>();
            var triangles = new List<CPlugSurface.Mesh.Triangle>();
            var surfaceSlots = 0;
            foreach (var part in parts)
            {
                if (geometry.CollisionMesh(part) is not { } partMesh)
                {
                    continue;
                }
                var slots = dressing is null ? null : DressedSlots(part, geometry.Shape(part)!, dressing);
                var vertexBase = vertices.Count;
                var surfaceBase = surfaceSlots;
                surfaceSlots += geometry.Shape(part)!.Materials!.Length;
                vertices.AddRange(partMesh.Vertices!.Select(vertex => Transforms.Rotate(part.Rotation, vertex) + part.Position));
                triangles.AddRange(partMesh.Triangles!.Select(triangle => new CPlugSurface.Mesh.Triangle
                {
                    Indices = triangle.Indices + new Int3(vertexBase, vertexBase, vertexBase),
                    SurfaceIndex = (short)(triangle.SurfaceIndex + surfaceBase),
                    U02 = slots?.ElementAtOrDefault(triangle.SurfaceIndex)?.Physics is { } physics ? (byte)physics : triangle.U02,
                    U03 = slots?.ElementAtOrDefault(triangle.SurfaceIndex)?.Gameplay is { } gameplay ? (byte)gameplay : triangle.U03
                }));
            }
            if (model.Shape is { Surf: CPlugSurface.Mesh source })
            {
                model.Shape.Surf = new CPlugSurface.Mesh { Version = source.Version, OctreeVersion = source.OctreeVersion, GameplayMainDir = source.GameplayMainDir, Vertices = [.. vertices], Triangles = [.. triangles] };
                model.Shape.Materials = [];
            }
            Hide(model.Mesh, geometry.Solid(shown), shown.Position, shown.Rotation, vertices);
            return model;
        }

        public CPlugSolid2Model InvisibleSolid(CPlugSolid2Model source, IReadOnlyList<Vec3> extent)
        {
            var solid = BodyOnly((CPlugSolid2Model)source.DeepClone());
            Hide(solid, source, Vec3.Zero, Quat.Identity, extent);
            return solid;
        }

        private static void Hide(CPlugSolid2Model solid, CPlugSolid2Model source, Vec3 position, Quat rotation, IReadOnlyList<Vec3> extent)
        {
            var (placeholder, geom, material) = Placeholder(source, position, rotation, extent);
            solid.Visuals = [placeholder];
            solid.ShadedGeoms = [geom];
            solid.CustomMaterials = [material];
            solid.Materials = null;
            solid.MaterialIds = [];
            solid.MaterialInsts = null;
            solid.Lights = [];
            solid.LightUserModels = [];
            solid.LightInsts = [];
            solid.PreLightGenerator = null;
            solid.FileWriteTime = null;
            solid.Boxes = extent.Count == 0 ? solid.Boxes : [VisualTransform.Bounds(extent)];
        }

        private static (CPlugVisual3D Visual, CPlugSolid2Model.ShadedGeom Geom, CPlugSolid2Model.Material Material) Placeholder(CPlugSolid2Model source, Vec3 position, Quat rotation, IReadOnlyList<Vec3> collision)
        {
            var geom = source.ShadedGeoms![0];
            var visual = VisualTransform.Copy((CPlugVisual3D)source.Visuals![geom.VisualIndex], position, rotation);
            IReadOnlyList<Vec3> extent = collision.Count > 0 ? collision : visual.VertexStreams![0].Positions!;
            var min = new Vec3(extent.Min(point => point.X), extent.Min(point => point.Y), extent.Min(point => point.Z));
            var max = new Vec3(extent.Max(point => point.X), extent.Max(point => point.Y), extent.Max(point => point.Z));
            foreach (var stream in visual.VertexStreams ?? [])
            {
                stream.Positions = [.. stream.Positions!.Select((_, index) => index == 1 ? max : min)];
            }
            if (visual is CPlugVisualIndexed { IndexBuffer: { Indices: { } indices } buffer } indexed)
            {
                var hidden = (CPlugIndexBuffer)buffer.DeepClone();
                hidden.Indices = new int[indices.Length];
                indexed.IndexBuffer = hidden;
            }
            visual.BoundingBox = VisualTransform.Bounds(extent);
            var merged = new MergedMaterials();
            merged.IndexOf(PlaceholderMaterial, CPlugSurface.MaterialId.NotCollidable, CPlugSurface.GameplayId.None);
            return (visual, new CPlugSolid2Model.ShadedGeom { VisualIndex = 0, MaterialIndex = 0, U01 = geom.U01, LodMask = geom.LodMask, U02 = geom.U02 }, merged.ToArray()[0]);
        }

        private static T BodyOnly<T>(T node) where T : CMwNod
        {
            foreach (var chunk in node.Chunks.OfType<IHeaderChunk>().ToList())
            {
                node.Chunks.Remove(chunk);
            }
            return node;
        }

        public CPlugSurface InlineSurface(CPlugSurface surface, Gbx owner, string ownerPath, MaterialDressing dressing, CPlugMaterial template)
        {
            var copy = BodyOnly((CPlugSurface)surface.DeepClone());
            var slots = (surface.Materials ?? []).Select(material => material.MaterialFile is { } file ? Slot(gameMaterials.Link(ownerPath, owner, file.FilePath), dressing) : new DressedSlot(null, null)).ToArray();
            if (copy.Surf is not CPlugSurface.Mesh source)
            {
                copy.Materials = [.. (surface.Materials ?? []).Select((material, index) =>
                {
                    var (physics, gameplay) = material.MaterialFile is { } file ? gameMaterials.SurfaceOf(gameMaterials.Link(ownerPath, owner, file.FilePath)) : (material.Material?.SurfaceId ?? material.SurfaceId ?? CPlugSurface.MaterialId.Concrete, material.Material?.GameplayId ?? CPlugSurface.GameplayId.None);
                    var inline = (CPlugMaterial)template.DeepClone();
                    inline.SurfaceId = slots[index].Physics ?? physics;
                    inline.GameplayId = slots[index].Gameplay ?? gameplay;
                    return new CPlugSurface.SurfMaterial { Material = inline };
                })];
                return copy;
            }
            var surfaces = source.Triangles!.Select(triangle => (
                Triangle: triangle,
                Physics: slots.ElementAtOrDefault(triangle.SurfaceIndex)?.Physics is { } physics ? (byte)physics : triangle.U02,
                Gameplay: slots.ElementAtOrDefault(triangle.SurfaceIndex)?.Gameplay is { } gameplay ? (byte)gameplay : triangle.U03)).ToList();
            var kinds = surfaces.Select(surface => (surface.Physics, surface.Gameplay)).Distinct().ToList();
            copy.Surf = new CPlugSurface.Mesh
            {
                Version = source.Version,
                OctreeVersion = source.OctreeVersion,
                GameplayMainDir = source.GameplayMainDir,
                Vertices = source.Vertices,
                Triangles = [.. surfaces.Select(surface => new CPlugSurface.Mesh.Triangle
                {
                    Indices = surface.Triangle.Indices,
                    SurfaceIndex = (short)kinds.IndexOf((surface.Physics, surface.Gameplay)),
                    U02 = surface.Physics,
                    U03 = surface.Gameplay
                })]
            };
            copy.Materials = [.. kinds.Select(kind =>
            {
                var material = (CPlugMaterial)template.DeepClone();
                material.SurfaceId = (CPlugSurface.MaterialId)kind.Physics;
                material.GameplayId = (CPlugSurface.GameplayId)kind.Gameplay;
                return new CPlugSurface.SurfMaterial { Material = material };
            })];
            return copy;
        }

        public CPlugSurface TriggerShape(CPlugSurface surface, Gbx owner, string ownerPath)
        {
            var copy = (CPlugSurface)surface.DeepClone();
            copy.Materials = [.. (surface.Materials ?? []).Select(material => new CPlugSurface.SurfMaterial
            {
                SurfaceId = material.MaterialFile is { } file
                    ? gameMaterials.PhysicsOf(gameMaterials.Link(ownerPath, owner, file.FilePath))
                    : material.Material?.SurfaceId ?? material.SurfaceId ?? CPlugSurface.MaterialId.Asphalt
            })];
            return copy;
        }

        private string OpenBorder(SurfaceTarget target)
        {
            var twin = DeckFamily.OpenBorderOf(target.Link);
            return gameMaterials.Exists(twin) ? twin : target.Link;
        }

        private DressedSlot[] DressedSlots(PrefabParts.Part part, CPlugSurface shape, MaterialDressing dressing) =>
            [.. shape.Materials!.Select(material => material.MaterialFile is { } file ? Slot(geometry.Link(part, file.FilePath), dressing) : new DressedSlot(null, null))];

        private DressedSlot Slot(string link, MaterialDressing dressing)
        {
            if (GameMaterials.IsGameplayCollision(link))
            {
                return new DressedSlot(null, dressing.StripGameplay(link));
            }
            var dressed = dressing.Dress(link);
            if (string.Equals(dressed, link, System.StringComparison.OrdinalIgnoreCase))
            {
                return new DressedSlot(null, null);
            }
            var (physics, gameplay) = gameMaterials.SurfaceOf(dressed);
            return new DressedSlot(physics, gameplay);
        }

        private sealed record DressedSlot(CPlugSurface.MaterialId? Physics, CPlugSurface.GameplayId? Gameplay);

        private SlotTreatment[] ShapeSlots(PrefabParts.Part part, CPlugSurface shape, DeckFamily family, SurfaceTarget target, MaterialDressing dressing) =>
            [.. shape.Materials!.Select(material =>
            {
                if (material.MaterialFile is { } file)
                {
                    var link = geometry.Link(part, file.FilePath);
                    return GameMaterials.IsGameplayCollision(link) ? new SlotTreatment(true, target.Physics, dressing.StripGameplay(link))
                        : family.IsDeck(link) || family.DeckOverlay(link) is not null || DeckFamily.IsOpenBorder(link) ? new SlotTreatment(true, target.Physics, null)
                        : new SlotTreatment(false, gameMaterials.PhysicsOf(dressing.Dress(link)), null);
                }
                return new SlotTreatment(false, material.Material?.SurfaceId ?? material.SurfaceId ?? CPlugSurface.MaterialId.Asphalt, null);
            })];

        private static CPlugSolid2Model.PreLightGen PreLight(CPlugSolid2Model.PreLightGen source, float metersPerUv, LightmapAtlas.Layout layout) => new()
        {
            Version = source.Version,
            U01 = source.U01,
            U02 = metersPerUv,
            U03 = source.U03,
            U04 = source.U04,
            U05 = source.U05,
            U06 = layout.Max.X,
            U07 = layout.Max.Y,
            U08 = source.U08,
            U09 = source.U09,
            U10 = source.U10,
            U11 = source.U11,
            SpriteCount = source.SpriteCount,
            U12 = source.U12,
            UvGroups = source.UvGroups
        };

        private static CPlugVisual3D DecalCopy(CPlugVisual3D visual)
        {
            var copy = VisualTransform.Detached((CPlugVisual3D)visual.DeepClone());
            var sets = copy.VertexStreams![0].UVs!;
            if (sets.Count > 2)
            {
                sets[0] = [.. sets[sets.Keys.Max() - 1]];
            }
            return copy;
        }
    }
}
