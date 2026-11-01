using System.Collections.Generic;
using System.Linq;
using GBX.NET;
using GBX.NET.Engines.Plug;
using Tman.Core.Game;

namespace Tman.Core.Surfaces.Building
{
    internal static class SurfaceBlockMesh
    {
        public static CPlugStaticObjectModel Merge(IReadOnlyList<PrefabParts.Part> parts, DeckFamily family, SurfaceTarget target, MaterialDressing dressing)
        {
            var collidable = parts.First(part => PartGeometry.CollisionMesh(part) is not null);
            var model = (CPlugStaticObjectModel)collidable.Model.DeepClone();
            model.Mesh = (CPlugSolid2Model)PartGeometry.Solid(collidable).DeepClone();
            model.Shape = (CPlugSurface)PartGeometry.Shape(collidable)!.DeepClone();
            var solid = model.Mesh;
            var shape = model.Shape;
            var source = (CPlugSurface.Mesh)shape.Surf!;
            var mesh = new CPlugSurface.Mesh { Version = source.Version, OctreeVersion = source.OctreeVersion, GameplayMainDir = source.GameplayMainDir };
            shape.Surf = mesh;
            var visuals = new List<CPlugVisual3D>();
            var islands = new List<IReadOnlyList<CPlugVisual3D>>();
            var geoms = new List<CPlugSolid2Model.ShadedGeom>();
            var materials = new MergedMaterials();
            var vertices = new List<Vec3>();
            var triangles = new List<CPlugSurface.Mesh.Triangle>();
            var surfaceSlots = 0;
            var lodDistances = LodLevels.Merge(parts.Select(part => PartGeometry.Solid(part).LodMaxDistAtFov90 ?? []));

            foreach (var part in parts)
            {
                var partSolid = PartGeometry.Solid(part);
                var partShape = PartGeometry.Shape(part);
                var links = PartGeometry.MaterialLinks(part, partSolid, partShape);
                var materialMap = new int[links.Length];
                var overlayMap = new int?[links.Length];
                for (var slot = 0; slot < links.Length; slot++)
                {
                    var deck = family.IsDeck(links[slot]);
                    var overlay = family.DeckOverlay(links[slot]);
                    var strip = GameMaterials.IsGameplayCollision(links[slot]);
                    var link = deck || overlay is not null ? target.Link : dressing.Dress(links[slot]);
                    var (physics, gameplay) = strip ? (target.Physics, dressing.StripGameplay(links[slot])) : GameMaterials.SurfaceOf(link);
                    materialMap[slot] = materials.IndexOf(link, deck || overlay is not null ? target.Physics : physics, gameplay);
                    if (overlay is not null && MaterialDressing.OverlayTwin(overlay) is { } twin)
                    {
                        overlayMap[slot] = materials.IndexOf(twin, CPlugSurface.MaterialId.NotCollidable, CPlugSurface.GameplayId.None);
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
                if (PartGeometry.CollisionMesh(part) is not { } partMesh)
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
                lightmap.U02 = parts.Max(part => PartGeometry.Solid(part).PreLightGenerator?.U02 ?? 0);
                lightmap.U06 = layout.Max.X;
                lightmap.U07 = layout.Max.Y;
            }
            solid.Visuals = [.. visuals];
            solid.ShadedGeoms = [.. geoms];
            solid.CustomMaterials = materials.ToArray();
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

        public static CPlugSurface TriggerShape(CPlugSurface surface, Gbx owner, string ownerPath)
        {
            var copy = (CPlugSurface)surface.DeepClone();
            copy.Materials = [.. (surface.Materials ?? []).Select(material => new CPlugSurface.SurfMaterial
            {
                SurfaceId = material.MaterialFile is { } file
                    ? GameMaterials.PhysicsOf(GameMaterials.Link(ownerPath, owner, file.FilePath))
                    : material.Material?.SurfaceId ?? material.SurfaceId ?? CPlugSurface.MaterialId.Asphalt
            })];
            return copy;
        }

        private static SlotTreatment[] ShapeSlots(PrefabParts.Part part, CPlugSurface shape, DeckFamily family, SurfaceTarget target, MaterialDressing dressing) =>
            [.. shape.Materials!.Select(material =>
            {
                if (material.MaterialFile is { } file)
                {
                    var link = PartGeometry.Link(part, file.FilePath);
                    return GameMaterials.IsGameplayCollision(link) ? new SlotTreatment(true, target.Physics, dressing.StripGameplay(link))
                        : family.IsDeck(link) || family.DeckOverlay(link) is not null ? new SlotTreatment(true, target.Physics, null)
                        : new SlotTreatment(false, GameMaterials.PhysicsOf(dressing.Dress(link)), null);
                }
                return new SlotTreatment(false, material.Material?.SurfaceId ?? material.SurfaceId ?? CPlugSurface.MaterialId.Asphalt, null);
            })];

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
