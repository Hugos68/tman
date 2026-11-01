using System;
using System.Collections.Generic;
using System.Linq;

namespace Tman.Core.Inventory
{
    public sealed record DeckFamily(string Prefix, IReadOnlySet<string> DeckMaterials, Surface? NativeSurface)
    {
        private const string Materials = @"Stadium\Media\Material\";

        private const string RoadTech = Materials + "RoadTech";

        private const string RoadDirt = Materials + "RoadDirt";

        private const string RoadIce = Materials + "RoadIce";

        private const string RoadBump = Materials + "RoadBump";

        private const string PlatformTech = Materials + "PlatformTech";

        private const string SnowRoad = Materials + "ThemeSnowRoad";

        private const string Pillar = "Pillar";

        private const string On = "On";

        public static readonly IReadOnlyList<DeckFamily> All =
        [
            new("RoadTech", Decks(RoadTech, SnowRoad), Surface.Road),
            new("RoadDirt", Decks(RoadDirt), Surface.Dirt),
            new("RoadIce", Decks(RoadIce), Surface.Ice),
            new("RoadBump", Decks(RoadBump), null),
            new("SnowRoad", Decks(SnowRoad), Surface.Wood),
            new("RallyRoadDirt", Decks(RoadDirt), Surface.Dirt),
            new("OpenTech", Decks(PlatformTech), Surface.Road),
            new("OpenDirt", Decks(PlatformTech), Surface.Dirt),
            new("OpenGrass", Decks(PlatformTech), Surface.Grass),
            new("OpenIce", Decks(PlatformTech), Surface.Ice),
            new("PlatformTech", Decks(PlatformTech), Surface.Road),
            new("PlatformDirt", Decks(PlatformTech), Surface.Dirt),
            new("PlatformIce", Decks(PlatformTech), Surface.Ice),
            new("PlatformGrass", Decks(PlatformTech), Surface.Grass),
            new("PlatformPlastic", Decks(PlatformTech), Surface.Plastic),
        ];

        public static DeckFamily? Of(Block block) => block.Id.Contains(Pillar, StringComparison.Ordinal)
            ? null
            : All.Where(family => block.Id.StartsWith(family.Prefix, StringComparison.Ordinal)).OrderByDescending(family => family.Prefix.Length).FirstOrDefault();

        public bool IsDeck(string link) => DeckMaterials.Contains(link);

        public string? DeckOverlay(string link)
        {
            var stem = link[(link.LastIndexOf('\\') + 1)..];
            foreach (var deck in DeckMaterials)
            {
                var suffix = On + deck[(deck.LastIndexOf('\\') + 1)..];
                if (stem.EndsWith(suffix, StringComparison.Ordinal) && stem.Length > suffix.Length)
                {
                    return link[..(link.LastIndexOf('\\') + 1)] + stem[..^suffix.Length];
                }
            }
            return null;
        }

        private static IReadOnlySet<string> Decks(params string[] links) => new HashSet<string>(links, StringComparer.OrdinalIgnoreCase);
    }
}
