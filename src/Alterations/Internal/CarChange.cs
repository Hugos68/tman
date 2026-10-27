using tman.src.Core;
using tman.src.Maps;

namespace tman.src.Alterations.Internal
{
    public class CarChange(Car car) : Alteration
    {
        protected override void Run(Map map)
        {
            map.Car = car;
        }
    }
}
