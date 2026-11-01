using System.Collections.Generic;
using Tman.Core.Inventory;
using Tman.Core.Alterations.Steps;

namespace Tman.Core.Alterations.Cars
{
    public class CarDesert : StandardAlteration
    {
        protected override string DisplayName => "Desert Car";

        protected override IEnumerable<Alteration> Steps =>
        [
            new CarChange(Car.Desert)
        ];
    }
}
