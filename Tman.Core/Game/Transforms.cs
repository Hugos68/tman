using System;
using GBX.NET;

namespace Tman.Core.Game
{
    internal static class Transforms
    {
        private const float IdentityTolerance = 1e-5f;

        private const float DegreesPerHalfTurn = 180f;

        private static readonly Vec3 AxisX = new(1, 0, 0);

        private static readonly Vec3 AxisY = new(0, 1, 0);

        private static readonly Vec3 AxisZ = new(0, 0, 1);

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

        public static Quat FromEulerDegrees(Vec3 degrees) =>
            Multiply(AroundAxis(AxisZ, degrees.Z), Multiply(AroundAxis(AxisY, degrees.Y), AroundAxis(AxisX, degrees.X)));

        public static Quat AroundAxis(Vec3 axis, float degrees)
        {
            var half = degrees * MathF.PI / DegreesPerHalfTurn / 2;
            var sine = MathF.Sin(half);
            return new Quat(axis.X * sine, axis.Y * sine, axis.Z * sine, MathF.Cos(half));
        }

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
