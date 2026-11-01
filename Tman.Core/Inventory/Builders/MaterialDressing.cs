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
                    if (GamePaks.Store.Resolve(skinned + MaterialExtension) is not null)
                    {
                        return skinned;
                    }
                }
                return link;
            }
            if (modifierFolder is not null)
            {
                var modified = modifierFolder + "\\" + stem;
                if (GamePaks.Store.Resolve(modified + MaterialExtension) is not null)
                {
                    return modified;
                }
            }
            return link;
        }
    }
}
