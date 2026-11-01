using System;
using Tman.Core.Maps;

namespace Tman.Core.Alterations.Steps
{
    public class MapNameEdited(Func<string, string> edit) : Alteration
    {
        protected override void Run(Map map)
        {
            map.Name = edit(map.Name);
        }
    }
}
