using System.Collections.Generic;
using tman.src.Alterations.Internal;
using tman.src.Core;

namespace tman.src.Alterations
{
    public class CarDesert : Alteration
    {
        protected override IEnumerable<Alteration> PreappliedAlterations =>
        [
            new ValidationRemoved(),
            new CarChange(Car.Desert),
            new MapNameEdited(name => name + " Desert Car")
        ];

    }
}