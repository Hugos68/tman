using Tman.Core.Inventory;
using Tman.Core.Maps;
using Xunit;

namespace Tman.Tests
{
    public class SelectorTests
    {
        [Fact]
        public void ABlockSelectorMatchesEveryVariantUnlessOneIsNamed()
        {
            Selector any = Blocks.RoadTechStraight;
            Selector variant = Blocks.RoadTechStraight.Variant(64);
            Assert.Null(any.Variant);
            Assert.Equal(64, variant.Variant);
            Assert.True(any.IsBlock);
            Assert.Equal(Blocks.RoadTechStraight.Id, any.Id);
        }

        [Fact]
        public void AnItemSelectorIsNotABlockSelector()
        {
            Selector item = Items.Flag16m;
            Assert.False(item.IsBlock);
        }
    }
}
