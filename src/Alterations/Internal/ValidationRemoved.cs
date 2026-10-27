using tman.src.Core;
using tman.src.Maps;

namespace tman.src.Alterations.Internal
{
    public class ValidationRemoved : Alteration
    {
        protected override void Run(Map map) => map.RemoveValidation();
    }
}
