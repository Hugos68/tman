using System;
using System.Collections.Generic;
using System.Linq;
using GBX.NET;
using GBX.NET.Components;
using GBX.NET.Engines.GameData;
using GBX.NET.Engines.Plug;
using Tman.Paks;

namespace Tman.Generator
{
    internal sealed class SizeWalk(PakStore store)
    {
        private float lx = float.PositiveInfinity, ly = float.PositiveInfinity, lz = float.PositiveInfinity;
        private float hx = float.NegativeInfinity, hy = float.NegativeInfinity, hz = float.NegativeInfinity;
        private bool any;
        private int depth;

        private static readonly string[] Followed = [".PREFAB.GBX", ".STATICOBJECT.GBX", ".DYNAOBJECT.GBX", ".MESH.GBX", ".SOLID2MODEL.GBX", ".SHAPE.GBX", ".HITSHAPE.GBX", ".SOLID.GBX"];

        public static Vec3? Measure(PakStore store, string logical)
        {
            var walk = new SizeWalk(store);
            walk.File(logical, Xform.Identity);
            return walk.any ? new Vec3(walk.hx - walk.lx, walk.hy - walk.ly, walk.hz - walk.lz) : null;
        }

        private void Point(Vec3 p)
        {
            lx = MathF.Min(lx, p.X); ly = MathF.Min(ly, p.Y); lz = MathF.Min(lz, p.Z);
            hx = MathF.Max(hx, p.X); hy = MathF.Max(hy, p.Y); hz = MathF.Max(hz, p.Z);
            any = true;
        }

        private void File(string logical, Xform at)
        {
            if (depth > 16)
            {
                return;
            }
            var gbx = store.Open(logical);
            if (gbx?.Node is null)
            {
                return;
            }
            depth++;
            Node(gbx.Node, new Owner(logical, gbx.RefTable?.AncestorLevel ?? 0), at);
            depth--;
        }

        private readonly record struct Owner(string Logical, int Ancestor);

        private void Slot(object? node, GbxRefTableFile? file, Owner owner, Xform at)
        {
            if (node is not null)
            {
                Node(node, owner, at);
            }
            else if (file is not null)
            {
                var upper = file.FilePath.ToUpperInvariant();
                if (Followed.Any(upper.EndsWith))
                {
                    File(PakStore.Logical(owner.Logical, owner.Ancestor, file.FilePath), at);
                }
            }
        }

        private void Node(object node, Owner owner, Xform at)
        {
            switch (node)
            {
                case CPlugPrefab prefab:
                    foreach (var ent in prefab.Ents)
                    {
                        Slot(ent.Model, ent.ModelFile, owner, at.Compose(Xform.FromQuat(ent.Rotation, ent.Position)));
                    }
                    break;
                case CPlugStaticObjectModel s:
                    Slot(s.Shape, s.ShapeFile, owner, at);
                    Slot(s.Mesh, s.MeshFile, owner, at);
                    break;
                case CPlugDynaObjectModel d:
                    Slot(d.StaticShape, d.StaticShapeFile, owner, at);
                    Slot(d.Mesh, d.MeshFile, owner, at);
                    if (!SameShape(d.DynaShape, d.DynaShapeFile, d.StaticShape, d.StaticShapeFile))
                    {
                        Slot(d.DynaShape, d.DynaShapeFile, owner, at);
                    }
                    break;
                case CPlugSurface surface:
                    Surf(surface.Surf, at);
                    break;
                case CPlugSolid2Model solid:
                    foreach (var visual in solid.Visuals ?? [])
                    {
                        Visual(visual, at);
                    }
                    break;
                case CPlugSolid plugSolid:
                    Slot(plugSolid.Tree, plugSolid.TreeFile, owner, at);
                    break;
                case CPlugTree tree:
                    var m = tree.Location is { } location ? at.Compose(Xform.FromIso(location)) : at;
                    if (tree.Visual is not null)
                    {
                        Visual(tree.Visual, m);
                    }
                    foreach (var child in tree.Children ?? [])
                    {
                        Node(child, owner, m);
                    }
                    break;
                case CGameItemModel item:
                    if (item.EntityModel is not null)
                    {
                        Node(item.EntityModel, owner, at);
                    }
                    break;
                case CGameCommonItemEntityModel entity:
                    if (entity.StaticObject is not null)
                    {
                        Node(entity.StaticObject, owner, at);
                    }
                    break;
            }
        }

        private static bool SameShape(CPlugSurface? a, GbxRefTableFile? af, CPlugSurface? b, GbxRefTableFile? bf)
        {
            if (a is not null || b is not null)
            {
                return ReferenceEquals(a, b);
            }
            if (af is null && bf is null)
            {
                return true;
            }
            return af is not null && bf is not null && af.FilePath == bf.FilePath;
        }

        private void Surf(CPlugSurface.ISurf? surf, Xform at)
        {
            foreach (var p in Flatten(surf))
            {
                Point(at.Apply(p));
            }
        }

        private static IEnumerable<Vec3> Flatten(CPlugSurface.ISurf? surf)
        {
            switch (surf)
            {
                case CPlugSurface.Mesh mesh:
                    foreach (var v in mesh.Vertices ?? [])
                    {
                        yield return v;
                    }
                    break;
                case CPlugSurface.Compound compound:
                    for (var i = 0; i < (compound.Surfs?.Length ?? 0); i++)
                    {
                        var loc = Xform.FromIso(compound.SurfLocs![i]);
                        foreach (var p in Flatten(compound.Surfs![i]))
                        {
                            yield return loc.Apply(p);
                        }
                    }
                    break;
            }
        }

        private void Visual(CPlugVisual? visual, Xform at)
        {
            if (visual is CPlugVisual3D v3)
            {
                foreach (var vertex in v3.Vertices ?? [])
                {
                    Point(at.Apply(vertex.Position));
                }
            }
            foreach (var stream in visual?.VertexStreams ?? [])
            {
                foreach (var p in stream.Positions ?? [])
                {
                    Point(at.Apply(p));
                }
            }
        }
    }
}
