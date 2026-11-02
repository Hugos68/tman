using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Tman.Core.Maps;

namespace Tman.Core.Alterations
{
    internal abstract class Alteration
    {
        protected virtual IEnumerable<Alteration> PreappliedAlterations => [];

        internal void Apply(Map map, AlterationContext context)
        {
            foreach (var alteration in PreappliedAlterations)
            {
                alteration.Apply(map, context);
            }
            context.Logger.LogDebug("{Step}", GetType().Name);
            Run(map, context);
            map.GenerateUid();
        }

        protected virtual void Run(Map map, AlterationContext context) { }
    }
}
