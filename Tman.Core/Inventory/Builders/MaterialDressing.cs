using System;
using System.Collections.Generic;
using System.Linq;
using GBX.NET.Engines.Game;
using GBX.NET.Engines.Plug;
using Tman.Paks;

namespace Tman.Core.Inventory.Builders
{
    public sealed class MaterialDressing(Collections collection, IReadOnlyList<string> modifierFolders)
    {
        private const string MaterialExtension = ".Material.Gbx";

        private const string TerrainModifierExtension = ".TerrainModifier.Gbx";

        private const string EnvironmentSkinFolder = @"\Media\Modifier\StadiumOnTerrain\";

        private const string GameplayMaterials = @"Effects\Media\Material\Collision";

        private const string InWorld = "InWorld";

        private const string Special = "Special";

        private const string Placeholder = "Turbo";

        private const string Decal = "Decal";

        private const string Paint = "Paint";

        private const string Collision = "Collision";

        private readonly Dictionary<string, string> dressed = new(StringComparer.OrdinalIgnoreCase);

        public static MaterialDressing For(Collections collection, string infoPath, CGameCtnBlockInfo info, int ancestor)
        {
            var folders = new[] { info.MaterialModifierFile?.FilePath, info.MaterialModifier2File?.FilePath }
                .Where(modifier => modifier is not null && modifier.EndsWith(TerrainModifierExtension, StringComparison.OrdinalIgnoreCase))
                .Select(modifier => PakStore.Logical(infoPath, ancestor, modifier!.Trim())[..^TerrainModifierExtension.Length])
                .ToList();
            return new MaterialDressing(collection, folders);
        }

        public string Dress(string link)
        {
            if (!dressed.TryGetValue(link, out var result))
            {
                result = Resolve(link);
                dressed[link] = result;
            }
            return result;
        }

        public static string? OverlayTwin(string plain)
        {
            var stem = plain[(plain.LastIndexOf('\\') + 1)..];
            var folder = plain[..(plain.LastIndexOf('\\') + 1)];
            var painted = stem.StartsWith(Decal, StringComparison.Ordinal) ? Decal + Paint + stem[Decal.Length..] : Paint + stem;
            return new[] { plain, folder + painted }.FirstOrDefault(Exists);
        }

        public bool IsGameplayStrip(string link) => link.StartsWith(GameplayMaterials, StringComparison.OrdinalIgnoreCase);

        public CPlugSurface.GameplayId Gameplay(string link)
        {
            foreach (var folder in modifierFolders)
            {
                if (GamePaks.Store.Open($@"{folder}\{Collision}{MaterialExtension}")?.Node is CPlugMaterial material)
                {
                    return material.GameplayId;
                }
            }
            return GameplayOf(link[GameplayMaterials.Length..]);
        }

        public static CPlugSurface.GameplayId GameplayOf(string collisionName) =>
            Enum.GetValues<CPlugSurface.GameplayId>()
                .Where(gameplay => collisionName.StartsWith(gameplay.ToString(), StringComparison.Ordinal))
                .OrderByDescending(gameplay => gameplay.ToString().Length)
                .FirstOrDefault(CPlugSurface.GameplayId.None);

        private string Resolve(string link)
        {
            var stem = link[(link.LastIndexOf('\\') + 1)..];
            if (collection != Collections.Stadium)
            {
                foreach (var candidate in new[] { stem, stem + InWorld })
                {
                    var skinned = collection + EnvironmentSkinFolder + candidate;
                    if (Exists(skinned))
                    {
                        return skinned;
                    }
                }
            }
            var unbranded = stem.Replace(Placeholder, "", StringComparison.Ordinal);
            var plain = unbranded.Replace(Special, "", StringComparison.Ordinal);
            foreach (var folder in modifierFolders)
            {
                foreach (var candidate in new[] { stem, unbranded, plain })
                {
                    var modified = folder + "\\" + candidate;
                    if (candidate.Length > 0 && Exists(modified))
                    {
                        return modified;
                    }
                }
            }
            return link;
        }

        public static bool Exists(string link) => GamePaks.Store.Resolve(link + MaterialExtension) is not null;
    }
}
