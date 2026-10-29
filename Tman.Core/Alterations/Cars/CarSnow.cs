using System.Collections.Generic;
using Tman.Alterations.Internal;
using Tman.Inventory;

namespace Tman.Alterations.Cars
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