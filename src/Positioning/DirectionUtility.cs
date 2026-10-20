using GBX.NET;

namespace tman.src.Positioning
{
    public static class DirectionUtility
    {
        public static Direction CalculateDirection(Direction direction, DirectionOffset offset)
        {
            return (Direction)(((int)direction + (int)offset) % 4);
        }
    }
}