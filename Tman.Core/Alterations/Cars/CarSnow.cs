using System.Collections.Generic;
using Tman.Core.Alterations.Steps;
using Tman.Core.Catalog;

namespace Tman.Core.Alterations.Cars
{
    public class CarSnow : StandardAlteration
    {
        protected override string DisplayName => "Snow Car";

        protected override IEnumerable<Alteration> Steps =>
        [
            new CarChange(Car.Snow)
        ];
    }
}
