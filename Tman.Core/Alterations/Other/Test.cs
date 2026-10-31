using System;
using System.Collections.Generic;
using Tman.Core.Alterations.Internal;
using Tman.Core.Maps;

namespace Tman.Core.Alterations.Other
{
    public class Test : Alteration
    {

        protected override IEnumerable<Alteration> PreappliedAlterations =>
        [
            new ValidationRemoved(),
            new LightmapRemoved(),
            new MapNameEdited(name => name + " Test")
        ];

        protected override void Run(Map map)
        {
            
        }
    }
}
