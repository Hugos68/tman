using System;
using System.IO;
using GBX.NET;
using GBX.NET.Engines.Game;
using GBX.NET.Engines.GameData;

namespace tman.src.Inventory
{
    public static class EmbedUtility
    {
        public static void Embed(CGameCtnChallenge map, string path)
        {
            map.UpdateEmbeddedZipData(zip =>
            {
                var entry = zip.CreateEntry(Path.GetFileName(path));
                using var entryStream = entry.Open();
                using var fileStream = File.OpenRead(path);
                fileStream.CopyTo(entryStream);
            });
        }

        public static Ident ExtractIdent(string path)
        {
            using var stream = File.OpenRead(path);
            if (Gbx.ParseHeaderNode(stream) is not CGameItemModel { Ident: Ident ident })
            {
                throw new InvalidOperationException($"Unable to get ident from {path}");
            }
            return ident;
        }
    }
}