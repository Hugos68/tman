using System;
using System.Collections.Generic;
using System.Linq;
using GBX.NET;
using GBX.NET.Engines.Plug;
using Tman.Core.Game;
using Tman.Paks;

namespace Tman.Core.Surfaces.Building
{
    internal static class GameMaterials
    {
        public const string Extension = ".Material.Gbx";

        private const string GameplayCollisionPrefix = @"Effects\Media\Material\Collision";

        private const string EnvironmentMaterialsFolder = @"Media\Material";

        private static readonly Dictionary<string, (CPlugSurface.MaterialId Physics, CPlugSurface.GameplayId Gameplay)> SurfaceByLink = new(StringComparer.OrdinalIgnoreCase);

        private static readonly (CPlugSurface.MaterialId Physics, CPlugSurface.GameplayId Gameplay) OutsideTheEnvironmentPaks = (CPlugSurface.MaterialId.Asphalt, CPlugSurface.GameplayId.None);

        public static string Link(string ownerPath, Gbx owner, string filePath)
        {
            var logical = PakStore.Logical(ownerPath, owner.RefTable!.AncestorLevel, filePath);
            var link = logical.EndsWith(Extension, StringComparison.OrdinalIgnoreCase) ? logical[..^Extension.Length] : logical;
            if (Exists(link))
            {
                return link;
            }
            var environmentMaterial = PakPath.Join(PakPath.Join(PakPath.Environment(link), EnvironmentMaterialsFolder), PakPath.Name(link));
            return Exists(environmentMaterial) ? environmentMaterial : link;
        }

        public static bool Exists(string link) => GamePaks.Store.Resolve(link + Extension) is not null;

        public static bool IsGameplayCollision(string link) => link.StartsWith(GameplayCollisionPrefix, StringComparison.OrdinalIgnoreCase);

        public static CPlugSurface.GameplayId GameplayOf(string collisionName) =>
            Enum.GetValues<CPlugSurface.GameplayId>()
                .Where(gameplay => collisionName.StartsWith(gameplay.ToString(), StringComparison.Ordinal))
                .OrderByDescending(gameplay => gameplay.ToString().Length)
                .FirstOrDefault(CPlugSurface.GameplayId.None);

        public static CPlugSurface.GameplayId GameplayOfCollision(string link) => GameplayOf(link[GameplayCollisionPrefix.Length..]);

        public static CPlugSurface.MaterialId PhysicsOf(string link) => SurfaceOf(link).Physics;

        public static (CPlugSurface.MaterialId Physics, CPlugSurface.GameplayId Gameplay) SurfaceOf(string link)
        {
            if (!SurfaceByLink.TryGetValue(link, out var surface))
            {
                surface = IsGameplayCollision(link)
                    ? GameplayCollision(link[GameplayCollisionPrefix.Length..])
                    : GamePaks.Store.Open(link + Extension)?.Node is CPlugMaterial material
                        ? (material.SurfaceId, material.GameplayId)
                        : OutsideTheEnvironmentPaks;
                SurfaceByLink[link] = surface;
            }
            return surface;
        }

        private static (CPlugSurface.MaterialId Physics, CPlugSurface.GameplayId Gameplay) GameplayCollision(string name)
        {
            var gameplay = GameplayOf(name);
            var surface = gameplay == CPlugSurface.GameplayId.None ? name : name[gameplay.ToString().Length..];
            return (Enum.TryParse<CPlugSurface.MaterialId>(surface, out var physics) ? physics : CPlugSurface.MaterialId.Asphalt, gameplay);
        }
    }
}
