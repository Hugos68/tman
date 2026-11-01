using System.Collections.Generic;
using GBX.NET.Engines.Plug;

namespace Tman.Core.Surfaces
{
    public sealed record SurfaceTarget(string Link, CPlugSurface.MaterialId Physics)
    {
        private const string Materials = @"Stadium\Media\Material\";

        private const string Modifiers = @"Stadium\Media\Modifier\";

        private static readonly Dictionary<Surface, SurfaceTarget> BySurface = new()
        {
            [Surface.Road] = new(Materials + "PlatformTech", CPlugSurface.MaterialId.Asphalt),
            [Surface.Dirt] = new(Modifiers + @"PlatformDirt\PlatformTech", CPlugSurface.MaterialId.Dirt),
            [Surface.Ice] = new(Modifiers + @"PlatformIce\PlatformTech", CPlugSurface.MaterialId.RoadIce),
            [Surface.Grass] = new(Modifiers + @"PlatformGrass\PlatformTech", CPlugSurface.MaterialId.Green),
            [Surface.Plastic] = new(Modifiers + @"PlatformPlastic\PlatformTech", CPlugSurface.MaterialId.Plastic),
            [Surface.Wood] = new(Materials + "ThemeSnowRoad", CPlugSurface.MaterialId.Wood),
            [Surface.Metal] = new(Materials + "Structure", CPlugSurface.MaterialId.ResonantMetal)
        };

        public static SurfaceTarget Of(Surface surface) => BySurface[surface];
    }
}
