using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Tman.Paks;

namespace Tman.Core.Inventory.Builders
{
    public static class SurfaceBlockCache
    {
        private const string FolderName = "Cache";

        private static readonly Lazy<string> Root = new(() => Path.Combine(Directory.GetParent(GamePaks.Directory)!.FullName, FolderName, "Blocks", PakStamp()));

        public static byte[] GetOrBuild(SurfaceBlockFile file)
        {
            var path = Path.Combine(Root.Value, file.CachePath);
            if (File.Exists(path))
            {
                return File.ReadAllBytes(path);
            }
            var bytes = SurfaceBlockBuilder.Build(file);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllBytes(path, bytes);
            return bytes;
        }

        private static string PakStamp()
        {
            var facts = PakStore.Paks.Select(pak => new FileInfo(Path.Combine(GamePaks.Directory, pak.Name + ".pak"))).Select(info => $"{info.Name}:{info.Length}:{info.LastWriteTimeUtc.Ticks}");
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join("|", facts))))[..16];
        }
    }
}
