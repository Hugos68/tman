using System.Collections.Generic;
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
            Run(map, context);
            map.GenerateUid();
        }

        protected virtual void Run(Map map, AlterationContext context) { }
    }
}
