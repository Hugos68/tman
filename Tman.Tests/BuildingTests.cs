using GBX.NET.Engines.Plug;
using Tman.Core.Catalog;
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
        public void EverySurfaceHasADeckLinkAndPhysicsForRoadsAndPlatforms()
        {
            var road = DeckFamily.Of(Blocks.RoadTechStraight)!;
            var platform = DeckFamily.Of(Blocks.PlatformTechBase)!;
            var openRoad = DeckFamily.Of(Blocks.OpenTechRoadStraight)!;
            Assert.False(road.IsPlatform);
            Assert.True(platform.IsPlatform);
            Assert.True(openRoad.IsPlatform);
            foreach (var surface in Surface.All)
            {
                foreach (var family in new[] { road, platform })
                {
                    var target = SurfaceTarget.Of(surface, family);
                    Assert.False(string.IsNullOrEmpty(target.Link));
                    Assert.NotEqual(CPlugSurface.MaterialId.Concrete, target.Physics);
                }
            }
            Assert.Equal(CPlugSurface.MaterialId.Plastic, SurfaceTarget.Of(Surface.Plastic, road).Physics);
            Assert.Equal(CPlugSurface.MaterialId.Green, SurfaceTarget.Of(Surface.Grass, platform).Physics);
            Assert.Equal(@"Stadium\Media\Material\PlatformTech", SurfaceTarget.Of(Surface.Road, road).Link);
        }

        [Fact]
        public void IceIsTheBobsleighDeckOnRoadsAndTheIcePlatformOnPlatformsAndOpenRoads()
        {
            Assert.Equal(@"Stadium\Media\Material\RoadIce", SurfaceTarget.Of(Surface.Ice, DeckFamily.Of(Blocks.RoadTechStraight)!).Link);
            Assert.Equal(@"Stadium\Media\Material\RoadIce", SurfaceTarget.Of(Surface.Ice, DeckFamily.Of(Blocks.RoadDirtStraight)!).Link);
            Assert.Equal(@"Stadium\Media\Modifier\PlatformIce\PlatformTech", SurfaceTarget.Of(Surface.Ice, DeckFamily.Of(Blocks.PlatformTechBase)!).Link);
            Assert.Equal(@"Stadium\Media\Modifier\PlatformIce\PlatformTech", SurfaceTarget.Of(Surface.Ice, DeckFamily.Of(Blocks.OpenTechRoadStraight)!).Link);
            Assert.Equal(CPlugSurface.MaterialId.RoadIce, SurfaceTarget.Of(Surface.Ice, DeckFamily.Of(Blocks.PlatformTechBase)!).Physics);
        }
    }
}
