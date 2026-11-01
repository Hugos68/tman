using System.Collections.Generic;
using Tman.Core.Catalog;
using Tman.Core.Alterations.Steps;

namespace Tman.Core.Alterations.Cars
{
    public sealed class CarDesert : StandardAlteration
    {
        protected override string DisplayName => "Desert Car";

        protected override IEnumerable<Alteration> Steps =>
        [
            new CarChange(Car.Desert)
        ];
    }
}
