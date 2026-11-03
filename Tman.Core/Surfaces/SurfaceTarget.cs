using System.Collections.Generic;
using GBX.NET.Engines.Plug;

namespace Tman.Core.Surfaces
{
    internal sealed record SurfaceTarget(string Link, CPlugSurface.MaterialId Physics, Tint Tint)
    {
        private const string Materials = @"Stadium\Media\Material\";

        private const string Modifiers = @"Stadium\Media\Modifier\";

        private const string CustomMaterials = @"Stadium\Media\Material_BlockCustom\";

        private static readonly Tint Black = new(0, 0, 0);

        private sealed record Looks(string Road, string Platform, CPlugSurface.MaterialId Physics, Tint Tint)
        {
            public Looks(string both, CPlugSurface.MaterialId physics) : this(both, both, physics, Tint.None) { }

            public Looks(string road, string platform, CPlugSurface.MaterialId physics) : this(road, platform, physics, Tint.None) { }

            public Looks(string both, CPlugSurface.MaterialId physics, Tint tint) : this(both, both, physics, tint) { }
        }

        private static readonly Dictionary<Surface, Looks> BySurface = new()
        {
            [Surface.Road] = new(Materials + "PlatformTech", CPlugSurface.MaterialId.Asphalt),
            [Surface.Dirt] = new(Modifiers + @"PlatformDirt\PlatformTech", CPlugSurface.MaterialId.Dirt),
            [Surface.Ice] = new(Materials + "RoadIce", Modifiers + @"PlatformIce\PlatformTech", CPlugSurface.MaterialId.RoadIce),
            [Surface.Grass] = new(Modifiers + @"PlatformGrass\PlatformTech", CPlugSurface.MaterialId.Green),
            [Surface.Plastic] = new(Modifiers + @"PlatformPlastic\PlatformTech", CPlugSurface.MaterialId.Plastic),
            [Surface.Wood] = new(Materials + "ThemeSnowRoad", CPlugSurface.MaterialId.Wood),
            [Surface.Metal] = new(Materials + "Structure", CPlugSurface.MaterialId.Metal),
            [Surface.Magnet] = new(CustomMaterials + "CustomConcrete", CPlugSurface.MaterialId.TechSuperMagnetic, Black)
        };

        public static SurfaceTarget Of(Surface surface, DeckFamily family)
        {
            var looks = BySurface[surface];
            return new SurfaceTarget(family.IsPlatform ? looks.Platform : looks.Road, looks.Physics, looks.Tint);
        }
    }
}
