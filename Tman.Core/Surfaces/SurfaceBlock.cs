using Tman.Core.Catalog;

namespace Tman.Core.Surfaces
{
    internal sealed record SurfaceBlock(Surface Surface, Block Archetype, Block? Owner = null)
    {
        public string Name => Surface.Name + Archetype.Id + Owner?.Id;
    }
}
