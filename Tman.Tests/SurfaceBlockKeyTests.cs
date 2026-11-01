using Tman.Core.Inventory;
using Xunit;

namespace Tman.Tests
{
    public class SurfaceBlockKeyTests
    {
        [Theory]
        [InlineData(true, 0, 0)]
        [InlineData(true, 1, 1000)]
        [InlineData(false, 0, 1000000)]
        [InlineData(false, 1, 1001000)]
        [InlineData(false, 3, 1003000)]
        public void FollowsTheGamesDecimalGroups(bool ground, int additionalVariant, int key)
        {
            Assert.Equal(key, SurfaceBlockKey.Of(ground, additionalVariant));
        }
    }
}
