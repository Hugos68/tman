using System.Collections.Generic;
using Tman.Alterations.Internal;
using Tman.Inventory;

namespace Tman.Alterations.Cars
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