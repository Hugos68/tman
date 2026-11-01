using System;
using System.Collections.Generic;
using System.Linq;

namespace Tman.Core.Inventory
{
    public sealed record RoadFamily(string Prefix, string DeckMaterial, Surface? NativeSurface)
    {
        private const string Materials = @"Stadium\Media\Material\";

        public static readonly IReadOnlyList<RoadFamily> All =
        [
            new("RoadTech", Materials + "RoadTech", Surface.Road),
            new("RoadDirt", Materials + "RoadDirt", Surface.Dirt),
            new("RoadIce", Materials + "RoadIce", Surface.Ice),
            new("RoadBump", Materials + "RoadBump", null),
            new("OpenTech", Materials + "PlatformTech", Surface.Road),
            new("OpenDirt", Materials + "PlatformTech", Surface.Dirt),
            new("OpenGrass", Materials + "PlatformTech", Surface.Grass),
            new("OpenIce", Materials + "PlatformTech", Surface.Ice),
        ];

        public static RoadFamily? Of(Block block) =>
            All.Where(family => block.Id.StartsWith(family.Prefix, StringComparison.Ordinal)).OrderByDescending(family => family.Prefix.Length).FirstOrDefault();
    }
}
