using System;
using tman.Core;
using tman.Maps;

namespace tman.Alterations.Internal
{
    public class MapNameEdited(Func<string, string> edit) : Alteration
    {
        protected override void Run(Map map)
        {
            map.Name = edit(map.Name);
        }
    }
}
