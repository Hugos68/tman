using Tman.Core.Inventory;
using Tman.Core.Maps;

namespace Tman.Core.Alterations.Internal
{
    public class LightmapRemoved : Alteration
    {
        protected override void Run(Map map) => map.RemoveLightmap();
    }
}
