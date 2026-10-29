using System.Collections.Generic;
using Tman.Core.Alterations.Internal;
using Tman.Core.Maps;

namespace Tman.Core.Alterations.Other
{
    public class Jitter : Alteration
    {
        protected override IEnumerable<Alteration> PreappliedAlterations =>
        [
            new ValidationRemoved(),
            new LightmapRemoved(),
            new MapNameEdited(name => name + " Jitter"),
        ];

        protected override void Run(Map map)
        {
            
        }
    }
}