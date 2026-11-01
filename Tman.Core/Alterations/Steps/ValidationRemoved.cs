using Tman.Core.Maps;

namespace Tman.Core.Alterations.Steps
{
    internal sealed class ValidationRemoved : Alteration
    {
        protected override void Run(Map map) => map.RemoveValidation();
    }
}
