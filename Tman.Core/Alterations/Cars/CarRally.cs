using System.Collections.Generic;
using Tman.Inventory;
using Tman.Alterations.Internal;

namespace Tman.Alterations.Cars
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