using System.Linq;
using GBX.NET;

namespace Tman.Core.Inventory
{
    public static class BlockExtensions
    {
        public static (int X, int Z)[] Footprint(this Block block) => block.Cells is null
            ? [.. Enumerable.Range(0, block.SizeX * block.SizeZ).Select(i => (i % block.SizeX, i / block.SizeX))]
            : [.. block.Cells.Select(cell => (cell[0], cell[1]))];

        public static (int X, int Z)[] Footprint(this Block block, Direction direction) =>
            [.. block.Footprint().Select(cell => direction switch
            {
                Direction.East => (block.SizeZ - 1 - cell.Z, cell.X),
                Direction.South => (block.SizeX - 1 - cell.X, block.SizeZ - 1 - cell.Z),
                Direction.West => (cell.Z, block.SizeX - 1 - cell.X),
                _ => cell,
            })];
    }
}
