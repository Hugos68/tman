using System.Collections.Generic;
using GBX.NET;

namespace Tman.Core.Inventory
{
    public static class CollectionIds
    {
        private static readonly Dictionary<Collections, int> Numbers = new()
        {
            [Collections.Stadium] = 26,
            [Collections.RedIsland] = 16,
            [Collections.WhiteShore] = 29,
            [Collections.GreenCoast] = 15,
            [Collections.BlueBay] = 28,
        };

        public static Id Of(Collections collection) => new(Numbers[collection]);
    }
}
