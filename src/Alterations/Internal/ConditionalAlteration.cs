using System;
using tman.Core;
using tman.Maps;

namespace tman.Alterations.Internal
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
