using Tman.Core.Maps;

namespace Tman.Core.Alterations.Steps
{
    public class LightmapRemoved : Alteration
    {
        protected override void Run(Map map) => map.RemoveLightmap();
    }
}
