using Tman.Core.Catalog;

namespace Tman.Core.Surfaces
{
    public sealed record SurfaceBlock(Surface Surface, Block Archetype)
    {
        public string Name => Surface.Name + Archetype.Id;
    }
}
