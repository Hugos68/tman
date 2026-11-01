using System.Collections.Generic;

namespace Tman.Core.Surfaces
{
    internal sealed record Surface(string Name, string PlatformFamily)
    {
        public static readonly Surface Road = new("Road", "Tech");

        public static readonly Surface Dirt = new("Dirt", "Dirt");

        public static readonly Surface Ice = new("Ice", "Ice");

        public static readonly Surface Grass = new("Grass", "Grass");

        public static readonly Surface Plastic = new("Plastic", "Plastic");

        public static readonly Surface Wood = new("Wood", "Wood");
        public static readonly Surface Metal = new("Metal", "Metal");

        public static readonly IReadOnlyList<Surface> All = [Road, Dirt, Ice, Grass, Plastic, Wood, Metal];
    }
}
