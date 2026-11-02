using System;
using Tman.Core.Maps;

namespace Tman.Core.Alterations.Steps
{
    internal sealed class ConditionalAlteration(Predicate<Map> predicate, Alteration alteration) : Alteration
    {
        protected override void Run(Map map, AlterationContext context)
        {
            if (predicate(map))
            {
                alteration.Apply(map, context);
            }
        }
    }
}
