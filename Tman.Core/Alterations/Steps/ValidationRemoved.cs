using Tman.Core.Maps;

namespace Tman.Core.Alterations.Steps
{
    public class ValidationRemoved : Alteration
    {
        protected override void Run(Map map) => map.RemoveValidation();
    }
}
