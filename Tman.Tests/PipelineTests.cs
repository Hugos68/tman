using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Tman.Core.Alterations;
using Tman.Core.Catalog;
using Tman.Core.Maps.Placements;
using Tman.Core.Surfaces;
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
            var renamed = map.Placements.OnlyBlocks().Count(placement => placement.Id.EndsWith("_CustomBlock", StringComparison.Ordinal));
            Assert.Equal(blocks.Requested.Count, renamed);
            Assert.Equal(files.Select(file => file.EmbeddedEntry).Order().ToList(), map.EmbeddedFiles.Where(entry => entry.StartsWith("Blocks/tman/", StringComparison.Ordinal)).Order().ToList());
        }

        [Fact]
        public void InvisibleEmbedsAnInvisibleBlockForEverythingButTheWaypoints()
        {
            var blocks = new FixedSurfaceBlocks();
            using var services = TestServices.Build(services => services.AddSingleton<ISurfaceBlocks>(blocks));
            var map = TestServices.LoadTestMap();
            var waypoints = map.Placements.OnlyBlocks().Where(placement => placement.IsWaypoint).Select(placement => placement.Id).ToList();
            var unclipped = map.Placements.OnlyBlocks().Where(placement => placement.IsWaypoint && placement.BlockModel is { Waypoint: Waypoint.Checkpoint or Waypoint.Finish } model && DeckFamily.Of(model) is not null).Select(placement => placement.Id).ToList();
            var others = map.Placements.OnlyBlocks().Count(placement => !placement.IsWaypoint && placement.BlockModel is not null);
            var items = map.Placements.OnlyItems().Count(placement => !placement.IsWaypoint);
            var baked = map.Baked.Count();
            services.GetRequiredService<AlterationPipeline>().Apply("Invisible", map);
            var invisible = blocks.Requested.Where(file => file.Block.Surface == Surface.Invisible).ToList();
            var kept = map.Placements.OnlyBlocks().Where(placement => placement.IsWaypoint).ToList();
            Assert.NotEmpty(waypoints);
            Assert.NotEmpty(unclipped);
            Assert.True(invisible.Count > others);
            Assert.Equal(others + items + baked - map.Baked.Count(), invisible.Count);
            Assert.All(map.Placements.OnlyBlocks(), placement => Assert.True(placement.IsWaypoint || placement.BlockModel is null));
            Assert.Equal(invisible.Select(file => file.Ident).Order(), map.Placements.OnlyItems().Where(placement => !placement.IsWaypoint).Select(placement => placement.Id).Order());
            Assert.Equal(unclipped.Order(), blocks.Requested.Where(file => file.Block.Surface == Surface.Waypoint).Select(file => file.Block.Archetype.Id).Order());
            Assert.Equal(invisible.Count + unclipped.Count, blocks.Requested.Count);
            Assert.Equal(waypoints.Count, kept.Count);
            Assert.Equal(unclipped.Count, kept.Count(placement => placement.Id.EndsWith("_CustomBlock", StringComparison.Ordinal) && !placement.Block!.IsGround));
            Assert.All(kept, placement => Assert.True(placement.IsFree));
            Assert.EndsWith("Invisible", map.Name);
        }

        [Fact]
        public void InvisibleSeeThroughEmbedsTheSameItemsAsInvisibleButSeeThrough()
        {
            var hidden = new FixedSurfaceBlocks();
            using (var services = TestServices.Build(services => services.AddSingleton<ISurfaceBlocks>(hidden)))
            {
                services.GetRequiredService<AlterationPipeline>().Apply("Invisible", TestServices.LoadTestMap());
            }
            var shown = new FixedSurfaceBlocks();
            using var seeThrough = TestServices.Build(services => services.AddSingleton<ISurfaceBlocks>(shown));
            var map = TestServices.LoadTestMap();
            seeThrough.GetRequiredService<AlterationPipeline>().Apply("InvisibleSeeThrough", map);
            Assert.Equal(hidden.Requested.Select(file => (file.Block.Surface == Surface.Invisible ? Surface.SeeThrough : file.Block.Surface, file.Block.Archetype.Id)), shown.Requested.Select(file => (file.Block.Surface, file.Block.Archetype.Id)));
            Assert.Contains(shown.Requested, file => file.Block.Surface == Surface.SeeThrough);
            Assert.All(shown.Requested.Where(file => file.Block.Surface == Surface.SeeThrough), file => Assert.True(file.Invisible));
            Assert.EndsWith("Invisible See-Through", map.Name);
        }

        [Fact]
        public void PodiumReverseLeavesAMapWithoutAPodiumAlone()
        {
            using var services = TestServices.Build();
            var map = TestServices.LoadTestMap();
            var starts = map.Placements.Where(placement => placement.IsStart).Select(placement => placement.Id).ToList();
            services.GetRequiredService<AlterationPipeline>().Apply("PodiumReverse", map);
            Assert.Equal(starts, map.Placements.Where(placement => placement.IsStart).Select(placement => placement.Id).ToList());
            Assert.EndsWith("Podium Reverse", map.Name);
        }
    }
}
