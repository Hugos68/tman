using System;
using Tman.Core.Inventory;
using Tman.Core.Maps;

namespace Tman.Core.Alterations.Steps
{
    public class ConditionalAlteration(Predicate<Map> predicate, Alteration alteration) : Alteration
    {
        protected override void Run(Map map)
        {
            if (predicate(map))
            {
                alteration.Apply(map);
            }
        }
    }
}
