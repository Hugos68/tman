using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using Tman.Core.Maps.Geometry;
using Tman.Core.Maps.Placements;

namespace Tman.Core.Alterations.Finishes
{
    internal sealed class WaterFootprints(Dictionary<(string Tile, uint Flags), ulong> masks)
    {
        private const string File = "WaterFootprints.json";

        private const int FlagsMask = 0x0FE01FFF;

        private const int LastSubcell = MapConstants.SubcellsPerCell - 1;

        public static WaterFootprints Load(AlterationContext context, string environment)
        {
            var masks = new Dictionary<(string, uint), ulong>();
            using var json = context.Open(Assets.Data(File));
            var footprints = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, Dictionary<string, string>>>>(json) ?? new();
            foreach (var (tile, flagMasks) in footprints.TryGetValue(environment, out var tiles) ? tiles : new())
            {
                foreach (var (flags, mask) in flagMasks)
                {
                    masks[(tile, uint.Parse(flags, NumberStyles.HexNumber, CultureInfo.InvariantCulture) & FlagsMask)] = ulong.Parse(mask, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                }
            }
            return new WaterFootprints(masks);
        }

        public IEnumerable<(int X, int Z)> Subcells(Placement block)
        {
            if (!masks.TryGetValue((block.Id, block.Flags & FlagsMask), out var deep))
            {
                yield break;
            }
            for (var z = 0; z < MapConstants.SubcellsPerCell; z++)
            {
                for (var x = 0; x < MapConstants.SubcellsPerCell; x++)
                {
                    if ((deep & (1UL << (z * MapConstants.SubcellsPerCell + x))) != 0)
                    {
                        var turned = block.Direction.Turn((x, z), (MapConstants.SubcellsPerCell, MapConstants.SubcellsPerCell));
                        yield return (block.Cell.X * MapConstants.SubcellsPerCell + turned.X, block.Cell.Z * MapConstants.SubcellsPerCell + turned.Z);
                    }
                }
            }
        }
    }
}
