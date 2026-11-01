using Tman.Core.Catalog;
using Tman.Core.Maps;

namespace Tman.Core.Alterations.Steps
{
    internal sealed class CarChange(Car car) : Alteration
    {
        protected override void Run(Map map)
        {
            map.Car = car;
        }
    }
}
