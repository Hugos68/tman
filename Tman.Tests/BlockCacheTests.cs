using System;
using System.IO;
using System.Linq;
using Tman.Core.Surfaces.Building;
using Tman.Paks;
using Xunit;

namespace Tman.Tests
{
    public class BlockCacheTests
    {
        [Fact]
        public void TheMemoryCacheReturnsWhatWasWrittenAndNothingElse()
        {
            var cache = new MemoryBlockCache();
            Assert.Null(cache.Read("a"));
            cache.Write("a", [1, 2, 3]);
            Assert.Equal([1, 2, 3], cache.Read("a"));
            Assert.Null(cache.Read("b"));
        }

        [Fact]
        public void TheFileCacheKeepsBlocksUnderAStampOfThePaksAndTheBuild()
        {
            var root = Directory.CreateTempSubdirectory("tman-tests").FullName;
            try
            {
                var paks = Path.Combine(root, PakDirectory.FolderName);
                Directory.CreateDirectory(paks);
                foreach (var pak in PakStore.Paks)
                {
                    File.WriteAllBytes(Path.Combine(paks, pak.Name + PakStore.Extension), [0]);
                }
                var cache = new FileBlockCache(Path.Combine(root, "Cache"), new DirectoryOnlyPaks(paks));
                Assert.Null(cache.Read(Path.Combine("Wood", "Ground", "a.Block.Gbx")));
                cache.Write(Path.Combine("Wood", "Ground", "a.Block.Gbx"), [4, 5]);
                Assert.Equal([4, 5], cache.Read(Path.Combine("Wood", "Ground", "a.Block.Gbx")));
                var stamped = Directory.GetDirectories(Path.Combine(root, "Cache", "Blocks")).Single();
                Assert.Matches("^[0-9A-F]{16}-[0-9a-f]{8}$", Path.GetFileName(stamped));
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        private sealed class DirectoryOnlyPaks(string directory) : IPakStore
        {
            public string Directory => directory;

            public System.Collections.Generic.IEnumerable<string> Entries => throw new InvalidOperationException();

            public string? Resolve(string logical) => throw new InvalidOperationException();

            public GBX.NET.Gbx? Open(string logical) => throw new InvalidOperationException();

            public byte[]? ReadDecompressed(string logical) => throw new InvalidOperationException();
        }
    }
}
