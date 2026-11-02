using System.Threading;

namespace Tman.Core.Surfaces.Building
{
    internal sealed class CachedSurfaceBlocks(IBlockCache cache, SurfaceBlockBuilder builder) : ISurfaceBlocks
    {
        private readonly Lock building = new();

        public byte[] Bytes(SurfaceBlockFile file)
        {
            lock (building)
            {
                if (cache.Read(file.CachePath) is { } cached)
                {
                    return cached;
                }
                var bytes = builder.Build(file);
                cache.Write(file.CachePath, bytes);
                return bytes;
            }
        }
    }
}
