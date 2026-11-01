using Tman.Core.Inventory;
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
            Assert.NotNull(SurfaceRules.SurfaceBlockOf(Blocks.RoadBumpStraight, Surface.Road));
            Assert.Null(SurfaceRules.SurfaceBlockOf(Blocks.RoadDirtStraight, Surface.Dirt));
            Assert.Null(SurfaceRules.SurfaceBlockOf(Blocks.OpenTechRoadStraight, Surface.Road));
            Assert.Null(SurfaceRules.SurfaceBlockOf(Blocks.PlatformTechBase, Surface.Plastic));
        }

        [Fact]
        public void RoadFamiliesNameTheirDeckMaterial()
        {
            Assert.Equal(@"Stadium\Media\Material\RoadTech", RoadFamily.Of(Blocks.RoadTechTiltCurve4)!.DeckMaterial);
            Assert.Equal(@"Stadium\Media\Material\RoadIce", RoadFamily.Of(Blocks.RoadIceWithWallStraight)!.DeckMaterial);
            Assert.Equal(@"Stadium\Media\Material\PlatformTech", RoadFamily.Of(Blocks.OpenGrassZoneStraight)!.DeckMaterial);
            Assert.Null(RoadFamily.Of(Blocks.PlatformTechBase));
        }
    }
}
