using System;
using GBX.NET;
using Tman.Core.Game;
using Xunit;

namespace Tman.Tests
{
    public class TransformsTests
    {
        private static readonly Quat HalfTurnAroundY = new(0, 1, 0, 0);

        private static readonly Quat QuarterTurnAroundY = new(0, MathF.Sqrt(0.5f), 0, MathF.Sqrt(0.5f));

        [Fact]
        public void IdentityLeavesAVectorAlone()
        {
            Assert.Equal(new Vec3(1, 2, 3), Transforms.Rotate(Quat.Identity, new Vec3(1, 2, 3)));
            Assert.True(Transforms.IsIdentity(Vec3.Zero, new Quat(0, -8.742278E-08f, 0, 1)));
        }

        [Fact]
        public void AHalfTurnAroundYMirrorsXAndZ()
        {
            AssertVector(new Vec3(-1, 2, -3), Transforms.Rotate(HalfTurnAroundY, new Vec3(1, 2, 3)));
        }

        [Fact]
        public void TwoQuarterTurnsMakeAHalfTurn()
        {
            AssertVector(new Vec3(-1, 0, 0), Transforms.Rotate(Transforms.Multiply(QuarterTurnAroundY, QuarterTurnAroundY), new Vec3(1, 0, 0)));
        }

        [Fact]
        public void EulerDegreesAroundOneAxisMatchTheQuaternion()
        {
            AssertVector(new Vec3(-1, 2, -3), Transforms.Rotate(Transforms.FromEulerDegrees(new Vec3(0, 180, 0)), new Vec3(1, 2, 3)));
            AssertVector(Transforms.Rotate(QuarterTurnAroundY, new Vec3(1, 0, 0)), Transforms.Rotate(Transforms.FromEulerDegrees(new Vec3(0, 90, 0)), new Vec3(1, 0, 0)));
        }

        [Fact]
        public void EulerDegreesApplyXThenYThenZ()
        {
            var xFirst = Transforms.Rotate(Transforms.FromEulerDegrees(new Vec3(-90, 0, 90)), new Vec3(0, 1, 0));
            var xOnly = Transforms.Rotate(Transforms.FromEulerDegrees(new Vec3(-90, 0, 0)), new Vec3(0, 1, 0));
            AssertVector(xOnly, xFirst);
            Assert.Equal(0, xFirst.Y, 4);
        }

        private static void AssertVector(Vec3 expected, Vec3 actual)
        {
            Assert.Equal(expected.X, actual.X, 4);
            Assert.Equal(expected.Y, actual.Y, 4);
            Assert.Equal(expected.Z, actual.Z, 4);
        }
    }
}
