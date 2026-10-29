using System.Collections.Generic;
using Tman.Core.Alterations.Internal;
using Tman.Core.Inventory;

namespace Tman.Core.Alterations.Cars
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