using System;
using GBX.NET;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Tman.Core.Maps
{
    public class Position(Vec3? coords, Vec3? yawPitchRoll)
    {
        public Vec3 Coords { get; set; } = coords ?? Vec3.Zero;
        public Vec3 YawPitchRoll { get; set; } = yawPitchRoll ?? Vec3.Zero;

        public static Position Zero => new(Vec3.Zero, Vec3.Zero);

        public Position(Vec3? coords) : this(coords, Vec3.Zero) { }

        public Position AddPosition(Position position)
        {
            Move(position.Coords);
            Rotate(position.YawPitchRoll);
            return this;
        }

        public void Move(Vec3 offset)
        {
            var yaw = YawPitchRoll.X;
            var roll = YawPitchRoll.Z;
            var pitch = YawPitchRoll.Y;

            var x = (double)offset.X;
            var y = (double)offset.Y;
            var z = (double)offset.Z;

            var z1 = z * Math.Cos(pitch) + y * Math.Sin(pitch);
            var x1 = x;
            var y1 = -z * Math.Sin(pitch) + y * Math.Cos(pitch);

            var z2 = z1;
            var x2 = x1 * Math.Cos(roll) - y1 * Math.Sin(roll);
            var y2 = x1 * Math.Sin(roll) + y1 * Math.Cos(roll);

            var z3 = z2 * Math.Cos(yaw) - x2 * Math.Sin(yaw);
            var x3 = z2 * Math.Sin(yaw) + x2 * Math.Cos(yaw);
            var y3 = y2;

            Coords += new Vec3((float)x3, (float)y3, (float)z3);
        }

        public void Rotate(Vec3 rotation)
        {
            var rotationMatrix = CreateZXYMatrix(YawPitchRoll);
            var byRotationMatrix = CreateZXYMatrix(rotation);

            rotationMatrix *= byRotationMatrix;

            YawPitchRoll = GetEulerZXY(rotationMatrix);
        }

        private static Vec3 GetEulerZXY(Matrix<double> rotationMatrix)
        {
            var roll = Math.Asin(rotationMatrix[2, 1]);
            var pitch = Math.Atan2(
                -rotationMatrix[2, 0],
                rotationMatrix[2, 2]);
            var yaw = Math.Atan2(
                -rotationMatrix[0, 1],
                rotationMatrix[1, 1]);

            return new Vec3((float)yaw, (float)pitch, (float)roll);
        }

        private static Matrix<double> CreateZXYMatrix(Vec3 rotation)
        {
            var yaw = rotation.X;
            var roll = rotation.Z;
            var pitch = rotation.Y;

            var rotationZ = DenseMatrix.OfArray(new double[,]
            {
                { Math.Cos(yaw), -Math.Sin(yaw), 0 },
                { Math.Sin(yaw), Math.Cos(yaw), 0 },
                { 0, 0, 1 }
            });

            var rotationX = DenseMatrix.OfArray(new double[,]
            {
                { 1, 0, 0 },
                { 0, Math.Cos(roll), -Math.Sin(roll) },
                { 0, Math.Sin(roll), Math.Cos(roll) }
            });

            var rotationY = DenseMatrix.OfArray(new double[,]
            {
                { Math.Cos(pitch), 0, Math.Sin(pitch) },
                { 0, 1, 0 },
                { -Math.Sin(pitch), 0, Math.Cos(pitch) }
            });

            return rotationZ * rotationX * rotationY;
        }
    }
}