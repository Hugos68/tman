using Tman.Inventory;
using Tman.Maps;

namespace Tman.Alterations.Internal
{
    public class CarChange(Car car) : Alteration
    {
        protected override void Run(Map map)
        {
            map.Car = car;
        }
    }
}
