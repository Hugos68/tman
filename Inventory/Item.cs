using GBX.NET;

namespace tman.Inventory
{
    public sealed record Item(string Id, string Author, string? Path, Collections Collections, Waypoint Waypoint, int Variants, Vec3 Size)
    {
        public Item? As(Waypoint kind) => kind == Waypoint ? this : Items.Families.TryGetValue((Id, kind), out var twin) ? twin : null;

        public ItemVariant Variant(int n) => new(this, n);
    }
}
