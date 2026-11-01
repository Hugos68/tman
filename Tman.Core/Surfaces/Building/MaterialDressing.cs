using System;
using System.Collections.Generic;
using System.Linq;
using GBX.NET.Engines.Game;
using GBX.NET.Engines.Plug;
using Tman.Core.Catalog;
using Tman.Core.Game;
using Tman.Paks;

namespace Tman.Core.Surfaces.Building
{
    internal sealed class MaterialDressing(Collections collection, IReadOnlyList<string> modifierFolders)
    {
        private const string TerrainModifierExtension = ".TerrainModifier.Gbx";

        private const string EnvironmentSkinFolder = @"Media\Modifier\StadiumOnTerrain";

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
            var name = PakPath.Name(plain);
            var painted = name.StartsWith(Decal, StringComparison.Ordinal) ? Decal + Paint + name[Decal.Length..] : Paint + name;
            return new[] { plain, PakPath.Join(PakPath.Folder(plain), painted) }.FirstOrDefault(GameMaterials.Exists);
        }

        public CPlugSurface.GameplayId Gameplay(string collisionLink)
        {
            foreach (var folder in modifierFolders)
            {
                if (GamePaks.Store.Open(PakPath.Join(folder, Collision) + GameMaterials.Extension)?.Node is CPlugMaterial material)
                {
                    return material.GameplayId;
                }
            }
            return GameMaterials.GameplayOfCollision(collisionLink);
        }

        private string Resolve(string link)
        {
            var name = PakPath.Name(link);
            if (collection != Collections.Stadium)
            {
                foreach (var candidate in new[] { name, name + InWorld })
                {
                    var skinned = PakPath.Join(PakPath.Join(collection.ToString(), EnvironmentSkinFolder), candidate);
                    if (GameMaterials.Exists(skinned))
                    {
                        return skinned;
                    }
                }
            }
            var unbranded = name.Replace(Placeholder, "", StringComparison.Ordinal);
            var plain = unbranded.Replace(Special, "", StringComparison.Ordinal);
            foreach (var folder in modifierFolders)
            {
                foreach (var candidate in new[] { name, unbranded, plain })
                {
                    var modified = PakPath.Join(folder, candidate);
                    if (candidate.Length > 0 && GameMaterials.Exists(modified))
                    {
                        return modified;
                    }
                }
            }
            return link;
        }
    }
}
