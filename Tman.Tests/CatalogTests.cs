using Tman.Core.Catalog;
using Xunit;

namespace Tman.Tests
{
    public class CatalogTests
    {
        [Theory]
        [InlineData(0x00000000, 0, 0)]
        [InlineData(0x00001000, 0, 0)]
        [InlineData(0x00200000, 0, 1)]
        [InlineData(0x00400003, 3, 2)]
        [InlineData(0x0FE0003F, 63, 127)]
        public void BlockFlagsSplitIntoBaseAndAdditionalVariant(int flags, int baseVariant, int additionalVariant)
        {
            Assert.Equal(baseVariant, BlockFlags.BaseVariant(flags));
            Assert.Equal(additionalVariant, BlockFlags.AdditionalVariant(flags));
            Assert.Equal(baseVariant + BlockFlags.VariantsPerAdditionalVariant * additionalVariant, BlockFlags.CombinedVariant(flags));
        }

        [Fact]
        public void CollectionsHaveTheGamesNumbers()
        {
            Assert.Equal(26, CollectionIds.Of(Collections.Stadium).Number);
            Assert.Equal(16, CollectionIds.Of(Collections.RedIsland).Number);
            Assert.Equal(28, CollectionIds.Of(Collections.BlueBay).Number);
        }

        [Fact]
        public void CustomItemsCarryTheirHeaderIdentAndGameItemsDoNot()
        {
            Assert.Null(Items.Flag16m.Ident);
            var ident = Custom.CheckpointExpandableBottom.Ident!;
            Assert.Equal(Custom.CheckpointExpandableBottom.Id, ident.Id);
            Assert.Equal("Stadium2020", ident.Collection.ToString());
            Assert.Equal("BlueBay", Custom.CheckpointExpandableBottomBlueBay.Ident!.Collection.ToString());
        }

        [Fact]
        public void ACustomItemHasATwinPerEnvironment()
        {
            Assert.Equal(Custom.CheckpointExpandableBottomBlueBay, Custom.CheckpointExpandableBottom.For(Collections.BlueBay));
            Assert.Equal(Custom.CheckpointExpandableBottom, Custom.CheckpointExpandableBottom.For(Collections.Stadium));
            Assert.Equal(Custom.FinishPlaneTemplate, Custom.FinishPlaneTemplate.For(Collections.RedIsland));
        }

        [Fact]
        public void WaypointTwinsAreFoundByFamily()
        {
            Assert.Equal(Blocks.RoadTechCheckpoint, Blocks.RoadTechFinish.As(Waypoint.Checkpoint));
            Assert.Equal(Blocks.RoadTechCheckpoint, Blocks.RoadTechCheckpoint.As(Waypoint.Checkpoint));
            Assert.Equal(Blocks.RoadTechStraight, Blocks.RoadTechCheckpoint.Plain);
            Assert.Null(Blocks.RoadTechStraight.As(Waypoint.Checkpoint));
        }

        [Fact]
        public void NamingTablesComeFromTheGenerator()
        {
            Assert.Equal(Blocks.RoadTechStraight, Blocks.PlainOfPenalty["RoadTechPenalty"]);
            Assert.False(Blocks.PlainOfPenalty.ContainsKey("RoadTechSlopeDownPenaltyHFC"));
            Assert.Equal("Water", Blocks.PlatformFamilyOf["PlatformWaterBase"]);
            Assert.Equal(Blocks.PlatformPlasticSlope2Straight, Blocks.PlatformTwins[("PlatformTechSlope2Straight", "Plastic")]);
            Assert.Equal(Items.GateSpecial16mBoost, Items.SpecialGates[("GateCheckpointCenter16m", "GateSpecialBoost")]);
            Assert.Equal(Custom.CheckpointExpandableBottomBlueBay, Custom.Skinned[("CheckpointExpandableBottom.Item.Gbx", Collections.BlueBay)]);
        }
    }
}
