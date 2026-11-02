using System.Threading;
using Microsoft.Extensions.Logging;

namespace Tman.Core.Surfaces.Building
{
    internal sealed class CachedSurfaceBlocks(IBlockCache cache, SurfaceBlockBuilder builder, ILogger<CachedSurfaceBlocks> logger) : ISurfaceBlocks
    {
        private readonly Lock building = new();

        public byte[] Bytes(SurfaceBlockFile file)
        {
            lock (building)
            {
                if (cache.Read(file.CachePath) is { } cached)
                {
                    logger.LogDebug("{Block} from the cache", file.Name);
                    return cached;
                }
                var bytes = builder.Build(file);
                cache.Write(file.CachePath, bytes);
                return bytes;
            }
        }
    }
}
