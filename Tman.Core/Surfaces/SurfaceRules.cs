using System;
using System.Linq;
using Tman.Core.Catalog;

namespace Tman.Core.Surfaces
{
    internal static class SurfaceRules
    {
        private const string Penalty = "Penalty";

        private const string Platform = "Platform";

        public static Block? PlainOfPenalty(Block block)
        {
            var index = block.Id.IndexOf(Penalty, StringComparison.Ordinal);
            if (index < 0 || Array.IndexOf(PenaltySurfaces, block.Id[(index + Penalty.Length)..]) < 0)
            {
                return null;
            }
            return Blocks.FromId(block.Id[..index] + "Straight");
        }

        public static Block? PlatformTwin(Block block, Surface surface)
        {
            if (!block.Id.StartsWith(Platform, StringComparison.Ordinal))
            {
                return null;
            }
            foreach (var family in Surface.All.Select(other => other.PlatformFamily))
            {
                if (family != surface.PlatformFamily && block.Id.StartsWith(Platform + family, StringComparison.Ordinal))
                {
                    return Blocks.FromId(Platform + surface.PlatformFamily + block.Id[(Platform.Length + family.Length)..]);
                }
            }
            return null;
        }

        public static SurfaceBlock? SurfaceBlockOf(Block block, Surface surface) =>
            DeckFamily.Of(block) is { } family && family.NativeSurface != surface ? new SurfaceBlock(surface, block) : null;

        private static readonly string[] PenaltySurfaces = ["", "Dirt", "Ice"];
    }
}
