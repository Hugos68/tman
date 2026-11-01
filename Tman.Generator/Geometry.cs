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
    internal readonly record struct Xform(float M0, float M1, float M2, float M3, float M4, float M5, float M6, float M7, float M8, float M9, float M10, float M11)
    {
        public static readonly Xform Identity = new(1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0);

        public static Xform FromQuat(Quat q, Vec3 p)
        {
            float x = q.X, y = q.Y, z = q.Z, w = q.W;
            return new Xform(
                2.0f * (w * w + x * x) - 1.0f,
                2.0f * (x * y + w * z),
                2.0f * (x * z - w * y),
                2.0f * (x * y - w * z),
                2.0f * (w * w + y * y) - 1.0f,
                2.0f * (y * z + w * x),
                2.0f * (x * z + w * y),
                2.0f * (y * z - w * x),
                2.0f * (w * w + z * z) - 1.0f,
                p.X, p.Y, p.Z);
        }

        public static Xform FromIso(Iso4 m) => new(m.XX, m.XY, m.XZ, m.YX, m.YY, m.YZ, m.ZX, m.ZY, m.ZZ, m.TX, m.TY, m.TZ);

        public Vec3 Apply(Vec3 v) => new(
            M0 * v.X + M3 * v.Y + M6 * v.Z + M9,
            M1 * v.X + M4 * v.Y + M7 * v.Z + M10,
            M2 * v.X + M5 * v.Y + M8 * v.Z + M11);

        public Xform Compose(Xform inner)
        {
            var c0 = Column(inner.M0, inner.M1, inner.M2);
            var c1 = Column(inner.M3, inner.M4, inner.M5);
            var c2 = Column(inner.M6, inner.M7, inner.M8);
            var t = Apply(new Vec3(inner.M9, inner.M10, inner.M11));
            return new Xform(c0.X, c0.Y, c0.Z, c1.X, c1.Y, c1.Z, c2.X, c2.Y, c2.Z, t.X, t.Y, t.Z);
        }

        private Vec3 Column(float a, float b, float c) => new(
            M0 * a + M3 * b + M6 * c,
            M1 * a + M4 * b + M7 * c,
            M2 * a + M5 * b + M8 * c);
    }

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

