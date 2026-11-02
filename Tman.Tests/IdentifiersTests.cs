using Tman.Generator;
using Xunit;

namespace Tman.Tests
{
    public class IdentifiersTests
    {
        [Fact]
        public void GameIdsBecomeValidUniqueIdentifiers()
        {
            var identifiers = new Identifiers();
            Assert.Equal("RoadTechStraight", identifiers.For("RoadTechStraight"));
            Assert.Equal("_12Road", identifiers.For("12Road"));
            Assert.Equal("class_", identifiers.For("class"));
            Assert.Equal("Gate_Special_Turbo", identifiers.For("Gate-Special Turbo"));
            Assert.Equal("Gate_Special_Turbo_2", identifiers.For("Gate.Special.Turbo"));
            Assert.Equal("Gate_Special_Turbo", identifiers.Of("Gate-Special Turbo"));
        }
    }
}
