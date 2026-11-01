using System;
using GBX.NET;

namespace Tman.Core.Game
{
    internal static class Transforms
    {
        private const float IdentityTolerance = 1e-5f;

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
            position == Vec3.Zero
            && MathF.Abs(rotation.X) < IdentityTolerance
            && MathF.Abs(rotation.Y) < IdentityTolerance
            && MathF.Abs(rotation.Z) < IdentityTolerance
            && MathF.Abs(MathF.Abs(rotation.W) - 1) < IdentityTolerance;

        private static float Dot(Vec3 a, Vec3 b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;

        private static Vec3 Cross(Vec3 a, Vec3 b) => new(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);
    }
}
