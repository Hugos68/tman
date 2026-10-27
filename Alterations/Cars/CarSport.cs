using System.Collections.Generic;
using tman.Alterations.Internal;
using tman.Inventory;

namespace tman.Alterations.Cars
{
    public class CarSport : Alteration
    {
        protected override IEnumerable<Alteration> PreappliedAlterations =>
        [
            new ValidationRemoved(),
            new LightmapRemoved(),
            new CarChange(Car.Sport),
            new MapNameEdited(name => name + " Sport Car")
        ];

    }
}