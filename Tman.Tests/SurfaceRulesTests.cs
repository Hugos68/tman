using Tman.Core.Catalog;
using Tman.Core.Surfaces;
using Xunit;

namespace Tman.Tests
{
    public class SurfaceRulesTests
    {
        [Fact]
        public void PenaltyKindsFallBackToTheirPlainStraight()
        {
            Assert.Equal(Blocks.RoadTechSlopeStraight, SurfaceRules.PlainOfPenalty(Blocks.RoadTechSlopePenaltyDirt));
            Assert.Equal(Blocks.RoadTechStraight, SurfaceRules.PlainOfPenalty(Blocks.RoadTechPenalty));
            Assert.Equal(Blocks.RoadDirtTiltStraight, SurfaceRules.PlainOfPenalty(Blocks.RoadDirtTiltPenaltyIce));
        }

        [Fact]
        public void KindsWithoutAPlainStraightAreLeftAlone()
        {
            Assert.Null(SurfaceRules.PlainOfPenalty(Blocks.RoadTechDiagLeftPenalty));
            Assert.Null(SurfaceRules.PlainOfPenalty(Blocks.RoadTechSlopeDownPenaltyHFC));
            Assert.Null(SurfaceRules.PlainOfPenalty(Blocks.RoadTechStraight));
        }

        [Fact]
        public void PlatformsSwitchFamilyAndKeepTheirShape()
        {
            Assert.Equal(Blocks.PlatformPlasticSlope2Straight, SurfaceRules.PlatformTwin(Blocks.PlatformTechSlope2Straight, Surface.Plastic));
            Assert.Equal(Blocks.PlatformDirtCheckpoint, SurfaceRules.PlatformTwin(Blocks.PlatformGrassCheckpoint, Surface.Dirt));
            Assert.Equal(Blocks.PlatformTechBase, SurfaceRules.PlatformTwin(Blocks.PlatformIceBase, Surface.Road));
        }

        [Fact]
        public void PlatformsAlreadyOfTheSurfaceAndOtherBlocksAreLeftAlone()
        {
            Assert.Null(SurfaceRules.PlatformTwin(Blocks.PlatformPlasticBase, Surface.Plastic));
            Assert.Null(SurfaceRules.PlatformTwin(Blocks.PlatformWaterBase, Surface.Plastic));
            Assert.Null(SurfaceRules.PlatformTwin(Blocks.RoadTechStraight, Surface.Plastic));
        }

        [Fact]
        public void RoadKindsGetASurfaceBlockUnlessTheyAlreadyAreThatSurface()
        {
            Assert.Equal(new SurfaceBlock(Surface.Plastic, Blocks.RoadTechStart), SurfaceRules.SurfaceBlockOf(Blocks.RoadTechStart, Surface.Plastic));
            Assert.Equal("PlasticRoadTechStart", SurfaceRules.SurfaceBlockOf(Blocks.RoadTechStart, Surface.Plastic)!.Name);
            Assert.NotNull(SurfaceRules.SurfaceBlockOf(Blocks.OpenDirtRoadCurve1, Surface.Plastic));
            Assert.Null(SurfaceRules.SurfaceBlockOf(Blocks.RoadTechStart, Surface.Road));
            Assert.Null(SurfaceRules.SurfaceBlockOf(Blocks.RoadDirtStraight, Surface.Dirt));
            Assert.Null(SurfaceRules.SurfaceBlockOf(Blocks.DecoWallBasePillar, Surface.Plastic));
        }

        [Fact]
        public void TransitionsBelongToTheFamilyOfTheirDeck()
        {
            Assert.NotNull(DeckFamily.Of(Blocks.RoadTechToRoadBump));
            Assert.Null(DeckFamily.Of(Blocks.DecoWallBasePillar));
        }

        [Theory]
        [InlineData(true, 0, 0)]
        [InlineData(true, 1, 1000)]
        [InlineData(false, 0, 1000000)]
        [InlineData(false, 1, 1001000)]
        [InlineData(false, 3, 1003000)]
        public void TheBlockKeyFollowsTheGamesDecimalGroups(bool ground, int additionalVariant, int key)
        {
            Assert.Equal(key, SurfaceBlockKey.Of(ground, additionalVariant));
        }

        [Fact]
        public void StadiumFilesHaveNoEnvironmentSuffix()
        {
            var file = new SurfaceBlockFile(new SurfaceBlock(Surface.Plastic, Blocks.RoadTechStart), true, 0, Collections.Stadium);
            Assert.Equal("PlasticRoadTechStart_0.Block.Gbx", file.FileName);
            Assert.Equal(@"tman\Plastic\Ground\PlasticRoadTechStart_0.Block.Gbx", file.Ident);
            Assert.Equal(@"tman\Plastic\Ground\PlasticRoadTechStart_0.Block.Gbx_CustomBlock", file.RecordName);
            Assert.Equal("Blocks/tman/Plastic/Ground/PlasticRoadTechStart_0.Block.Gbx", file.EmbeddedEntry);
        }

        [Fact]
        public void VistaFilesCarryTheEnvironmentAndTheAirKey()
        {
            var file = new SurfaceBlockFile(new SurfaceBlock(Surface.Plastic, Blocks.RoadTechStart), false, 1, Collections.RedIsland);
            Assert.Equal("PlasticRoadTechStart_1001000RedIsland.Block.Gbx", file.FileName);
            Assert.Equal(@"tman\Plastic\Air\PlasticRoadTechStart_1001000RedIsland.Block.Gbx", file.Ident);
            Assert.Equal(System.IO.Path.Combine("Plastic", "Air", "PlasticRoadTechStart_1001000RedIsland.Block.Gbx"), file.CachePath);
        }
    }
}
