using Tman.Core.Catalog;
using Tman.Core.Maps;

namespace Tman.Core.Alterations.Steps
{
    internal sealed class CarChanged(Car car) : Alteration
    {
        protected override void Run(Map map, AlterationContext context)
        {
            map.Car = car;
        }
    }
}
