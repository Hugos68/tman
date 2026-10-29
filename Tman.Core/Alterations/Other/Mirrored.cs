using System.Collections.Generic;
using System.Linq;
using Tman.Core.Alterations.Internal;
using Tman.Core.Maps;

namespace Tman.Core.Alterations.Other
{
    public class Mirrored : Alteration
    {
        protected override IEnumerable<Alteration> PreappliedAlterations =>
        [
            new ValidationRemoved(),
            new LightmapRemoved(),
            new MapNameEdited(name => name + " Mirrored")
        ];

        protected override void Run(Map map)
        {
            foreach (var placement in map.Placements.Concat(map.Baked).ToList())
            {
                map.Mirror(placement);
            }
        }
    }
}
