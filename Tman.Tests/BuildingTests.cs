using GBX.NET.Engines.Plug;
using Tman.Core.Surfaces;
using Tman.Core.Surfaces.Building;
using Xunit;

namespace Tman.Tests
{
    public class BuildingTests
    {
        [Theory]
        [InlineData("Turbo", CPlugSurface.GameplayId.Turbo)]
        [InlineData("TurboRoadSynthetic", CPlugSurface.GameplayId.Turbo)]
        [InlineData("TurboDirt", CPlugSurface.GameplayId.Turbo)]
        [InlineData("Turbo2Grass", CPlugSurface.GameplayId.Turbo2)]
        [InlineData("TurboRoulettePlastic", CPlugSurface.GameplayId.TurboRoulette)]
        [InlineData("Ice", CPlugSurface.GameplayId.None)]
        public void TheGameplayOfACollisionMaterialIsItsLongestGameplayPrefix(string collisionName, CPlugSurface.GameplayId gameplay)
        {
            Assert.Equal(gameplay, GameMaterials.GameplayOf(collisionName));
        }

        [Fact]
        public void MergedLodDistancesAreTheSortedUnion()
        {
            Assert.Equal([64f, 128f, 256f], LodLevels.Merge([[64f, 128f], [], [128f, 256f]]));
        }

        [Fact]
        public void APartWithoutLevelsStaysVisibleAtEveryDistance()
        {
            Assert.Equal(0b1111, LodLevels.Remap(1, [], [64f, 128f, 256f]));
        }

        [Fact]
        public void APartLevelCoversEveryMergedLevelInsideItsRange()
        {
            Assert.Equal(0b0011, LodLevels.Remap(0b01, [128f], [64f, 128f, 256f]));
            Assert.Equal(0b1100, LodLevels.Remap(0b10, [128f], [64f, 128f, 256f]));
            Assert.Equal(0b0100, LodLevels.Remap(0b100, [64f, 128f, 256f], [64f, 128f, 256f]));
        }

        [Fact]
        public void EverySurfaceHasADeckLinkAndPhysics()
        {
            foreach (var surface in Surface.All)
            {
                var target = SurfaceTarget.Of(surface);
                Assert.False(string.IsNullOrEmpty(target.Link));
                Assert.NotEqual(CPlugSurface.MaterialId.Concrete, target.Physics);
            }
            Assert.Equal(CPlugSurface.MaterialId.Plastic, SurfaceTarget.Of(Surface.Plastic).Physics);
            Assert.Equal(CPlugSurface.MaterialId.Green, SurfaceTarget.Of(Surface.Grass).Physics);
            Assert.Equal(CPlugSurface.MaterialId.RoadIce, SurfaceTarget.Of(Surface.Ice).Physics);
            Assert.Equal(@"Stadium\Media\Material\PlatformTech", SurfaceTarget.Of(Surface.Road).Link);
        }
    }
}
