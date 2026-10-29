using System;
using Tman.Inventory;
using Tman.Maps;

namespace Tman.Alterations.Internal
{
    public class MapNameEdited(Func<string, string> edit) : Alteration
    {
        protected override void Run(Map map)
        {
            map.Name = edit(map.Name);
        }
    }
}
