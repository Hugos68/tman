using System.Collections.Generic;
using Tman.Core.Alterations.Steps;
using Tman.Core.Catalog;

namespace Tman.Core.Alterations.Cars
{
    public class CarSport : StandardAlteration
    {
        protected override string DisplayName => "Sport Car";

        protected override IEnumerable<Alteration> Steps =>
        [
            new CarChange(Car.Sport)
        ];
    }
}
