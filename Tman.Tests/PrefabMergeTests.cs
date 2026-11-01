using System;
using GBX.NET;
using Tman.Core.Inventory.Builders;
using Xunit;

namespace Tman.Tests
{
    public class PrefabMergeTests
    {
        private static readonly Quat HalfTurnAroundY = new(0, 1, 0, 0);

        private static readonly Quat QuarterTurnAroundY = new(0, MathF.Sqrt(0.5f), 0, MathF.Sqrt(0.5f));

        [Fact]
        public void IdentityLeavesAVectorAlone()
        {
            Assert.Equal(new Vec3(1, 2, 3), PrefabMerge.Rotate(new Quat(0, 0, 0, 1), new Vec3(1, 2, 3)));
            Assert.True(PrefabMerge.IsIdentity(Vec3.Zero, new Quat(0, -8.742278E-08f, 0, 1)));
        }

        [Fact]
        public void AHalfTurnAroundYMirrorsXAndZ()
        {
            var turned = PrefabMerge.Rotate(HalfTurnAroundY, new Vec3(1, 2, 3));
            Assert.Equal(-1, turned.X, 4);
            Assert.Equal(2, turned.Y, 4);
            Assert.Equal(-3, turned.Z, 4);
        }

        [Fact]
        public void TwoQuarterTurnsMakeAHalfTurn()
        {
            var half = PrefabMerge.Multiply(QuarterTurnAroundY, QuarterTurnAroundY);
            var turned = PrefabMerge.Rotate(half, new Vec3(1, 0, 0));
            Assert.Equal(-1, turned.X, 4);
            Assert.Equal(0, turned.Z, 4);
        }
    }
}
