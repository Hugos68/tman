using System;
using System.Collections.Generic;
using System.IO;
using GBX.NET;
using GBX.NET.Engines.GameData;
using Tman.Core.Maps;

namespace Tman.Core.Catalog
{
    internal static class ItemExtensions
    {
        private static readonly Dictionary<Item, Ident> Idents = new();

        public static Item For(this Item item, Collections collection) =>
            collection == Collections.Stadium ? item : Custom.FromId(item.Id.Replace(".Item.Gbx", $"{collection}.Item.Gbx")) ?? item;

        public static string FullPath(this Item item) =>
            Path.Combine(Assets.Root, (item.Path ?? throw new InvalidOperationException($"{item.Id} is not a custom item")).Replace('/', Path.DirectorySeparatorChar));

        public static Ident Ident(this Item item, Map map)
        {
            if (item.Path is null)
            {
                return new Ident(item.Id, map.Challenge.MapInfo.Collection, item.Author);
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
