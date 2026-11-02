using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using GBX.NET;
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

        internal void PlaceEmbedded(Ident ident, byte[] item)
        {
            Embed(ident.Id, item);
            Challenge.PlaceAnchoredObject(ident, Vec3.Zero, Vec3.Zero, Vec3.Zero);
        }

        internal IReadOnlyList<string> EmbeddedFiles
        {
            get
            {
                using var zip = Challenge.OpenReadEmbeddedZipData();
                return [.. zip.Entries.Select(entry => entry.FullName)];
            }
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
