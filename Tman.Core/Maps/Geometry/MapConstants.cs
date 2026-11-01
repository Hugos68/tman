namespace Tman.Core.Maps.Geometry
{
    internal static class MapConstants
    {
        public const float CellSize = 32f;
        public const float SubcellSize = 4f;

        public const int SubcellsPerCell = (int)(CellSize / SubcellSize);
        public const float CellHeight = 8f;
        public const float ItemStartZOffsetZ = -10.75f;
    }
}