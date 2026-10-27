using System.Collections.Generic;
using tman.Alterations.Internal;
using tman.Core;

namespace tman.Alterations
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