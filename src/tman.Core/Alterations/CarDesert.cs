using System.Collections.Generic;
using tman.Alterations.Internal;
using tman.Core;

namespace tman.Alterations
{
    public class CarDesert : Alteration
    {
        protected override IEnumerable<Alteration> PreappliedAlterations =>
        [
            new ValidationRemoved(),
            new LightmapRemoved(),
            new CarChange(Car.Desert),
            new MapNameEdited(name => name + " Desert Car")
        ];

    }
}