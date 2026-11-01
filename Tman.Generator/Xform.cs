using GBX.NET;

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
}
