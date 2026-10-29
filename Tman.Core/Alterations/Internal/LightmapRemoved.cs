using Tman.Inventory;
using Tman.Maps;

namespace Tman.Alterations.Internal
{
    public class LightmapRemoved : Alteration
    {
        protected override void Run(Map map) => map.RemoveLightmap();
    }
}
