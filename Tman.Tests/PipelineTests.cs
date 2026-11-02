using System;
using System.IO.Compression;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Tman.Core.Alterations;
using Tman.Core.Maps.Placements;
using Tman.Core.Surfaces.Building;
using Tman.Tests.Fakes;
using Xunit;

namespace Tman.Tests
{
    public class PipelineTests
    {
        [Fact]
        public void EveryAlterationResolvesFromTheContainer()
        {
            using var services = TestServices.Build();
            foreach (var type in AlterationPipeline.AlterationTypes)
            {
                Assert.IsType(type, services.GetRequiredService(type));
            }
        }

        [Fact]
        public void AlterationNamesAreUniqueIgnoringCaseAndNameNoStep()
        {
            var names = AlterationPipeline.Names.ToList();
            Assert.Equal(names.Count, names.Distinct(StringComparer.OrdinalIgnoreCase).Count());
            Assert.Contains("Wood", names);
            Assert.Contains("FloorFinish", names);
            Assert.DoesNotContain("BlocksReplaced", names);
            Assert.DoesNotContain("ValidationRemoved", names);
        }

        [Fact]
        public void AnUnknownAlterationIsRefused()
        {
            using var services = TestServices.Build();
            var pipeline = services.GetRequiredService<AlterationPipeline>();
            Assert.Throws<ArgumentException>(() => pipeline.Apply("Nope", TestServices.LoadTestMap()));
        }

        [Fact]
        public void AnEffectAlterationRunsWithoutThePaks()
        {
            using var services = TestServices.Build();
            var map = TestServices.LoadTestMap();
            var name = map.Name;
            services.GetRequiredService<AlterationPipeline>().Apply("freewheel", map);
            Assert.Equal($"{name} Freewheel", map.Name);
        }

        [Fact]
        public void ASurfaceAlterationEmbedsOneBlockPerKindAndRenamesEveryRoadPlacement()
        {
            var blocks = new FixedSurfaceBlocks();
            using var services = TestServices.Build(services => services.AddSingleton<ISurfaceBlocks>(blocks));
            var map = TestServices.LoadTestMap();
            services.GetRequiredService<AlterationPipeline>().Apply("Wood", map);
            var files = blocks.Requested.Distinct().ToList();
            Assert.NotEmpty(files);
            Assert.All(files, file => Assert.Equal("Wood", file.Block.Surface.Name));
            var renamed = map.Placements.OnlyBlocks().Count(placement => placement.Block!.Name.EndsWith("_CustomBlock", StringComparison.Ordinal));
            Assert.Equal(blocks.Requested.Count, renamed);
            using var zip = map.Challenge.OpenReadEmbeddedZipData();
            Assert.Equal(files.Select(file => file.EmbeddedEntry).Order().ToList(), zip.Entries.Select(entry => entry.FullName).Where(entry => entry.StartsWith("Blocks/tman/", StringComparison.Ordinal)).Order().ToList());
        }
    }
}
