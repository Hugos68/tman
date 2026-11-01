using Tman.Core.Catalog;
using Tman.Core.Maps;

namespace Tman.Core.Alterations.Steps
{
    public class CarChange(Car car) : Alteration
    {
        protected override void Run(Map map)
        {
            map.Car = car;
        }
    }
}
