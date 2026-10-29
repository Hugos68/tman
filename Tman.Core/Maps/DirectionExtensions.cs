using GBX.NET;

namespace tman.Maps
{
    public static class DirectionExtensions
    {
        public static Direction Turn(this Direction direction, DirectionOffset offset) =>
            (Direction)(((int)direction + (int)offset) % 4);
    }
}
