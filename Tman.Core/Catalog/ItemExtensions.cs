using System.Collections.Generic;
namespace Tman.Core.Catalog
{
    internal static class ItemExtensions
    {
        public static Item For(this Item item, Collections collection) => Custom.Skinned.GetValueOrDefault((item.Id, collection)) ?? item;
    }
}
