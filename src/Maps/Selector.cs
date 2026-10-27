using tman.Core;

namespace tman.Maps
{
    public readonly record struct Selector(string Id, bool IsBlock, int? Variant)
    {
        public static implicit operator Selector(Block block) => new(block.Id, true, null);

        public static implicit operator Selector(BlockVariant variant) => new(variant.Model.Id, true, variant.Variant);

        public static implicit operator Selector(Item item) => new(item.Id, false, null);

        public static implicit operator Selector(ItemVariant variant) => new(variant.Model.Id, false, variant.Variant);

        public bool Matches(Placement placement) =>
            placement.IsBlock == IsBlock && placement.Id == Id && (Variant is null || placement.Variant == Variant);
    }
}
