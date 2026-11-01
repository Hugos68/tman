using System.Collections.Generic;
using Tman.Core.Inventory;
using Tman.Core.Alterations.Steps;

namespace Tman.Core.Alterations.Cars
{
    public class CarRally : StandardAlteration
    {
        protected override string DisplayName => "Rally Car";

        protected override IEnumerable<Alteration> Steps =>
        [
            new CarChange(Car.Rally)
        ];
    }
}
