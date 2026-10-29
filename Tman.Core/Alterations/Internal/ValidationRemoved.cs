using Tman.Inventory;
using Tman.Maps;

namespace Tman.Alterations.Internal
{
    public class ValidationRemoved : Alteration
    {
        protected override void Run(Map map) => map.RemoveValidation();
    }
}
