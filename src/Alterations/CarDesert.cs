using System.Collections.Generic;
using tman.src.Core;
using tman.src.Alterations.Internal;

namespace tman.src.Alterations
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