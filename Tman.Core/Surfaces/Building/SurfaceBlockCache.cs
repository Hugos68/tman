using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Tman.Core.Game;
using Tman.Paks;

namespace Tman.Core.Surfaces.Building
{
    internal static class SurfaceBlockCache
    {
        private const string FolderName = "Cache";

        private const string BlocksFolder = "Blocks";

        private const string ApplicationFolder = "tman";

        private const int StampLength = 16;

        private const int BuilderStampLength = 8;

        private static readonly Lazy<string> Root = new(() => Path.Combine(CacheFolder(), BlocksFolder, $"{PakStamp()}-{BuilderStamp()}"));

        private static readonly object Building = new();

        public static byte[] GetOrBuild(SurfaceBlockFile file)
        {
            var path = Path.Combine(Root.Value, file.CachePath);
            lock (Building)
            {
                if (File.Exists(path))
                {
                    return File.ReadAllBytes(path);
                }
                var bytes = SurfaceBlockBuilder.Build(file);
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                File.WriteAllBytes(path, bytes);
                return bytes;
            }
        }

        private static string CacheFolder()
        {
            var paks = new DirectoryInfo(GamePaks.Directory);
            if (paks.Name == PakDirectory.FolderName && paks.Parent is { } repository)
            {
                return Path.Combine(repository.FullName, FolderName);
            }
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ApplicationFolder, FolderName);
        }

        private static string BuilderStamp() => typeof(SurfaceBlockBuilder).Assembly.ManifestModule.ModuleVersionId.ToString("N")[..BuilderStampLength];

        private static string PakStamp()
        {
            var facts = PakStore.Paks.Select(pak => new FileInfo(Path.Combine(GamePaks.Directory, pak.Name + PakStore.Extension))).Select(info => $"{info.Name}:{info.Length}:{info.LastWriteTimeUtc.Ticks}");
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join("|", facts))))[..StampLength];
        }
    }
}
