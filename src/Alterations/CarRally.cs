using System.Collections.Generic;
using tman.src.Core;
using tman.src.Alterations.Internal;

namespace tman.src.Alterations
{
    public class CarRally : Alteration
    {
        protected override IEnumerable<Alteration> PreappliedAlterations =>
        [
            new ValidationRemoved(),
            new LightmapRemoved(),
            new CarChange(Car.Rally),
            new MapNameEdited(name => name + " Rally Car")
        ];

    }
}