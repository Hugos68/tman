using tman.Core;
using tman.Maps;

namespace tman.Alterations.Internal
{
    public class ValidationRemoved : Alteration
    {
        protected override void Run(Map map) => map.RemoveValidation();
    }
}
