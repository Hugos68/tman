using tman.Core;
using tman.Maps;

namespace tman.Alterations.Internal
{
    public class LightmapRemoved : Alteration
    {
        protected override void Run(Map map) => map.RemoveLightmap();
    }
}
