using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace tman.src.Inventory
{
    public static class Blocks
    {
        public sealed record Block(int SizeX, int SizeZ, int[][]? Cells)
        {
            public (int, int)[] Footprint => Cells is null
                ? Enumerable.Range(0, SizeX * SizeZ).Select(i => (i % SizeX, i / SizeX)).ToArray()
                : Cells.Select(c => (c[0], c[1])).ToArray();
        }

        private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

        public static Dictionary<string, Block> Load(string path) =>
            JsonSerializer.Deserialize<Dictionary<string, Block>>(File.ReadAllText(path), Options) ?? new();
    }
}
