using Tman.Core.Inventory;
using Tman.Core.Maps;

namespace Tman.Core.Alterations.Internal
{
    public class CarChange(Car car) : Alteration
    {
        protected override void Run(Map map)
        {
            map.Car = car;
        }
    }
}
