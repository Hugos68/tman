using System.IO;
using System.IO.Compression;
using Tman.Core.Catalog;
using Tman.Core.Surfaces;

namespace Tman.Core.Maps
{
    public static class MapEmbedding
    {
        public static void Embed(this Map map, Item custom) => map.Embed(custom.FullPath());

        internal static void Embed(this Map map, string path)
        {
            map.Gbx.UpdateEmbeddedZipData(zip =>
            {
                var name = Path.GetFileName(path);
                if (zip.GetEntry(name) is not null)
                {
                    return;
                }
                var entry = zip.CreateEntry(name);
                using var entryStream = entry.Open();
                using var fileStream = File.OpenRead(path);
                fileStream.CopyTo(entryStream);
            });
        }

        internal static void EmbedSurfaceBlock(this Map map, SurfaceBlockFile file, byte[] bytes)
        {
            map.Gbx.UpdateEmbeddedZipData(zip =>
            {
                if (zip.GetEntry(file.EmbeddedEntry) is not null)
                {
                    return;
                }
                var entry = zip.CreateEntry(file.EmbeddedEntry, CompressionLevel.Optimal);
                using var entryStream = entry.Open();
                entryStream.Write(bytes);
            });
        }
    }
}
