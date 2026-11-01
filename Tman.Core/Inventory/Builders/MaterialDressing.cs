using System;
using System.Collections.Generic;
using GBX.NET.Engines.Game;
using Tman.Paks;

namespace Tman.Core.Inventory.Builders
{
    public sealed class MaterialDressing(Collections collection, string? modifierFolder)
    {
        private const string MaterialExtension = ".Material.Gbx";

        private const string TerrainModifierExtension = ".TerrainModifier.Gbx";

        private const string EnvironmentSkinFolder = @"\Media\Modifier\StadiumOnTerrain\";

        private const string InWorld = "InWorld";

        private const string Special = "Special";

        private readonly Dictionary<string, string> dressed = new(StringComparer.OrdinalIgnoreCase);

        public static MaterialDressing For(Collections collection, string infoPath, CGameCtnBlockInfo info, int ancestor)
        {
            var modifier = info.MaterialModifierFile?.FilePath;
            var folder = modifier is not null && modifier.EndsWith(TerrainModifierExtension, StringComparison.OrdinalIgnoreCase)
                ? PakStore.Logical(infoPath, ancestor, modifier)[..^TerrainModifierExtension.Length]
                : null;
            return new MaterialDressing(collection, folder);
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
            if (modifierFolder is not null)
            {
                var modifierName = modifierFolder[(modifierFolder.LastIndexOf('\\') + 1)..];
                foreach (var candidate in new[] { stem, stem.Replace(modifierName, "", StringComparison.Ordinal).Replace(Special, "", StringComparison.Ordinal) })
                {
                    var modified = modifierFolder + "\\" + candidate;
                    if (candidate.Length > 0 && Exists(modified))
                    {
                        return modified;
                    }
                }
            }
            return link;
        }

        private static bool Exists(string link) => GamePaks.Store.Resolve(link + MaterialExtension) is not null;
    }
}
