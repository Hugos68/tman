using Tman.Core.Catalog;

namespace Tman.Core.Surfaces
{
    internal sealed record SurfaceBlock(Surface Surface, Block Archetype)
    {
        public string Name => Surface.Name + Archetype.Id;
    }
}
