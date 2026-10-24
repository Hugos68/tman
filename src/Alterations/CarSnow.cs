using System.Collections.Generic;
using tman.src.Alterations.Internal;

namespace tman.src.Alterations
{
    public class CarSnow : Alteration
    {
        protected override IEnumerable<Alteration> PreappliedAlterations =>
        [
            new ValidationRemoved(),
            new CarChange(Car.Snow),
            new MapNameEdited(name => name + " Car Snow")
        ];

    }
}