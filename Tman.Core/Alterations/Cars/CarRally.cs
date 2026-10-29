using System.Collections.Generic;
using Tman.Core.Inventory;
using Tman.Core.Alterations.Internal;

namespace Tman.Core.Alterations.Cars
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