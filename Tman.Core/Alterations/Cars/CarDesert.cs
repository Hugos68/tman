using System.Collections.Generic;
using Tman.Core.Inventory;
using Tman.Core.Alterations.Internal;

namespace Tman.Core.Alterations.Cars
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