using GBX.NET.Engines.Plug;
using Tman.Core.Inventory;
using Xunit;

namespace Tman.Tests
{
    public class SurfaceTargetTests
    {
        [Fact]
        public void CollectionsHaveTheGamesNumbers()
        {
            Assert.Equal(26, CollectionIds.Of(Collections.Stadium).Number);
            Assert.Equal(16, CollectionIds.Of(Collections.RedIsland).Number);
            Assert.Equal(28, CollectionIds.Of(Collections.BlueBay).Number);
        }

        [Fact]
        public void EverySurfaceHasADeckLinkAndPhysics()
        {
            foreach (var surface in Surface.All)
            {
                var target = SurfaceTarget.Of(surface);
                Assert.EndsWith(@"\PlatformTech", target.Link);
                Assert.NotEqual(CPlugSurface.MaterialId.Concrete, target.Physics);
            }
            Assert.Equal(CPlugSurface.MaterialId.Plastic, SurfaceTarget.Of(Surface.Plastic).Physics);
            Assert.Equal(@"Stadium\Media\Material\PlatformTech", SurfaceTarget.Of(Surface.Road).Link);
        }
    }
}
