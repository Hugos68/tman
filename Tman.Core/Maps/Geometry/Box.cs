using GBX.NET;

namespace Tman.Core.Maps.Geometry
{
    internal readonly record struct Box(float X0, float X1, float Z0, float Z1, float Y0, float Y1)
    {
        public Vec3[] Corners() =>
        [
            new Vec3(X0, Y0, Z0), new Vec3(X1, Y0, Z0), new Vec3(X1, Y1, Z0), new Vec3(X0, Y1, Z0),
            new Vec3(X0, Y0, Z1), new Vec3(X1, Y0, Z1), new Vec3(X1, Y1, Z1), new Vec3(X0, Y1, Z1),
        ];
    }
}
