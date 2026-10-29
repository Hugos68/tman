using System.Collections.Generic;
using Tman.Inventory;
using Tman.Alterations.Internal;

namespace Tman.Alterations.Cars
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