using System;
using Tman.Core.Inventory;
using Tman.Core.Maps;

namespace Tman.Core.Alterations.Internal
{
    public class MapNameEdited(Func<string, string> edit) : Alteration
    {
        protected override void Run(Map map)
        {
            map.Name = edit(map.Name);
        }
    }
}
