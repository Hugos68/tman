namespace Tman.Core.Surfaces
{
    internal static class SurfaceBlockKey
    {
        public const int Ground = 0;

        public const int Air = 1_000_000;

        public const int AdditionalVariantStep = 1_000;

        public static int Of(bool ground, int additionalVariant) => (ground ? Ground : Air) + AdditionalVariantStep * additionalVariant;
    }
}
