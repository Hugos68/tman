using System;
using System.Collections.Generic;

namespace Tman.Core.Surfaces.Building
{
    public sealed class MemoryBlockCache : IBlockCache
    {
        private readonly Dictionary<string, byte[]> blocks = new(StringComparer.Ordinal);

        public byte[]? Read(string key) => blocks.GetValueOrDefault(key);

        public void Write(string key, byte[] bytes) => blocks[key] = bytes;
    }
}
