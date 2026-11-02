using System.Collections.Generic;
using System.Linq;
using Tman.Core.Catalog;

namespace Tman.Core.Surfaces
{
    internal static class SurfaceRules
    {
        public static Block? PlainOfPenalty(Block block) => Blocks.PlainOfPenalty.GetValueOrDefault(block.Id);

        public static Block? PlatformTwin(Block block, Surface surface) =>
            Blocks.PlatformFamilyOf.TryGetValue(block.Id, out var family) && family != surface.PlatformFamily && Surface.All.Any(other => other.PlatformFamily == family)
                ? Blocks.PlatformTwins.GetValueOrDefault((block.Id, surface.PlatformFamily))
                : null;

        public static SurfaceBlock? SurfaceBlockOf(Block block, Surface surface) =>
            DeckFamily.Of(block) is { } family && family.NativeSurface != surface ? new SurfaceBlock(surface, block) : null;
    }
}
