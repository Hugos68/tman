using GBX.NET;

namespace Tman.Core.Maps.Geometry
{
    internal static class DirectionExtensions
    {
        public static Direction Turn(this Direction direction, DirectionOffset offset) =>
            (Direction)(((int)direction + (int)offset) % 4);

        public static (int X, int Z) Turn(this Direction direction, (int X, int Z) cell, (int X, int Z) size) => direction switch
        {
            Direction.East => (size.Z - 1 - cell.Z, cell.X),
            Direction.South => (size.X - 1 - cell.X, size.Z - 1 - cell.Z),
            Direction.West => (cell.Z, size.X - 1 - cell.X),
            _ => cell,
        };
    }
}
