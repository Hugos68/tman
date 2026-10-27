using tman.src.Core;
using tman.Maps;

namespace tman.src.Alterations.Internal
{
    public class LightmapRemoved : Alteration
    {
        protected override void Run(Map map) => map.RemoveLightmap();
    }
}
