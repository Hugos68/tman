namespace Tman.Core.Inventory
{
    public sealed record SurfaceBlock(Surface Surface, Block Archetype)
    {
        public string Name => Surface.Name + Archetype.Id;
    }
}
