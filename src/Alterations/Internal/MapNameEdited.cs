using System;
using tman.src.Core;
using tman.Maps;

namespace tman.src.Alterations.Internal
{
    public class MapNameEdited(Func<string, string> edit) : Alteration
    {
        protected override void Run(Map map)
        {
            map.Name = edit(map.Name);
        }
    }
}
