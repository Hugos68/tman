using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Tman.Paks;

namespace Tman.Core.Surfaces.Building
{
    public sealed class FileBlockCache(string folder, IPakStore paks) : IBlockCache
    {
        private const string BlocksFolder = "Blocks";

        private const int StampLength = 16;

        private const int BuilderStampLength = 8;

        private readonly Lazy<string> root = new(() => Path.Combine(folder, BlocksFolder, $"{PakStamp(paks)}-{BuilderStamp()}"));

        public byte[]? Read(string key)
        {
            var path = Path.Combine(root.Value, key);
            return File.Exists(path) ? File.ReadAllBytes(path) : null;
        }

        public void Write(string key, byte[] bytes)
        {
            var path = Path.Combine(root.Value, key);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllBytes(path, bytes);
        }

        private static string BuilderStamp() => typeof(FileBlockCache).Assembly.ManifestModule.ModuleVersionId.ToString("N")[..BuilderStampLength];

        private static string PakStamp(IPakStore paks)
        {
            var facts = PakStore.Paks.Select(pak => new FileInfo(Path.Combine(paks.Directory, pak.Name + PakStore.Extension))).Select(info => $"{info.Name}:{info.Length}:{info.LastWriteTimeUtc.Ticks}");
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join("|", facts))))[..StampLength];
        }
    }
}
