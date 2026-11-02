using System.Collections.Generic;
using Tman.Core.Catalog;
using Tman.Core.Maps.Geometry;
using Tman.Core.Maps.Placements;

namespace Tman.Core.Alterations.Finishes
{
    internal static class WaterFootprints
    {
        private const uint FlagsMask = 0x0FE01FFF;

        private static readonly Dictionary<(Collections Collection, string Tile, uint Flags), ulong> DeepSubcells = new()
        {
            [(Collections.RedIsland, "Water", 0x00001000)] = 0xFFFFFFFFFFFFFFFF,
            [(Collections.RedIsland, "WaterHill", 0x00001000)] = 0x3F3FFFFFFFFFFFFF,
            [(Collections.RedIsland, "WaterHill", 0x00001001)] = 0x3C3CFFFFFFFFFFFF,
            [(Collections.RedIsland, "WaterHill", 0x00001002)] = 0x1F3F7FFFFFFEFCFC,
            [(Collections.RedIsland, "WaterHill", 0x00001003)] = 0x181818FFFFF8F8F8,
            [(Collections.RedIsland, "WaterHill", 0x00001005)] = 0x000000FFFFFFFFFF,
            [(Collections.RedIsland, "WaterHill", 0x00001006)] = 0x000000FFFFFFFCF8,
            [(Collections.RedIsland, "WaterHill", 0x00001007)] = 0x000000FFFF1F1F1F,
            [(Collections.RedIsland, "WaterHill", 0x00001008)] = 0x000000FFFF3C1818,
            [(Collections.RedIsland, "WaterHill", 0x00001009)] = 0x000000C0F0F0F8F8,
            [(Collections.RedIsland, "WaterHill", 0x0000100A)] = 0x000000E0F0F03838,
            [(Collections.RedIsland, "WaterHill", 0x0000100B)] = 0x000000FFFF000000,
            [(Collections.RedIsland, "WaterHill", 0x0000100C)] = 0x000000F8F8000000,
            [(Collections.RedIsland, "WaterHill", 0x0000100D)] = 0x0000000000000000,
            [(Collections.RedIsland, "WaterHill", 0x00001045)] = 0x000000FFFFFFFFFF,
            [(Collections.RedIsland, "WaterHill", 0x00001049)] = 0x000000E0F0F8F8F8,
            [(Collections.RedIsland, "WaterHill", 0x0000104A)] = 0x000000C0F0F87838,
            [(Collections.RedIsland, "WaterHill", 0x0000104B)] = 0x000000FFFF000000,
            [(Collections.RedIsland, "WaterHill", 0x00001085)] = 0x000000FFFFFFFFFF,
            [(Collections.RedIsland, "WaterHill", 0x00001089)] = 0x000000C0E0F0F8F8,
            [(Collections.RedIsland, "WaterHill", 0x0000108A)] = 0x000000C0F0F07838,
            [(Collections.RedIsland, "WaterHill", 0x0000108B)] = 0x000000FFFF000000,
            [(Collections.RedIsland, "WaterHill", 0x000010CB)] = 0x000000FFFF000000,
            [(Collections.RedIsland, "WaterHill", 0x0000110B)] = 0x000000FFFF000000,
            [(Collections.RedIsland, "WaterHill", 0x0000114B)] = 0x000000FFFF000000,
            [(Collections.RedIsland, "WaterHill", 0x0000118B)] = 0x000000FFFF000000,
            [(Collections.RedIsland, "WaterHill", 0x000011CB)] = 0x000000FFEF000000,
            [(Collections.WhiteShore, "Water", 0x00001000)] = 0xFFFFFFFFFFFFFFFF,
            [(Collections.WhiteShore, "WaterShore1", 0x00001000)] = 0x07070F0F3FFFFFFF,
            [(Collections.WhiteShore, "WaterShore1", 0x00001002)] = 0x07070F0C30F0E0E0,
            [(Collections.WhiteShore, "WaterShore1", 0x00001005)] = 0x0000000000FFFFFF,
            [(Collections.WhiteShore, "WaterShore1", 0x00001009)] = 0x000000000080C0E0,
            [(Collections.WhiteShore, "WaterShore1", 0x00001040)] = 0x07070F1F3FFFFFFF,
            [(Collections.WhiteShore, "WaterShore1", 0x00001045)] = 0x0000000000FFFFFF,
            [(Collections.WhiteShore, "WaterShore1", 0x00001049)] = 0x0000000000C0E0E0,
            [(Collections.WhiteShore, "WaterShore1", 0x00001080)] = 0x07070F0F3FFFFFFF,
            [(Collections.WhiteShore, "WaterShore1", 0x00001085)] = 0x0000000000E3FFFF,
            [(Collections.WhiteShore, "WaterShore1", 0x00001089)] = 0x000000000080C0E0,
            [(Collections.GreenCoast, "Lake", 0x00001000)] = 0xFFFFFFFFFFFFFFFF,
            [(Collections.GreenCoast, "LakeShore", 0x00001000)] = 0x0F0F1F3FFFFFFFFF,
            [(Collections.GreenCoast, "LakeShore", 0x00001001)] = 0x00000000FFFFFFFF,
            [(Collections.GreenCoast, "LakeShore", 0x00001002)] = 0x0F0F0F0FF0F0F0F0,
            [(Collections.GreenCoast, "LakeShore", 0x00001003)] = 0x00000000E0F0F0F0,
            [(Collections.GreenCoast, "LakeShore", 0x00001005)] = 0x00000000FFFFFFFF,
            [(Collections.GreenCoast, "LakeShore", 0x00001006)] = 0x0000000080C0E0F0,
            [(Collections.GreenCoast, "LakeShore", 0x00001007)] = 0x000000000103070F,
            [(Collections.GreenCoast, "LakeShore", 0x00001008)] = 0x0000000000000000,
            [(Collections.GreenCoast, "LakeShore", 0x00001009)] = 0x0000000080C0E0F0,
            [(Collections.GreenCoast, "LakeShore", 0x0000100A)] = 0x0000000000000000,
            [(Collections.GreenCoast, "LakeShore", 0x0000100B)] = 0x0000000000000000,
            [(Collections.GreenCoast, "LakeShore", 0x0000100C)] = 0x0000000000000000,
            [(Collections.GreenCoast, "LakeShore", 0x0000100D)] = 0x0000000000000000,
            [(Collections.GreenCoast, "LakeShore", 0x00001045)] = 0x00000000FFFFFFFF,
            [(Collections.GreenCoast, "LakeShore", 0x0000104B)] = 0x0000000000000000,
            [(Collections.GreenCoast, "LakeShore", 0x0000104C)] = 0x0000000000000000,
            [(Collections.GreenCoast, "LakeShore", 0x00001085)] = 0x00000000F1FFFFFF,
            [(Collections.GreenCoast, "LakeShore", 0x0000108B)] = 0x0000000000000000,
            [(Collections.BlueBay, "Beach", 0x00001000)] = 0xC080000000000000,
            [(Collections.BlueBay, "Beach", 0x00001002)] = 0xC0C0000000000303,
            [(Collections.BlueBay, "Beach", 0x00001005)] = 0xFFE7000000000000,
            [(Collections.BlueBay, "Beach", 0x00001009)] = 0xFFFF070303030303,
            [(Collections.BlueBay, "Beach", 0x00001040)] = 0xC080000000000000,
            [(Collections.BlueBay, "Beach", 0x00001042)] = 0xC080000000000103,
            [(Collections.BlueBay, "Beach", 0x00001045)] = 0xFFFF000000000000,
            [(Collections.BlueBay, "Beach", 0x00001049)] = 0xFFFF0F0703030303,
            [(Collections.BlueBay, "Beach", 0x00001082)] = 0xC080000000000103,
            [(Collections.BlueBay, "Beach", 0x00001085)] = 0xFFE7000000000000,
            [(Collections.BlueBay, "Beach", 0x00001089)] = 0xFFFF030303030303,
            [(Collections.BlueBay, "SeaCliff13", 0x00001000)] = 0xC080000000000000,
            [(Collections.BlueBay, "SeaCliff13", 0x00001002)] = 0xE0E0600000070703,
            [(Collections.BlueBay, "SeaCliff13", 0x00001005)] = 0xFFFF000000000000,
            [(Collections.BlueBay, "SeaCliff13", 0x00001009)] = 0xFFFF1F0303030303,
            [(Collections.BlueBay, "SeaCliff13", 0x00001045)] = 0xFFFF000000000000,
            [(Collections.BlueBay, "SeaCliff13", 0x00001049)] = 0xFFFF7F0F0F0F0703,
            [(Collections.BlueBay, "Sea_Land0_Land12", 0x00001000)] = 0xFFFF000000000000,
            [(Collections.BlueBay, "Sea_Land0_Land12", 0x00001001)] = 0xFF83000000000000,
            [(Collections.BlueBay, "Sea_Land0_Land12", 0x00001002)] = 0x0000000000000103,
            [(Collections.BlueBay, "Sea_Land0_Land12", 0x00001003)] = 0x00000000000080C0,
            [(Collections.BlueBay, "Sea_Land0_Land12", 0x00001004)] = 0x0000000000000103,
            [(Collections.BlueBay, "Sea_Land0_Land12", 0x00001005)] = 0x00000000000080C0,
            [(Collections.BlueBay, "Sea_Land0_Land12", 0x00001006)] = 0xC0E0600000060703,
            [(Collections.BlueBay, "Sea_Land0_Land12", 0x00001007)] = 0x00000000000080C0,
            [(Collections.BlueBay, "Sea_Land0_Land12", 0x00001008)] = 0x0301000000000000,
        };

        public static IEnumerable<(int X, int Z)> Subcells(Collections collection, Placement block)
        {
            if (!DeepSubcells.TryGetValue((collection, block.Id, block.Flags & FlagsMask), out var deep))
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
