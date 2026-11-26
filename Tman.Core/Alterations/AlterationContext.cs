using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Tman.Core.Catalog;
using Tman.Core.Game;
using Tman.Core.Maps;
using Tman.Core.Maps.Placements;
using Tman.Core.Surfaces;
using Tman.Core.Surfaces.Building;

namespace Tman.Core.Alterations
{
    internal sealed class AlterationContext(IAssets assets, ISurfaceBlocks surfaceBlocks, ILogger logger)
    {
        public ILogger Logger { get; } = logger;

        public Stream Open(string assetPath) => assets.Open(assetPath);

        public void Embed(Map map, Item custom)
        {
            using var content = assets.Open(custom.Path ?? throw new InvalidOperationException($"{custom.Id} is not a custom item"));
            map.Embed(Path.GetFileName(custom.Path), content);
        }

        public bool Replace(Map map, Placement placement, SurfaceBlock model)
        {
            var file = map.SurfaceBlockFile(placement, model);
            var custom = placement.IsBlock ? null : map.EmbeddedItem(placement.Id);
            if ((custom is null ? surfaceBlocks.Bytes(file) : surfaceBlocks.CustomItemBytes(file, custom)) is not { } bytes)
            {
                return false;
            }
            if (bytes.Length == 0)
            {
                map.Remove(placement);
                return true;
            }
            if (file.Invisible)
            {
                map.PlaceInvisible(placement, file, bytes);
                return true;
            }
            map.Replace(placement, file, bytes);
            return true;
        }
    }
}
