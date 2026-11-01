using System.IO;
using System.IO.Compression;
using Tman.Core.Catalog;
using Tman.Core.Surfaces;

namespace Tman.Core.Maps
{
    public sealed partial class Map
    {
        internal void Embed(Item custom) => Embed(custom.FullPath());

        internal void Embed(string path)
        {
            Gbx.UpdateEmbeddedZipData(zip =>
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

        internal void EmbedSurfaceBlock(SurfaceBlockFile file, byte[] bytes)
        {
            Gbx.UpdateEmbeddedZipData(zip =>
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
