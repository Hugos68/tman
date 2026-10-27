using tman.Core;
using tman.Maps;

namespace tman.Alterations.Internal
{
    public class CarChange(Car car) : Alteration
    {
        protected override void Run(Map map)
        {
            map.Car = car;
        }
    }
}
