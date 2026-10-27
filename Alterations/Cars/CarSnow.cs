using System.Collections.Generic;
using tman.Alterations.Internal;
using tman.Inventory;

namespace tman.Alterations.Cars
{
    public class CarSnow : Alteration
    {
        protected override IEnumerable<Alteration> PreappliedAlterations =>
        [
            new ValidationRemoved(),
            new LightmapRemoved(),
            new CarChange(Car.Snow),
            new MapNameEdited(name => name + " Snow Car")
        ];

    }
}