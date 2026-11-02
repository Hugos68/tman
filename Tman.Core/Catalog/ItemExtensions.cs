using GBX.NET;
using Tman.Core.Maps;

namespace Tman.Core.Catalog
{
    internal static class ItemExtensions
    {
        private const string Extension = ".Item.Gbx";

        public static Item For(this Item item, Collections collection) =>
            collection == Collections.Stadium ? item : Custom.FromId(item.Id.Replace(Extension, $"{collection}{Extension}")) ?? item;

        public static Ident Ident(this Item item, Map map) => item.Ident ?? new Ident(item.Id, map.Challenge.MapInfo.Collection, item.Author);
    }
}
