using Tman.Core.Inventory;
using Xunit;

namespace Tman.Tests
{
    public class BlockFlagsTests
    {
        [Theory]
        [InlineData(0x00000000, 0, 0)]
        [InlineData(0x00001000, 0, 0)]
        [InlineData(0x00200000, 0, 1)]
        [InlineData(0x00400003, 3, 2)]
        [InlineData(0x0FE0003F, 63, 127)]
        public void SplitsBaseAndAdditionalVariant(int flags, int baseVariant, int additionalVariant)
        {
            Assert.Equal(baseVariant, BlockFlags.BaseVariant(flags));
            Assert.Equal(additionalVariant, BlockFlags.AdditionalVariant(flags));
            Assert.Equal(baseVariant + 64 * additionalVariant, BlockFlags.CombinedVariant(flags));
        }

        [Fact]
        public void CustomBlockBitIsBit15()
        {
            Assert.Equal(1 << 15, BlockFlags.CustomBlock);
        }
    }
}
