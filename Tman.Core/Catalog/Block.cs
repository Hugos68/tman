namespace Tman.Core.Catalog
{
    public sealed record Block(string Id, int SizeX, int SizeZ, int[][]? Cells, Collections Collections, Waypoint Waypoint, bool Ground, bool Air, int Variants)
    {
        public Block? Plain => Blocks.PlainOf.TryGetValue(Id, out var plain) ? plain : null;

        public Block? As(Waypoint kind) => kind == Waypoint ? this : Blocks.Families.TryGetValue((Id, kind), out var twin) ? twin : null;

        public BlockVariant Variant(int n) => new(this, n);
    }
}
