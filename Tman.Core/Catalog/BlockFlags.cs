namespace Tman.Core.Catalog
{
    public static class BlockFlags
    {
        public const int BaseVariantMask = 0x3F;

        public const int CustomBlock = 0x8000;

        public const int AdditionalVariantShift = 21;

        public const int AdditionalVariantMask = 0x7F << AdditionalVariantShift;

        public const int VariantsPerAdditionalVariant = BaseVariantMask + 1;

        public static int BaseVariant(int flags) => flags & BaseVariantMask;

        public static int AdditionalVariant(int flags) => (flags & AdditionalVariantMask) >> AdditionalVariantShift;

        public static int CombinedVariant(int flags) => BaseVariant(flags) + VariantsPerAdditionalVariant * AdditionalVariant(flags);
    }
}
