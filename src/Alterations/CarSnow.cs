using System.Collections.Generic;
using tman.src.Alterations.Internal;
using tman.src.Core;

namespace tman.src.Alterations
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