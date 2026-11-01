using System;

namespace Tman.Core.Catalog
{
    [Flags]
    public enum Collections
    {
        Stadium = 1,
        RedIsland = 2,
        WhiteShore = 4,
        GreenCoast = 8,
        BlueBay = 16,
        All = 31
    }
}
