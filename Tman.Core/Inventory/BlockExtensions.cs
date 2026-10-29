using System.Linq;

namespace Tman.Core.Inventory
{
    public static class BlockExtensions
    {
        public static (int, int)[] Footprint(this Block block) => block.Cells is null
            ? [.. Enumerable.Range(0, block.SizeX * block.SizeZ).Select(i => (i % block.SizeX, i / block.SizeX))]
            : [.. block.Cells.Select(cell => (cell[0], cell[1]))];
    }
}
