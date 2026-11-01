using Tman.Core.Inventory;
using Xunit;

namespace Tman.Tests
{
    public class SurfaceBlockFileTests
    {
        private static readonly SurfaceBlock Block = new(Surface.Plastic, Blocks.RoadTechStart);

        [Fact]
        public void StadiumFilesHaveNoEnvironmentSuffix()
        {
            var file = new SurfaceBlockFile(Block, true, 0, Collections.Stadium);
            Assert.Equal("PlasticRoadTechStart_0.Block.Gbx", file.FileName);
            Assert.Equal(@"tman\Plastic\Ground\PlasticRoadTechStart_0.Block.Gbx", file.Ident);
            Assert.Equal(@"tman\Plastic\Ground\PlasticRoadTechStart_0.Block.Gbx_CustomBlock", file.RecordName);
            Assert.Equal("Blocks/tman/Plastic/Ground/PlasticRoadTechStart_0.Block.Gbx", file.EmbeddedEntry);
        }

        [Fact]
        public void VistaFilesCarryTheEnvironmentAndTheAirKey()
        {
            var file = new SurfaceBlockFile(Block, false, 1, Collections.RedIsland);
            Assert.Equal("PlasticRoadTechStart_1001000RedIsland.Block.Gbx", file.FileName);
            Assert.Equal(@"tman\Plastic\Air\PlasticRoadTechStart_1001000RedIsland.Block.Gbx", file.Ident);
        }

        [Fact]
        public void CachePathLiesUnderTheSurfaceFolder()
        {
            var file = new SurfaceBlockFile(Block, false, 0, Collections.BlueBay);
            Assert.Equal(System.IO.Path.Combine("Plastic", "Air", "PlasticRoadTechStart_1000000BlueBay.Block.Gbx"), file.CachePath);
        }
    }
}
