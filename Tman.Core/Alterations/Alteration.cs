using System.Collections.Generic;
using Tman.Core.Maps;

namespace Tman.Core.Alterations
{
    public abstract class Alteration
    {
        protected virtual IEnumerable<Alteration> PreappliedAlterations => [];

        public void Apply(Map map)
        {
            foreach (var alteration in PreappliedAlterations)
            {
                alteration.Apply(map);
            }
            Run(map);
            map.GenerateUid();
        }

        protected virtual void Run(Map map) { }
    }
}
