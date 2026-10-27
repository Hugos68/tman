using System.Collections.Generic;
using tman.Inventory;
using tman.Alterations.Internal;

namespace tman.Alterations.Cars
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