using System.IO;
using System.IO.Compression;
using Tman.Core.Catalog;
using Tman.Core.Surfaces;

namespace Tman.Core.Maps
{
    public sealed partial class Map
    {
        internal void Embed(string name, Stream content)
        {
            Challenge.UpdateEmbeddedZipData(zip =>
            {
                if (zip.GetEntry(name) is not null)
                {
                    return;
                }
                var entry = zip.CreateEntry(name);
                using var entryStream = entry.Open();
                content.CopyTo(entryStream);
            });
        }

        internal void Embed(string name, byte[] content)
        {
            using var stream = new MemoryStream(content);
            Embed(name, stream);
        }

        internal void EmbedSurfaceBlock(SurfaceBlockFile file, byte[] bytes)
        {
            Challenge.UpdateEmbeddedZipData(zip =>
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
