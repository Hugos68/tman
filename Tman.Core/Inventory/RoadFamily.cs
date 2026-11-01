using System;
using System.Collections.Generic;
using System.Linq;

namespace Tman.Core.Inventory
{
    public sealed record RoadFamily(string Prefix, IReadOnlySet<string> DeckMaterials, Surface? NativeSurface)
    {
        private const string Materials = @"Stadium\Media\Material\";

        private const string RoadTech = Materials + "RoadTech";

        private const string RoadDirt = Materials + "RoadDirt";

        private const string RoadIce = Materials + "RoadIce";

        private const string RoadBump = Materials + "RoadBump";

        private const string PlatformTech = Materials + "PlatformTech";

        private const string SnowRoad = Materials + "ThemeSnowRoad";

        private const string Pillar = "Pillar";

        public static readonly IReadOnlyList<RoadFamily> All =
        [
            new("RoadTech", Decks(RoadTech, SnowRoad), Surface.Road),
            new("RoadDirt", Decks(RoadDirt), Surface.Dirt),
            new("RoadIce", Decks(RoadIce), Surface.Ice),
            new("RoadBump", Decks(RoadBump), null),
            new("SnowRoad", Decks(SnowRoad), null),
            new("RallyRoadDirt", Decks(RoadDirt), Surface.Dirt),
            new("OpenTech", Decks(PlatformTech), Surface.Road),
            new("OpenDirt", Decks(PlatformTech), Surface.Dirt),
            new("OpenGrass", Decks(PlatformTech), Surface.Grass),
            new("OpenIce", Decks(PlatformTech), Surface.Ice),
        ];

        public static RoadFamily? Of(Block block) => block.Id.Contains(Pillar, StringComparison.Ordinal)
            ? null
            : All.Where(family => block.Id.StartsWith(family.Prefix, StringComparison.Ordinal)).OrderByDescending(family => family.Prefix.Length).FirstOrDefault();

        public bool IsDeck(string link) => DeckMaterials.Contains(link);

        private static IReadOnlySet<string> Decks(params string[] links) => new HashSet<string>(links, StringComparer.OrdinalIgnoreCase);
    }
}
