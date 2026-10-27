using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GBX.NET;
using GBX.NET.Engines.GameData;
using tman.src.Application;
using tman.src.Maps;

namespace tman.src.Core
{
    public static class Inventory
    {
        private static readonly Dictionary<Item, Ident> Idents = new();

        public static Block? Block(string id) => Blocks.ById.TryGetValue(id, out var block) ? block : null;

        public static Item? Item(string id) =>
            Items.ById.TryGetValue(id, out var item) ? item
            : Custom.ById.TryGetValue(id, out var custom) ? custom
            : null;

        public static Item EffectItem(Item waypoint, Block gate)
        {
            var effect = gate.Id["GateSpecial".Length..];
            var id = waypoint.Id
                .Replace("Start", "Special", StringComparison.Ordinal)
                .Replace("Checkpoint", "Special", StringComparison.Ordinal)
                .Replace("Finish", "Special", StringComparison.Ordinal)
                .Replace("Left", "", StringComparison.Ordinal)
                .Replace("Center", "", StringComparison.Ordinal)
                .Replace("Right", "", StringComparison.Ordinal)
                .Replace("v2", "", StringComparison.Ordinal)
                + effect;
            return Item(id) ?? throw new KeyNotFoundException($"No {effect} gate for {waypoint.Id}: {id}");
        }

        public static (int, int)[] Footprint(this Block block) => block.Cells is null
            ? Enumerable.Range(0, block.SizeX * block.SizeZ).Select(i => (i % block.SizeX, i / block.SizeX)).ToArray()
            : block.Cells.Select(cell => (cell[0], cell[1])).ToArray();

        public static string FullPath(this Item item) =>
            Path.Combine(ApplicationConstants.ProjectDirectory, (item.Path ?? throw new InvalidOperationException($"{item.Id} is not a custom item")).Replace('/', Path.DirectorySeparatorChar));

        public static Ident Ident(this Item item, Map map)
        {
            if (item.Path is null)
            {
                return new Ident(item.Id, map.Gbx.MapInfo.Collection, item.Author);
            }
            if (!Idents.TryGetValue(item, out var ident))
            {
                using var stream = File.OpenRead(item.FullPath());
                if (Gbx.ParseHeaderNode(stream) is not CGameItemModel { Ident: Ident header })
                {
                    throw new InvalidOperationException($"Unable to get ident from {item.Path}");
                }
                Idents[item] = ident = header;
            }
            return ident;
        }
    }
}
